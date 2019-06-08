using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
//using MongoDB.Driver;

public class HTTP_Listen : MonoBehaviour
{
    private HttpListener listener;
    private Thread listenerThread;

    private void HTTPRecieved(IAsyncResult result) // callback function for when we get an http request
    {
        Debug.Log("HTTP recieved");
        HttpListener listener = (HttpListener)result.AsyncState;
        HttpListenerContext context = listener.EndGetContext(result);

        // do stuff
        HttpListenerRequest request = context.Request;
        Debug.Log(request.ToString());

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


}
