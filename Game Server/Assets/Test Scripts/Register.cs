/*using Proyecto26;
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
        StartCoroutine(create("PlayerTest", "secure123"));

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
        RestClient.Get<Player>("https://ics4u-748c2.firebaseio.com/" + user + ".json").Then(response =>
        {
            if (response != null)
            {
                Debug.Log("Username taken");
            }
            else
            {
                RestClient.Put<ResponseHelper>("https://ics4u-748c2.firebaseio.com/" + user + ".json", new Player(user, pass)).Then(response2 =>
                {
                    Debug.Log("Account creation status " + response2.StatusCode);
                    
                }).Catch(error =>
                {
                    Debug.Log("Error " + error.ToString());
                });
                
            }
        });

        yield return 0;

    }
}
*/