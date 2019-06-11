using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class UIItemMall : MonoBehaviour {
    public KeyCode hotKey = KeyCode.X;
    public GameObject panel;
    public Button categorySlotPrefab;
    public Transform categoryContent;
    public UIItemMallSlot itemSlotPrefab;
    public Transform itemContent;
    public string currencyName = "Coins";
    public string buyUrl = "http://unity3d.com/";
    public Color priceColor = Color.yellow;
    int currentCategory = 0;
    public Text nameText;
    public Text levelText;
    public Text currencyNameText;
    public Text currencyAmountText;
    public Button buyButton;
    public InputField couponInput;
    public Button couponButton;
    public GameObject inventoryPanel;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        // only update the panel if it's active
        if (panel.activeSelf) {
            // find all the items that should be sold in the item mall
            var items = (from kvp in ItemTemplate.dict
                         where kvp.Value.itemMallPrice > 0
                         select kvp.Value).ToList();

            // find the needed categories
            var categories = items.Select(item => Utils.ParseFirstNoun(item.category)).Distinct().ToList();

            // instantiate/destroy enough category slots
            UIUtils.BalancePrefabs(categorySlotPrefab.gameObject, categories.Count, categoryContent);

            // refresh all category buttons
            for (int i = 0; i < categories.Count; ++i) {
                var button = categoryContent.GetChild(i).GetComponent<Button>();
                button.interactable = i != currentCategory;
                button.GetComponentInChildren<Text>().text = categories[i];
                int icopy = i;
                button.onClick.SetListener(() => { currentCategory = icopy; });
            }

            if (categories.Count > 0) {
                // instantiate/destroy enough item slots for that category
                var categoryItems = items.Where(
                    item => Utils.ParseFirstNoun(item.category) == categories[currentCategory]
                ).ToList();
                UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, categoryItems.Count, itemContent);

                // refresh all items in that category
                for (int i = 0; i < categoryItems.Count; ++i) {
                    var slot = itemContent.GetChild(i).GetComponent<UIItemMallSlot>();
                    var item = categoryItems[i];

                    // refresh item
                    slot.tooltip.text = new Item(item).ToolTip();
                    slot.image.color = Color.white;
                    slot.image.sprite = item.image;
                    slot.descriptionText.text = item.name + "\n\n<b><color=#" + ColorUtility.ToHtmlStringRGBA(priceColor) + ">" + item.itemMallPrice + "</color></b> " + currencyName;
                    slot.unlockButton.interactable = player.health > 0 && player.coins >= item.itemMallPrice;
                    slot.unlockButton.onClick.SetListener(() => {
                        inventoryPanel.SetActive(true); // better feedback
                        player.CmdUnlockItem(item.name);
                    });
                }
            }

            // overview
            nameText.text = player.name;
            levelText.text = "Lv. " + player.level;
            currencyNameText.text = currencyName;
            currencyAmountText.text = player.coins.ToString();
            buyButton.GetComponentInChildren<Text>().text = "Buy " + currencyName;
            buyButton.onClick.SetListener(() => { Application.OpenURL(buyUrl); });
            couponInput.interactable = NetworkTime.time >= player.nextRiskyActionTime;
            couponButton.interactable = NetworkTime.time >= player.nextRiskyActionTime;
            couponButton.onClick.SetListener(() => {
                if (!Utils.IsNullOrWhiteSpace(couponInput.text))
                    player.CmdEnterCoupon(couponInput.text);
                couponInput.text = "";
            });
        }

        // addon system hooks
        Utils.InvokeMany(typeof(UIItemMall), this, "Update_");
    }
}
