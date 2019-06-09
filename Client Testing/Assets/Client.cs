using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Upload("register", "iamnoamesa", "supercool"));
    }

    IEnumerator Upload(string request, string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("request", "register");
        form.AddField("username", username);
        form.AddField("password", password);

        UnityWebRequest www = UnityWebRequest.Post("http://209.182.232.50:1234/", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
