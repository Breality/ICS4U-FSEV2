// We implemented a chat system that works directly with UNET. The chat supports
// different channels that can be used to communicate with other players:
//
// - **Local Chat:** by default, all messages that don't start with a **/** are
// addressed to the local chat. If one player writes a local message, then all
// players around him _(all observers)_ will be able to see the message.
// - **Whisper Chat:** a player can write a private message to another player by
// using the **/ name message** format.
// - **Guild Chat:** we implemented guild chat support with the **/g message**
// - **Info Chat:** the info chat can be used by the server to notify all
// players about important news. The clients won't be able to write any info
// messages.
//
// _Note: the channel names, colors and commands can be edited in the Inspector_
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ChannelInfo {
    public string command; // /w etc.
    public string identifierOut; // for sending
    public string identifierIn; // for receiving
    public Color color;

    public ChannelInfo(string command, string identifierOut, string identifierIn, Color color) {
        this.command = command;
        this.identifierOut = identifierOut;
        this.identifierIn = identifierIn;
        this.color = color;
    }
}

[System.Serializable]
public class MessageInfo {
    public string content; // the actual message
    public string replyPrefix; // copied to input when clicking the message
    public Color color;

    public MessageInfo(string sender, string identifier, string message, string replyPrefix, Color color) {
        // construct the message (we don't really need to save all the parts,
        // also this will save future computations)
        content = "<b>" + sender + identifier + ":</b> " + message;
        this.replyPrefix = replyPrefix;
        this.color = color;
    }
}

[NetworkSettings(channel=Channels.DefaultUnreliable)]
public partial class Chat : NetworkBehaviour {
    // channels
    [Header("Channels")]
    public ChannelInfo whisper = new ChannelInfo("/w", "(TO)", "(FROM)", Color.magenta);
    public ChannelInfo local = new ChannelInfo("", "", "", Color.white);
    public ChannelInfo guild = new ChannelInfo("/g", "(Guild)", "(Guild)", Color.cyan);
    public ChannelInfo info = new ChannelInfo("", "(Info)", "(Info)", Color.red);

    [Header("Other")]
    public int maxLength = 70;

    public override void OnStartLocalPlayer() {
        // test messages
        AddMessage(new MessageInfo("", info.identifierIn, "Just type a message here to chat!", "", info.color));
        //AddMessage(new MessageInfo("", info.identifierIn, "  Use /g for guild chat", "",  info.color));
        AddMessage(new MessageInfo("", info.identifierIn, "  Use /w NAME to whisper a player", "",  info.color));
        AddMessage(new MessageInfo("", info.identifierIn, "  Or click on a message to reply", "",  info.color));
        AddMessage(new MessageInfo("Someone", guild.identifierIn, "Anyone here?", "/g ",  guild.color));
        AddMessage(new MessageInfo("Someone", whisper.identifierIn, "Are you there?", "/w Someone ",  whisper.color));
        AddMessage(new MessageInfo("Someone", local.identifierIn, "Hello!", "",  local.color));

        // addon system hooks
        Utils.InvokeMany(typeof(Chat), this, "OnStartLocalPlayer_");
    }

    // submit tries to send the string and then returns the new input text
    [Client]
    public string OnSubmit(string text) {
        // not empty and not only spaces?
        if (!Utils.IsNullOrWhiteSpace(text)) {
            // command in the commands list?
            // note: we don't do 'break' so that one message could potentially
            //       be sent to multiple channels (see mmorpg local chat)
            string lastcommand = "";
            if (text.StartsWith(whisper.command)) {
                // whisper
                string[] parsed = ParsePM(whisper.command, text);
                string user = parsed[0];
                string msg = parsed[1];
                if (!Utils.IsNullOrWhiteSpace(user) && !Utils.IsNullOrWhiteSpace(msg)) {
                    if (user != name) {
                        lastcommand = whisper.command + " " + user + " ";
                        CmdMsgWhisper(user, msg);
                    } else print("cant whisper to self");
                } else print("invalid whisper format: " + user + "/" + msg);
            } else if (!text.StartsWith("/")) {
                // local chat is special: it has no command
                lastcommand = "";
                CmdMsgLocal(text);
            } else if (text.StartsWith(guild.command)) {
                // guild
                string msg = ParseGeneral(guild.command, text);
                if (!Utils.IsNullOrWhiteSpace(msg)) {
                    lastcommand = guild.command + " ";
                    CmdMsgGuild(msg);
                }
            }

            // addon system hooks
            Utils.InvokeMany(typeof(Chat), this, "OnSubmit_", text);

            // input text should be set to lastcommand
            return lastcommand;
        }

        // input text should be cleared
        return "";
    }

