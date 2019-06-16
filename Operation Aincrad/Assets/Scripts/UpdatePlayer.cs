using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UpdatePlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Transform SwordL, SwordR, Armor, BootL, BootR, Pendants, Helmets;
    private Transform[] curActive, possEquip;
    private void Start()
    {
        possEquip = new Transform[7] { SwordL, SwordR, Armor, BootL, BootR, Pendants, Helmets };
        curActive = new Transform[7];
        ClearCA();
      
    }
    private void ClearCA()
    {
        for(int i = 0; i<curActive.Length; i++)
        {
            curActive[i] = null;
        }
    }
    public void UpdateEquip()
    {
       ClearCA();
       for(int curEquipInd = 0; curEquipInd<possEquip.Length; curEquipInd++)
        {
            GetActiveEquip(possEquip[curEquipInd], curEquipInd);
        }
        foreach(Transform t in curActive)
        {
            Debug.Log(t);
        }
        CmdUpdatePlayer(curActive);

    }
    private void GetActiveEquip(Transform EquipHolder, int curEquipInd)
    {
        foreach(Transform child in EquipHolder)
        {
            if (child.gameObject.activeSelf)
            {
                curActive[curEquipInd] = child;
                return;
            }
        }
    }
    public void SetEquipAct(Transform[] curAct)
    {
        for (int curEquipIndex = 0; curEquipIndex < possEquip.Length; curEquipIndex++)
        {
            foreach (Transform weapon in possEquip[curEquipIndex])
            {
                if (weapon == curAct[curEquipIndex])
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
    public void CmdUpdatePlayer(Transform[] curAct)
    {
        SetEquipAct(curAct);
        RpcUpdatePlayer(curAct);
    }
    [ClientRpc]
    public void RpcUpdatePlayer(Transform[] curAct)
    {
        SetEquipAct(curAct);
    }

}
