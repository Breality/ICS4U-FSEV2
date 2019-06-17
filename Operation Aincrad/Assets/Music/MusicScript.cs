using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour
{
    [SerializeField]
    private AudioListener menuListen, gameListen;
    [SerializeField]
    private AudioSource menuPlayer, gamePlayer;
    private void OnDisable()
    {
        menuListen.enabled = false;
        menuPlayer.enabled = false;
        gameListen.enabled = true;
        gamePlayer.enabled = true;
    }
}
