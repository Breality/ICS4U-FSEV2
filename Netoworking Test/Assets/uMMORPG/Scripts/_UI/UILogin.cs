﻿// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public partial class UILogin : MonoBehaviour {
    public NetworkManagerMMO manager; // singleton=null in Start/Awake
    public GameObject panel;
    public Text statusText;
    public InputField accountInput;
    public InputField passwordInput;
    public Dropdown serverDropdown;
    public Button loginButton;
    public Button registerButton;
    [TextArea(1, 30)] public string registerMessage = "First time? Just log in and we will\ncreate an account automatically.";
    public Button hostButton;
    public Button dedicatedButton;
    public Button cancelButton;
    public Button quitButton;

    void Start() {
        // load last server by name in case order changes some day.
        if (PlayerPrefs.HasKey("LastServer")) {
            string last = PlayerPrefs.GetString("LastServer", "");
            serverDropdown.value = manager.serverList.FindIndex(s => s.name == last);
        }
    }

    void OnDestroy() {
        // save last server by name in case order changes some day
        PlayerPrefs.SetString("LastServer", serverDropdown.captionText.text);
    }

    void Update() {
        // only update while visible
        if (!panel.activeSelf) return;

        // status
        statusText.text = manager.IsConnecting() ? "Connecting..." : "";

        // buttons. interactable while network is not active
        // (using IsConnecting is slightly delayed and would allow multiple clicks)
        registerButton.onClick.SetListener(() => { FindObjectOfType<UIPopup>().Show(registerMessage); });
        loginButton.interactable = !manager.isNetworkActive;
        loginButton.onClick.SetListener(() => { manager.StartClient(); });
        hostButton.interactable = !manager.isNetworkActive;
        hostButton.onClick.SetListener(() => { manager.StartHost(); });
        cancelButton.gameObject.SetActive(manager.isNetworkActive);
        cancelButton.onClick.SetListener(() => { manager.StopClient(); });
        dedicatedButton.interactable = !manager.isNetworkActive;
        dedicatedButton.onClick.SetListener(() => { manager.StartServer(); });
        quitButton.onClick.SetListener(() => { Application.Quit(); });

        // inputs
        manager.loginAccount = accountInput.text;
        manager.loginPassword = passwordInput.text;

        // copy servers to dropdown; copy selected one to networkmanager ip/port.
        serverDropdown.interactable = !manager.isNetworkActive;
        serverDropdown.options = manager.serverList.Select(
            sv => new Dropdown.OptionData(sv.name)
        ).ToList();
        manager.networkAddress = manager.serverList[serverDropdown.value].ip;

        // addon system hooks
        Utils.InvokeMany(typeof(UILogin), this, "Update_");
    }

    public void Show() { panel.SetActive(true); }
    public void Hide() { panel.SetActive(false); }
}
