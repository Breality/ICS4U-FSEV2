using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Buttonsa : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Tester()
    {
        StartCoroutine(Login("noobmaster69", "saoiii"));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(Register("noobmaster69", "saoiii"));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(Change("noobmaster69", 20));           //"noobmaster69", "saoiii"));
    }

    void Start()
    {
        StartCoroutine(Tester());
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
        
        UnityWebRequest www = UnityWebRequest.Post("http://home/nooriscool/sqlconnect/register.php", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
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

        

    }

    IEnumerator Login(string username, string pass)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", pass);

        UnityWebRequest www = UnityWebRequest.Post("http://sqlconnect/login.php", form); //http://localhost/sqlconnect/login.php
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
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
    }

    IEnumerator Change(string username, int coins)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("coins", coins);


        UnityWebRequest www = UnityWebRequest.Post("http://localhost/nooriscool/sqlconnect/change.php", form); 
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
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
}