    [Client]
    void AddMessage(MessageInfo mi) {
        FindObjectOfType<UIChat>().AddMessage(mi);
    }

    // parse a message of form "/command message"
    static string ParseGeneral(string command, string msg) {
		// return message without command prefix (if any)
        return msg.StartsWith(command + " ") ? msg.Substring(command.Length + 1) : "";
    }

    static string[] ParsePM(string command, string pm) {
        // parse to /w content
        string content = ParseGeneral(command, pm);

        // now split the content in "user msg"
        if (content != "") {
            // find the first space that separates the name and the message
            int i = content.IndexOf(" ");
            if (i >= 0) {
                string user = content.Substring(0, i);
                string msg = content.Substring(i+1);
                return new string[] {user, msg};
            }
        }
        return new string[] {"", ""};
    }

    // networking //////////////////////////////////////////////////////////////
    [Command(channel=Channels.DefaultUnreliable)] // unimportant => unreliable
    void CmdMsgLocal(string message) {
        if (message.Length > maxLength) return;

        // it's local chat, so let's send it to all observers via ClientRpc
        RpcMsgLocal(name, message);
    }

    [Command(channel=Channels.DefaultUnreliable)] // unimportant => unreliable
    void CmdMsgGuild(string message) {
        if (message.Length > maxLength) return;

        // send message to all online guild members
        foreach (var player in GetComponent<Player>().FindOnlineGuildMembers()) {
            var chat = player.GetComponent<Chat>();
            if (chat != null) {
                chat.TargetMsgGuild(chat.connectionToClient, name, message);
            }
        }
    }

    [Command(channel=Channels.DefaultUnreliable)] // unimportant => unreliable
    void CmdMsgWhisper(string playerName, string message) {
        if (message.Length > maxLength) return;

        // find the player with that name
        if (Player.onlinePlayers.ContainsKey(playerName)) {
            var chat = Player.onlinePlayers[playerName].GetComponent<Chat>();
            // receiver gets a 'from' message, sender gets a 'to' message
            chat.TargetMsgWhisperFrom(chat.connectionToClient, name, message);
            TargetMsgWhisperTo(connectionToClient, playerName, message);
        }
    }

    // message handlers ////////////////////////////////////////////////////////
    [TargetRpc(channel=Channels.DefaultUnreliable)] // only send to one client
    public void TargetMsgWhisperFrom(NetworkConnection target, string sender, string message) {
        // add message with identifierIn
        string identifier = whisper.identifierIn;
        string reply = whisper.command + " " + sender + " "; // whisper
        AddMessage(new MessageInfo(sender, identifier, message, reply, whisper.color));
    }

    [TargetRpc(channel=Channels.DefaultUnreliable)] // only send to one client
    public void TargetMsgWhisperTo(NetworkConnection target, string receiver, string message) {
        // add message with identifierOut
        string identifier = whisper.identifierOut;
        string reply = whisper.command + " " + receiver + " "; // whisper
        AddMessage(new MessageInfo(receiver, identifier, message, reply, whisper.color));
    }

    [ClientRpc(channel=Channels.DefaultUnreliable)] // send to observers
    public void RpcMsgLocal(string sender, string message) {
        // add message with identifierIn or Out depending on who sent it
        string identifier = sender != name ? local.identifierIn : local.identifierOut;
        string reply = whisper.command + " " + sender + " "; // whisper
        AddMessage(new MessageInfo(sender, identifier, message, reply, local.color));
    }

    [TargetRpc(channel=Channels.DefaultUnreliable)] // only send to one client
    public void TargetMsgGuild(NetworkConnection target, string sender, string message) {
        string reply = whisper.command + " " + sender + " "; // whisper
        AddMessage(new MessageInfo(sender, guild.identifierIn, message, reply, guild.color));
    }

    [TargetRpc(channel=Channels.DefaultUnreliable)] // only send to one client
    public void TargetMsgInfo(NetworkConnection target, string message) {
        AddMessage(new MessageInfo("", info.identifierIn, message, "", info.color));
    }
}
