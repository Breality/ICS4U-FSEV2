using System.Collections;
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

    [SerializeField]
    private ShopToggle stMan1, stMan2;
    private InfoCenter inCen;
    private HTTPClient handlerMan;
    string userName;
    void Awake()
    {
        inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        inCen.MoneyText = moneyText;
        inCen.WeaponsL = WeaponsLeft;
        inCen.WeaponsR = WeaponsRight;
        inCen.NameText = nameText;


        JoyStickListen jsL = this.GetComponent<JoyStickListen>();
        jsL.sellers = GameObject.Find("Sellers");

        handlerMan = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();
        stMan1.info = stMan2.info = inCen;
        stMan1.HTTP = stMan2.HTTP = handlerMan;
        //NetworkServer.Spawn(this.gameObject)


    }
    private void Start()
    {
        if (isLocalPlayer)
        {
            this.transform.parent.name = inCen.LogIn(handlerMan.GetLoadedD(), handlerMan.GetLoadedEquip());
            CmdSendName(this.transform.parent.name);
        }
    }
    [Command]
    public void CmdSendName(string username)
    {
        RpcReceiveName(username);
        this.transform.parent = username;
    }
    [ClientRpc]
    public void RpcReceiveName(string username)
    {
        if (!isLocalPlayer)
        {
            this.transform.parent.name = username;
        }
    }

}
