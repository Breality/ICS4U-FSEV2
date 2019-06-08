using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Button : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Register("noobmaster69", "saoiii"));
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
}
