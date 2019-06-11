using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class UIGuild : MonoBehaviour {
    public KeyCode hotKey = KeyCode.G;
    public GameObject panel;
    public Text nameText;
    public Text masterText;
    public Text currentCapacityText;
    public Text maximumCapacityText;
    public InputField noticeInput;
    public Button noticeEditButton;
    public Button noticeSetButton;
    public UIGuildMemberSlot slotPrefab;
    public Transform memberContent;
    public Color onlineColor = Color.cyan;
    public Color offlineColor = Color.gray;
    public Button leaveButton;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        // only update the panel if it's active
        if (panel.activeSelf) {
            var master = player.guildMembers.ToList().Find(m => m.isMaster);

            // guild properties
            nameText.text = player.guild;
            masterText.text = master.name;
            currentCapacityText.text = player.guildMembers.Count.ToString();
            maximumCapacityText.text = Player.guildCapacity.ToString();

            // notice edit button
            noticeEditButton.interactable = player.CanGuildNotify() &&
                                            !noticeInput.interactable;
            noticeEditButton.onClick.SetListener(() => {
                noticeInput.interactable = true;
            });

            // notice set button
            noticeSetButton.interactable = player.CanGuildNotify() &&
                                           noticeInput.interactable &&
                                           NetworkTime.time >= player.nextRiskyActionTime;
            noticeSetButton.onClick.SetListener(() => {
                noticeInput.interactable = false;
                if (noticeInput.text.Length > 0 &&
                    !Utils.IsNullOrWhiteSpace(noticeInput.text)) {
                    player.CmdSetGuildNotice(noticeInput.text);
                }
            });

            // notice input: copies notice while not editing it
            if (!noticeInput.interactable) noticeInput.text = player.guildNotice;
            noticeInput.characterLimit = Player.guildNoticeMaxLength;

            // leave
            leaveButton.interactable = player.CanLeaveGuild();
            leaveButton.onClick.SetListener(() => {
                player.CmdLeaveGuild();
            });

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.guildMembers.Count, memberContent);

            // refresh all members
            for (int i = 0; i < player.guildMembers.Count; ++i) {
                var slot = memberContent.GetChild(i).GetComponent<UIGuildMemberSlot>();
                var member = player.guildMembers[i];

                slot.onlineStatusImage.color = member.online ? onlineColor : offlineColor;
                slot.nameText.text = member.name;
                slot.levelText.text = member.level.ToString();
                slot.rankText.text = member.rank.name;
                slot.promoteButton.interactable = player.CanGuildPromote(member);
                slot.promoteButton.onClick.SetListener(() => {
                    player.CmdGuildPromote(member.name);
                });
                slot.demoteButton.interactable = player.CanGuildDemote(member);
                slot.demoteButton.onClick.SetListener(() => {
                    player.CmdGuildDemote(member.name);
                });
                slot.kickButton.interactable = player.CanKickFromGuild(member);
                slot.kickButton.onClick.SetListener(() => {
                    player.CmdGuildKick(member.name);
                });
            }
        }

        // addon system hooks
        Utils.InvokeMany(typeof(UIGuild), this, "Update_");
    }
}
