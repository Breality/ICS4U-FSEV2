using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class Starter : MonoBehaviour
{
    IEnumerator Upload() // testi
    {
        WWWForm form = new WWWForm();
        form.AddField("request", "Time compare");
        form.AddField("timer", Time.time.ToString());

        UnityWebRequest www = UnityWebRequest.Post("http://209.182.232.50:1234/", form);
        yield return www.SendWebRequest();
        
    }

    void Start()
    {
        StartCoroutine(Upload());
    }
}