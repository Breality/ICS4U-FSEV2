using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * InitShop.cs
 * Description: Main Setup Script used to set all types of variables when user loads in. 
 */
public class InitShop : MonoBehaviour
{
    [SerializeField]
    private Transform shopHolder;//get transform to be looped to get all shop handlers 
    [SerializeField]
    private Image hFill, mFill, sFill;// get health, mana, and stamina bars to be edited by server
    [SerializeField]
    private TMP_Text hText;// get health numbers
    [SerializeField]
    private GameObject WeaponsLeft, WeaponsRight, bLeft, bRight, Hats, Necklaces, Arnament;//get all objects that hold equipment
    [SerializeField]
    private TMP_Text nameText;//get username text
    [SerializeField]
    private TextMesh moneyText;// get money text
    void Start()
    {
        //Get Script Components so we can set variables needed by those scripts
        InfoCenter inCen = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
        HTTPClient handlerMan = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();
        UDPClient UDP = GameObject.Find("UDP Handler").GetComponent<UDPClient>();

        UDP.UP = this.GetComponent<UpdatePlayer>();//The UDP Server needs to update the client equipment when people by new equipment and put them on

        handlerMan.character = this.transform.gameObject;//needs this character so that when a character dies it can respawn it at a good position

        //Set info center data
        inCen.MoneyText = moneyText;
        inCen.WeaponsL = WeaponsLeft;
        inCen.WeaponsR = WeaponsRight;
        inCen.NameText = nameText;
        inCen.BootsLeft = bLeft;
        inCen.BootsRight = bRight;
        inCen.Helmets = Hats;
        inCen.Armour = Arnament;
        inCen.Pendants = Necklaces;
        inCen.HpFill = hFill;
        inCen.ManaFill = mFill;
        inCen.StaminaFill = sFill;
        inCen.HpText = hText;

        JoyStickListen jsL = this.GetComponent<JoyStickListen>();//listens to controllers adn it needs the sellers in the main menu so it can check distance to closest seller. When distance is close enough, proper menu shows up.
        jsL.sellers = GameObject.Find("Sellers");//update sellers.

        this.GetComponent<SwordCollision>().udpHandler = UDP;//sword collision needs the UDP Client so it can send messages of when a sword hits another player

        foreach(Transform shop in shopHolder)// for each shop in the shop holder set shop info
        {
            ShopToggle stMan = shop.Find("Shop Handler").GetComponent<ShopToggle>();
            stMan.HTTP = handlerMan;
            stMan.info = inCen;
        }

        UDP.StartAsking();//send message to UDP client to start listening and updating the client about health, mana, stamina, etc after a certain time interval.
    }
}
