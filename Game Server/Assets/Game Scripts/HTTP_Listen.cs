using Proyecto26;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using Random = System.Random;

public class HTTP_Listen : MonoBehaviour
{
    public Game game;
    private HttpListener listener;
    private Thread listenerThread;
    private Dictionary<string, Player> playerDB = new Dictionary<string, Player> { };
    private Dictionary<string, string> playerHash = new Dictionary<string, string> { };

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
    
    private void ConstructResponse(HttpListenerContext context, string message)
    {
        // Construct a response.
        HttpListenerResponse response = context.Response;
        string responseString = message;
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);

        output.Close();
    }

    private void HTTPRecieved(IAsyncResult result) // callback function for when we get an http request
    {
        Debug.Log("HTTP recieved");
        HttpListener listener = (HttpListener)result.AsyncState;
        HttpListenerContext context = listener.EndGetContext(result);

        // do stuff
        string startData = GetRequestPostData(context.Request);
        Debug.Log(startData);

        string[] dataSent = startData.Split('&');
        Debug.Log(dataSent.Length);

        Dictionary<string, string> data = new Dictionary<string, string> { };

        foreach (string d in dataSent)
        {
            string[] cell = d.Split('=');
            data[cell[0]] = cell[1];
            Debug.Log(cell[0] + ":" + cell[1]);
        }

        if (!data.ContainsKey("request"))
        {
            ConstructResponse(context, "Request not included");
            return;
        }else if (data.ContainsKey("token") && !playerDB.ContainsKey(data["token"]))
        {
            ConstructResponse(context, "Token does not exist");
            return;
        }

        // handling the differant kinds of requests they want
        if (data["request"].Equals("register") && data.ContainsKey("username") && data.ContainsKey("password"))
        {
            StartCoroutine(Register(context, data["username"], data["password"]));
        }
        else if (data["request"].Equals("login") && data.ContainsKey("username") && data.ContainsKey("password"))
        {
            StartCoroutine(LogIn(context, data["username"], data["password"]));
        }
        else if (data["request"].Equals("logout") && data.ContainsKey("token"))
        {
            StartCoroutine(Logout(context, data["token"]));
            
        }
        else // they did not match it
        {
            ConstructResponse(context, "Invalid arguements");
        }
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

    private int RandomNumber(int min, int max)
    {
        Random random = new Random();
        return random.Next(min, max);
    }

    private string RandomString(int size, bool lowerCase)
    {
        StringBuilder builder = new StringBuilder();
        Random random = new Random();
        char ch;
        for (int i = 0; i < size; i++)
        {
            ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
            builder.Append(ch);
        }
        if (lowerCase)
            return builder.ToString().ToLower();
        return builder.ToString();
    }

    private string RandomToken()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(RandomString(8, new Random().Next(1)==0));
        builder.Append(RandomNumber(1000, 99999));
        builder.Append(RandomString(4, false));
        return builder.ToString();
    }


    public IEnumerator Logout(HttpListenerContext sender, string token)
    {
        Player player = playerDB[token];
        DBPlayer saveDava = new DBPlayer(player, playerHash[token]);
        RestClient.Put<ResponseHelper>("https://ics4u-748c2.firebaseio.com/" + saveDava.username + ".json", saveDava);

        if (sender != null) // sender is null during testing
        {
            ConstructResponse(sender, "You have been logged out");
        }

        yield return 0;
    }

    public IEnumerator Register(HttpListenerContext sender, string username, string password)
    {
        Debug.Log("Registration starting");
        RestClient.Get<DBPlayer>("https://ics4u-748c2.firebaseio.com/" + username + ".json").Then(response =>
        {
            if (response != null)
            {
                Debug.Log("Username taken");
                ConstructResponse(sender, "Username taken");
            }
            else
            {
                DBPlayer player = new DBPlayer(username, Hash(password));
                RestClient.Put<ResponseHelper>("https://ics4u-748c2.firebaseio.com/" + username + ".json", player);
                Debug.Log("Account made");

                // return the http response now
                string randToken = RandomToken();
                Player newPlayer = new Player(player, randToken);

                playerHash[randToken] = player.hash;
                playerDB[randToken] = newPlayer;
                game.PlayerEnter(newPlayer, randToken);

                if (sender != null) // sender is null during testing
                {
                    ConstructResponse(sender, "Creation success, token:" + randToken + ", data" + player.ToString());
                }
            }
        });

        yield return 0;
    }

    public IEnumerator LogIn(HttpListenerContext sender, string username, string password)
    {
        RestClient.Get<DBPlayer>("https://ics4u-748c2.firebaseio.com/" + username + ".json").Then(response =>
        {
            if (response == null)
            {
                Debug.Log("Username does not exist");
                ConstructResponse(sender, "Username and password do not match");
            }
            else if (response.hash != Hash(password))
            {
                Debug.Log("Incorrect password");
                ConstructResponse(sender, "Username and password do not match");
            }
            else
            {
                Debug.Log("Login sucess");

                // return the http response now
                try
                {
                    string randToken = RandomToken();
                    Player newPlayer = new Player(response, randToken);

                    playerHash[randToken] = response.hash;
                    playerDB[randToken] = newPlayer;
                    game.PlayerEnter(newPlayer, randToken);

                    if (sender != null) // sender is null during testing
                    {
                        ConstructResponse(sender, "Login success, token:" + randToken + ", data" + response.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
                
            }
        });

        yield return 0;
    }
}
