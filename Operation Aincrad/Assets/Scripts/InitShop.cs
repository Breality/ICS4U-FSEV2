using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InitShop : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Transform shopHolder;
    [SerializeField]
    private Image hFill, mFill, sFill;
    [SerializeField]
    private TMP_Text hText;
    [SerializeField]
    private GameObject WeaponsLeft, WeaponsRight;
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private TextMesh moneyText;
    void Start()
    {
        InfoCenter inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        HTTPClient handlerMan = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();
        UDPClient UDP = GameObject.Find("UDP Handler").GetComponent<UDPClient>();
        inCen.MoneyText = moneyText;
        inCen.WeaponsL = WeaponsLeft;
        inCen.WeaponsR = WeaponsRight;
        inCen.NameText = nameText;


        JoyStickListen jsL = this.GetComponent<JoyStickListen>();
        jsL.sellers = GameObject.Find("Sellers");

        this.GetComponent<SwordCollision>().udpHandler = UDP;

        foreach(Transform shop in shopHolder)
        {
            ShopToggle stMan = shop.Find("Shop Handler").GetComponent<ShopToggle>();
            stMan.HTTP = handlerMan;
            stMan.info = inCen;
        }


        inCen.HpFill = hFill;
        inCen.ManaFill = mFill;
        inCen.StaminaFill = sFill;
        inCen.HpText = hText;

        UDP.StartAsking();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
