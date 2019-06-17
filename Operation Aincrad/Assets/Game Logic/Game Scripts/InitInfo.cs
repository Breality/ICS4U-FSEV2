using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * InitInfo.cs
 * Description: Main use to update the username across all clients as that is the unique identifier used to distinguish interactions across server.
 */
public class InitInfo : NetworkBehaviour
{
    private InfoCenter inCen;//used to log in a player when they load in and receive username
    private HTTPClient handlerMan;//used to get information needed for login from another script
    private UpdatePlayer upHandler;//used to update equipment of player on login so all others see their equipment
    [SyncVar]
    string userName = "";//sync variable of username across all server and clients
    void Awake()
    {
        //Get the necessary script components from hierachy searches
        upHandler = this.GetComponent<UpdatePlayer>();
        inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        handlerMan = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();

    }
    private void Start()
    {
        if (isLocalPlayer)//if the current player prefab represents the client, set stats and update. This is called when client loads in. 
        {
            string cur_user = inCen.LogIn(handlerMan.GetLoadedD(), handlerMan.GetLoadedEquip());//get username
            userName = cur_user;//sync it on client
            CmdSendName(cur_user);//send to server to sync on all other clients as well
            upHandler.UpdateEquip();//update player equipment as they just logged in
        }
    }
    private void Update()
    {
        this.transform.parent.name = userName;//set username of this player to change to synced variable
    }
    [Command]
    void CmdSendName(string user)//send server command to change username (server's variable is shared across all clients)
    {
        userName = user;//set username to sync across all methods.
        this.transform.parent.name = user;//change the name on the server.
    }

}
