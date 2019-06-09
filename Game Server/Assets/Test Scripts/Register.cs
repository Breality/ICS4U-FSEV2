using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    public InputField username;
    public InputField password;
    public InputField password2;
    public Button clicker;
    public Text infoGiver;

    // Start is called before the first frame update
    void Start()
    {
        clicker.onClick.AddListener(ButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ButtonClicked() // register
    {
        string user = username.text;
        string pass = password.text;
        StartCoroutine(create(user, pass));
        
    }

    private IEnumerator create(string user, string pass)
    {
        RestClient.Get<DBPlayer>("https://ics4u-748c2.firebaseio.com/" + user + ".json").Then(response =>
        {
            if (response != null)
            {
                infoGiver.text = "Username taken";
            }
            else
            {
                RestClient.Put("https://ics4u-748c2.firebaseio.com/" + user + ".json", new DBPlayer(user, pass, 0));
                infoGiver.text = "Account made";
            }
        });

        yield return 0;

    }
}
