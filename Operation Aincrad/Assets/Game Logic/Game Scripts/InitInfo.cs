using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
public class InitInfo : NetworkBehaviour
{



    private InfoCenter inCen;
    private HTTPClient handlerMan;
    private UpdatePlayer upHandler;
    [SyncVar]
    string userName = "";
    void Awake()
    {
        upHandler = this.GetComponent<UpdatePlayer>();
        inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();

        handlerMan = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();

        //NetworkServer.Spawn(this.gameObject)


    }
    private void Start()
    {
        if (isLocalPlayer)
        {
            string cur_user = inCen.LogIn(handlerMan.GetLoadedD(), handlerMan.GetLoadedEquip());
            Debug.Log(cur_user);
            userName = cur_user;
            CmdSendName(cur_user);
            
        }
        upHandler.OnJoinGame();
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
