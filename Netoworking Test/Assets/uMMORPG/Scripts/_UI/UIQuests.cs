// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public partial class UIQuests : MonoBehaviour {
    public KeyCode hotKey = KeyCode.Q;
    public GameObject panel;
    public Transform content;
    public UIQuestSlot slotPrefab;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        // only update the panel if it's active
        if (panel.activeSelf) {
            // only show active quests, no completed ones
            var activeQuests = player.quests.Where(q => !q.completed).ToList();

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, activeQuests.Count, content);

            // refresh all
            for (int i = 0; i < activeQuests.Count; ++i) {
                var slot = content.GetChild(i).GetComponent<UIQuestSlot>();
                var quest = activeQuests[i];
                int gathered = quest.gatherName != "" ? player.InventoryCountAmount(quest.gatherName) : 0;
                slot.descriptionText.text = quest.ToolTip(gathered);
            }
        }

        // addon system hooks
        Utils.InvokeMany(typeof(UIQuests), this, "Update_");
    }
}
