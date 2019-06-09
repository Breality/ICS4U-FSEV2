using Proyecto26;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

public class HTTP_Listen : MonoBehaviour
{
    private HttpListener listener;
    private Thread listenerThread;

    private string GetRequestPostData(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
        {
            return null;
        }
        using (System.IO.Stream body = request.InputStream) // here we have data
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }
    }

    int c = 0;
    private void HTTPRecieved(IAsyncResult result) // callback function for when we get an http request
    {
        Debug.Log("HTTP recieved: " + c);
        HttpListener listener = (HttpListener)result.AsyncState;
        HttpListenerContext context = listener.EndGetContext(result);

        // do stuff
        string data = GetRequestPostData(context.Request);
        Debug.Log(data);

        // Construct a response.
        HttpListenerResponse response = context.Response;
        string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);

        output.Close();
        Debug.Log("We sent a response back");
        c++;
    }

    private void HttpHandler()
    {
        Debug.Log("Starting Http Handler");
        listener = new HttpListener();
        listener.Prefixes.Add("http://*:1234/");
        listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        listener.Start();

        Debug.Log("We are now online");
        while (true)
        {
            var result = listener.BeginGetContext(HTTPRecieved, listener);
            Thread.Sleep(10);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        listenerThread = new Thread(HttpHandler);
        listenerThread.Start();
    }

    void OnApplicationQuit()
    {
        listenerThread.Abort();
        listener.Close();
        Debug.Log("Closed, we are offline.");
    }


    // Database handle
    private string Hash(string word)
    {
        MD5 hashed = MD5.Create();
        byte[] data = hashed.ComputeHash(Encoding.UTF8.GetBytes(word));

        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }

    public IEnumerator MakePlayer(string username, string password)
    {
        RestClient.Get<DBPlayer>("https://ics4u-748c2.firebaseio.com/" + username + ".json").Then(response =>
        {
            if (response != null)
            {
                Debug.Log("Username taken");
            }
            else
            {
                DBPlayer player = new DBPlayer(username, Hash(password), null);
                RestClient.Put<ResponseHelper>("https://ics4u-748c2.firebaseio.com/" + username + ".json", player);
                Debug.Log("Account made");

                // return the http response now
                Player newPlayer = new Player(player);
            }
        });

        yield return 0;
    }

    public IEnumerator LogIn(string username, string password)
    {
        RestClient.Get<DBPlayer>("https://ics4u-748c2.firebaseio.com/" + username + ".json").Then(response =>
        {
            if (response == null)
            {
                Debug.Log("Username does not exist");
            }
            else if (response.hash != Hash(password))
            {
                Debug.Log("Incorrect password");
            }
            else
            {
                DBPlayer player = new DBPlayer(username, Hash(password), null);
                Debug.Log("Login sucess");

                // return the http response now
                Player newPlayer = new Player(player);
            }
        });

        yield return 0;
    }
}
