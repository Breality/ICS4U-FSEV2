// Guilds have to be structs in order to work with SyncLists.
using UnityEngine.Networking;

public class GuildRank {
    public string name;

    // permissions
    public bool canInvite;
    public bool canKick;
    public bool canNotify;

    public GuildRank(string name, bool canInvite, bool canKick, bool canNotify) {
        this.name = name;
        this.canInvite = canInvite;
        this.canKick = canKick;
        this.canNotify = canNotify;
    }
}

[System.Serializable]
public partial struct GuildMember {
    // basic info
    public string name;
    public int level;
    public bool online;
    public int rankIndex;

    public bool isMaster { get { return rankIndex == ranks.Length - 1; } }
    public GuildRank rank { get { return ranks[rankIndex]; } }

    public static GuildRank[] ranks = {new GuildRank("Member", false, false, false),
                                       new GuildRank("Vice", true, true, true),
                                       new GuildRank("Master", true, true, true)};
}

public class SyncListGuildMember : SyncListStruct<GuildMember> {}