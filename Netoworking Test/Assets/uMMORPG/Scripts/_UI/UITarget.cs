// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

public partial class UITarget : MonoBehaviour {
    public GameObject panel;
    public Slider healthSlider;
    public Text nameText;
    public Button tradeButton;
    public Button guildInviteButton;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        if (player.target != null && player.target != player) {
            // name and health
            panel.SetActive(true);
            healthSlider.value = player.target.HealthPercent();
            nameText.text = player.target.name;

            // trade button
            if (player.target is Player) {
                tradeButton.gameObject.SetActive(true);
                tradeButton.interactable = player.CanStartTradeWith(player.target);
                tradeButton.onClick.SetListener(() => {
                    player.CmdTradeRequestSend();
                });
            } else tradeButton.gameObject.SetActive(false);

            // guild invite button
            if (player.InGuild() && player.target is Player) {
                guildInviteButton.gameObject.SetActive(true);
                guildInviteButton.interactable = player.CanGuildInvite((Player)player.target);
                guildInviteButton.onClick.SetListener(() => {
                    player.CmdGuildInviteTarget();
                });
            } else guildInviteButton.gameObject.SetActive(false);
        } else panel.SetActive(false); // hide

        // addon system hooks
        Utils.InvokeMany(typeof(UITarget), this, "Update_");
    }
}
