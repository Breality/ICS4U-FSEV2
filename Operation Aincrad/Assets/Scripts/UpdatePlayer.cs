using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * UpdatePlayer.cs
 * Description: Updates Player Equipment Across server so all users see same characters with same equipment.
 */
public class UpdatePlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Transform SwordL, SwordR, Armor, BootL, BootR, Pendants, Helmets;//all transforms that hold equipment
    private Transform[] possEquip;//a list that holds all of the transforms or easy loop
    private string[] curEquip = new string[7];
    private void Awake()//it is instantiated in awake because other functions on start call this function so possEquip needs to be initialized quickly. Therefore, awake is used.
    {
        possEquip = new Transform[7] { SwordL, SwordR, Armor, BootL, BootR, Pendants, Helmets }; //hold  transforms for easy loop    
    }
    private void ClearCA()
    {
        for(int i = 0; i<curEquip.Length; i++)//clears the current equipment of user
        {
            curEquip[i] = null;
        }
    }
    public void UpdateEquip()//Function called to update all equipment on clients across the server
    {
       ClearCA();//first clears all equipment that was previously active
       for(int curEquipInd = 0; curEquipInd<possEquip.Length; curEquipInd++)//loop through all equipment to check what is still active
        {
            GetActiveEquip(possEquip[curEquipInd], curEquipInd);//Get active equipment per equipment type
        }
        CmdUpdatePlayer(curEquip);//send a command to server to update clients

    }
    private void GetActiveEquip(Transform EquipHolder, int curEquipInd)//get active equipment in specific equipment type
    {
        foreach(Transform child in EquipHolder)//loop through equipment in specific holder
        {
            if (child.gameObject.activeSelf)//if one is active set it to be the current one active
            {
                curEquip[curEquipInd] = child.name;
                return;//there will only be one at a specific moment.
            }
        }
    }
    public void SetEquipAct(string[] curAct)//Sets active equipment given a list of the current ones active
    {
        for (int curEquipIndex = 0; curEquipIndex < possEquip.Length; curEquipIndex++)//for each equipment type
        {
            foreach (Transform weapon in possEquip[curEquipIndex])//loop to find the one that is currently active
            {
                if (weapon.name == curAct[curEquipIndex])//when found set it to active so client can see it
                {
                    weapon.gameObject.SetActive(true);
                }
                else//otherwise set to false so client does not see it
                {
                    weapon.gameObject.SetActive(false);
                }
            }
        }
    }
    [Command]
    public void CmdUpdatePlayer(string[] curAct)//send command to set the equipment of player on server and update on all clients
    {
        SetEquipAct(curAct);//sets equipment on server as well
        RpcUpdatePlayer(curAct);//sends update to all individual clients
    }
    [ClientRpc]
    public void RpcUpdatePlayer(string[] curAct)//Update a player based off of list string. - gets sent to all clients
    {
        SetEquipAct(curAct);//update
    }
}
