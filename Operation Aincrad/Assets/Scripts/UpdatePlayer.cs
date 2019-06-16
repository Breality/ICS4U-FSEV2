using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UpdatePlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Transform SwordL, SwordR, Armor, BootL, BootR, Pendants, Helmets;
    private Transform[] possEquip;
    private string[] curEquip = new string[7];
    private void Awake()
    {
        possEquip = new Transform[7] { SwordL, SwordR, Armor, BootL, BootR, Pendants, Helmets };      
    }
    private void ClearCA()
    {
        for(int i = 0; i<curEquip.Length; i++)
        {
            curEquip[i] = null;
        }
    }
    public void UpdateEquip()
    {
       Debug.Log("Called");
       ClearCA();
       for(int curEquipInd = 0; curEquipInd<possEquip.Length; curEquipInd++)
        {
            GetActiveEquip(possEquip[curEquipInd], curEquipInd);
            Debug.Log(curEquipInd + " " + curEquip[curEquipInd]);
        }
        CmdUpdatePlayer(curEquip);

    }
    private void GetActiveEquip(Transform EquipHolder, int curEquipInd)
    {
        foreach(Transform child in EquipHolder)
        {
            if (child.gameObject.activeSelf)
            {
                curEquip[curEquipInd] = child.name;
                return;
            }
        }
    }
    public void SetEquipAct(string[] curAct)
    {
        for (int curEquipIndex = 0; curEquipIndex < possEquip.Length; curEquipIndex++)
        {
            foreach (Transform weapon in possEquip[curEquipIndex])
            {
                if (weapon.name == curAct[curEquipIndex])
                {
                    weapon.gameObject.SetActive(true);
                }
                else
                {
                    weapon.gameObject.SetActive(false);
                }
            }
        }
    }
    [Command]
    public void CmdUpdatePlayer(string[] curAct)
    {
        SetEquipAct(curAct);
        RpcUpdatePlayer(curAct);
    }
    [ClientRpc]
    public void RpcUpdatePlayer(string[] curAct)
    {
        SetEquipAct(curAct);
    }

}
