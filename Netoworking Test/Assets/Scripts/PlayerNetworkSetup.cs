using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlayerNetworkSetup : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        foreach(Renderer ren in rends)
        {
            ren.enabled = false;
        }

        GetComponent<NetworkAnimator>().
    }
}
