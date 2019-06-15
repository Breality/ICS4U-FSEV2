using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InitShop : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private ShopToggle stMan1, stMan2;
    [SerializeField]
    private Image hFill, mFill, sFill;
    [SerializeField]
    private TMP_Text hText;
    void Awake()
    {
        InfoCenter inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        HTTPClient handlerMan = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();
        stMan1.info = stMan2.info = inCen;
        stMan1.HTTP = stMan2.HTTP = handlerMan;

        inCen.HpFill = hFill;
        inCen.ManaFill = mFill;
        inCen.StaminaFill = sFill;
        inCen.HpText = hText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
