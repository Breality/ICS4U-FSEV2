﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
public class InitInfo : NetworkBehaviour
{
    [SerializeField]
    private GameObject WeaponsLeft, WeaponsRight;
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private TextMesh moneyText;


    private InfoCenter inCen;
    private HTTPClient handlerMan;
    private UpdatePlayer upHandler;
    [SyncVar]
    string userName = "";
    void Start()
    {
        Debug.Log(2);
        upHandler = this.GetComponent<UpdatePlayer>();
        inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        inCen.MoneyText = moneyText;
        inCen.WeaponsL = WeaponsLeft;
        inCen.WeaponsR = WeaponsRight;
        inCen.NameText = nameText;


        JoyStickListen jsL = this.GetComponent<JoyStickListen>();
        jsL.sellers = GameObject.Find("Sellers");

        handlerMan = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();

        if (isLocalPlayer)
        {
            string cur_user = inCen.LogIn(handlerMan.GetLoadedD(), handlerMan.GetLoadedEquip());
            Debug.Log(cur_user);
            userName = cur_user;
            CmdSendName(cur_user);
            upHandler.UpdateEquip();
        }
    }
    private void Update()
    {
        this.transform.parent.name = userName;
    }
    [Command]
    void CmdSendName(string user)
    {
        userName = user;
        this.transform.parent.name = user;
    }

}
