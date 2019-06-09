/*using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
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
        StartCoroutine(join("noorio", "he"));

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ButtonClicked() // login
    {
        string user = username.text;
        string pass = password.text;
        StartCoroutine(join(user, pass));
        
    }

    private IEnumerator join(string user, string pass)
    {
        RestClient.Get<DBPlayer>("https://ics4u-748c2.firebaseio.com/" + user + ".json").Then(response =>
        {
            if (response == null)
            {
                infoGiver.text = "Username does not exist";
            }
            else if (!response.password.Equals(pass))
            {
                infoGiver.text = "Password is incorrect";
            }
            else
            {
                Debug.Log(user + " has a score of " + response.score);
            }
        });

        yield return 0;

    }
}
*/