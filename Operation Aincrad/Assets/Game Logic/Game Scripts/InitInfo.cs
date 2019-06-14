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
    [SerializeField]
    private ShopToggle stMan1, stMan2;
    // Start is called before the first frame update
    void Awake()
    {
        InfoCenter inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        inCen.MoneyText = moneyText;
        inCen.WeaponsL = WeaponsLeft;
        inCen.WeaponsR = WeaponsRight;
        inCen.NameText = nameText;


        JoyStickListen jsL = this.GetComponent<JoyStickListen>();
        jsL.sellers = GameObject.Find("Sellers");

        stMan1.info = stMan2.info = inCen;
        stMan1.HTTP = stMan2.HTTP = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();
    }
}
