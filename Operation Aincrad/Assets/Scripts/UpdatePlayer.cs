using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UpdatePlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Transform SwordL, SwordR, Armor, BootL, BootR, Pendants, Helmets;
    private List<Transform> curActive, possEquip;
    private void Start()
    {
        possEquip = new List<Transform>() { SwordL, SwordR, Armor, BootL, BootR, Pendants, Helmets };
    }
    public void UpdateEquip()
    {
        curActive.Clear();
        foreach(Transform equip in possEquip)
        {
            GetActiveEquip(equip);
        }
        foreach(Transform t in curActive)
        {
            Debug.Log(t);
        }
        Debug.Log(curActive);

    }
    private void GetActiveEquip(Transform EquipHolder)
    {
        foreach(Transform child in EquipHolder)
        {
            if (child.gameObject.activeSelf)
            {
                curActive.Add(child);
                return;
            }
        }
        curActive.Add(null);
    }
    public void SetEquipAct(List<Transform> curAct)
    {
        for (int curEquipIndex = 0; curEquipIndex < possEquip.Count; curEquipIndex++)
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
    public void CmdUpdatePlayer(List<Transform> curAct)
    {
        SetEquipAct(curAct);
        RpcUpdatePlayer(curAct);
    }
    [ClientRpc]
    public void RpcUpdatePlayer(List<Transform> curAct)
    {
        SetEquipAct(curAct);
    }

}
