using UnityEngine;
using UnityEngine.UI;

public partial class UINpcGuildManagement : MonoBehaviour {
    public GameObject panel;
    public Text createPriceText;
    public InputField createNameInput;
    public Button createButton;
    public Button terminateButton;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // use collider point(s) to also work with big entities
        if (player.target != null && player.target is Npc &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange) {
            createNameInput.interactable = !player.InGuild() &&
                                           player.gold >= Player.guildCreationPrice;
            createNameInput.characterLimit = Player.guildNameMaxLength;

            createPriceText.text = Player.guildCreationPrice.ToString();

            createButton.interactable = player.CanCreateGuild(createNameInput.text);
            createButton.onClick.SetListener(() => {
                player.CmdCreateGuild(createNameInput.text);
            });

            terminateButton.interactable = player.CanTerminateGuild();
            terminateButton.onClick.SetListener(() => {
                player.CmdTerminateGuild();
            });
        } else panel.SetActive(false); // hide

        // addon system hooks
        Utils.InvokeMany(typeof(UINpcGuildManagement), this, "Update_");
    }
}
