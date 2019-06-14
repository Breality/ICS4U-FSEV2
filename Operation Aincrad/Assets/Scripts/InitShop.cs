using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitShop : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private ShopToggle stMan1, stMan2;
    void Awake()
    {
        InfoCenter inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        HTTPClient handlerMan = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();
        stMan1.info = stMan2.info = inCen;
        stMan1.HTTP = stMan2.HTTP = handlerMan;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
