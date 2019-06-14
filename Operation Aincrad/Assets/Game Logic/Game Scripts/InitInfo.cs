using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InitInfo : MonoBehaviour
{
    [SerializeField]
    private GameObject WeaponsLeft, WeaponsRight;
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private TextMesh moneyText;
<<<<<<< HEAD
    [SerializeField]
    private ShopToggle stMan1, stMan2;
=======
>>>>>>> parent of e48984f... Conflict
    // Start is called before the first frame update
    void Awake()
    {
        InfoCenter inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        inCen.MoneyText = moneyText;
        inCen.WeaponsL = WeaponsLeft;
        inCen.WeaponsR = WeaponsRight;
        inCen.NameText = nameText;
<<<<<<< HEAD


        JoyStickListen jsL = this.GetComponent<JoyStickListen>();
        jsL.sellers = GameObject.Find("Sellers");

        stMan1.info = stMan2.info = inCen;
        stMan1.HTTP = stMan2.HTTP = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();
=======
>>>>>>> parent of e48984f... Conflict
    }
}
