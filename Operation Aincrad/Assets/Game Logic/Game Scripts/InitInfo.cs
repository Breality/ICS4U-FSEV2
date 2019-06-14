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
=======
    [SerializeField]
    private ShopToggle stMan;
>>>>>>> 5471fe37d0b776fa448d4e0633b5861f00c25cdb
    // Start is called before the first frame update
    void Awake()
    {
        InfoCenter inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        inCen.MoneyText = moneyText;
        inCen.WeaponsL = WeaponsLeft;
        inCen.WeaponsR = WeaponsRight;
        inCen.NameText = nameText;
<<<<<<< HEAD
=======

        JoyStickListen jsL = this.GetComponent<JoyStickListen>();
        jsL.sellers = GameObject.Find("Sellers");

        stMan.info = inCen;

>>>>>>> 5471fe37d0b776fa448d4e0633b5861f00c25cdb
    }
}
