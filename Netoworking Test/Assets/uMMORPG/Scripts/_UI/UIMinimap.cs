﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public partial class UIMinimap : MonoBehaviour {
    public GameObject panel;
    public float zoomMin = 5;
    public float zoomMax = 50;
    public float zoomStepSize = 5;
    public Text sceneText;
    public Button plusButton;
    public Button minusButton;
    public Camera minimapCamera;

    void Start() {
        plusButton.onClick.SetListener(() => {
            minimapCamera.orthographicSize = Mathf.Max(minimapCamera.orthographicSize - zoomStepSize, zoomMin);
        });
        minusButton.onClick.SetListener(() => {
            minimapCamera.orthographicSize = Mathf.Min(minimapCamera.orthographicSize + zoomStepSize, zoomMax);
        });
    }

    void Update() {
        var player = Utils.ClientLocalPlayer();
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;

        sceneText.text = SceneManager.GetActiveScene().name;

        // addon system hooks
        Utils.InvokeMany(typeof(UIMinimap), this, "Update_");
    }
}
