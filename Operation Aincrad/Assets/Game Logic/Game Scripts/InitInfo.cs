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


    private InfoCenter inCen;
    private HTTPClient handlerMan;
    [SyncVar]
    string userName = "";
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

        //NetworkServer.Spawn(this.gameObject)


    }
    private void Start()
    {
        if (isLocalPlayer)
        {
            string cur_user = inCen.LogIn(handlerMan.GetLoadedD(), handlerMan.GetLoadedEquip());
            CmdSendName(cur_user);
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
