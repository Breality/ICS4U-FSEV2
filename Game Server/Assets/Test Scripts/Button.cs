using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Button : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Login("noobmaster69", "saoiii"));
        StartCoroutine(Register("noobmaster69", "saoiii"));
        StartCoroutine(Change("noobmaster69", 20));           //"noobmaster69", "saoiii"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Register(string username, string pass)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", pass);
        
        UnityWebRequest www = UnityWebRequest.Post("http://localhost/sqlconnect/register.php", form);
        yield return www.SendWebRequest();

        string text = www.downloadHandler.text.Substring(1);

        if (text.Equals("0"))
        {
            Debug.Log("User created");
        }
        else
        {
            Debug.Log(text);
        }

    }

    IEnumerator Login(string username, string pass)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", pass);

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/sqlconnect/login.php", form);
        yield return www.SendWebRequest();

        string text = www.downloadHandler.text.Substring(1);

        if (text[0] == '0')
        {
            Debug.Log("User logged in");
        }
        else
        {
            Debug.Log(text);
        }
    }

    IEnumerator Change(string username, int coins)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("coins", coins);

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/sqlconnect/Change.php", form);
        yield return www.SendWebRequest();

        string text = www.downloadHandler.text.Substring(1);

        if (text[0] == '0')
        {
            Debug.Log("Stat changed");
        }
        else
        {
            Debug.Log(text);
        }
    }
}
