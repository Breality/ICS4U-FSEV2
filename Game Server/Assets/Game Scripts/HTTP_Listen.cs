using Newtonsoft.Json;
using Proyecto26;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

using Random = System.Random;

public class HTTP_Listen : MonoBehaviour
{
    public Game game;
    private HttpListener listener;
    private Thread listenerThread;
    private Dictionary<string, Player> playerDB = new Dictionary<string, Player> { };
    private Dictionary<string, string> playerHash = new Dictionary<string, string> { };
    private string firebaseExtension = "hidden";
    
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
            data[cell[0]] =  cell[1].Replace("%20", " ");
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
        // login/register/logout
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

        // debugging purposes
        else if (data["request"].Equals("time compare"))
        {
            Debug.Log(Time.time - float.Parse(data["timer"]));
        }

        // purchasing stuff
        else if (data["request"].Equals("purchase") && data.ContainsKey("item type") && data.ContainsKey("item name"))
        {
            game.Purchase(data["token"], data["item type"], data["item name"]);
        }

        // equipment changes
        else if (data["request"].Equals("equip") && data.ContainsKey("equipment type") && data.ContainsKey("equipment name"))
        {
            Player player = playerDB[data["token"]];
            if (!Array.Exists(player.GetWeapons(), element => element == data["equipment name"])){ // check if they own the item
                ConstructResponse(context, "You do not own this item");
                return;
            }
       
            if (data["equipment type"].Equals("Weapon"))
            {
                Weapon item = new Weapon(null, "Not important", game.equipments["Weapons"][data["Equipment Name"]]);
                if (item.weaponType == 0)
                {
                    player.ChangeEquipped(4, data["equipment name"]);
                }
                else if (item.weaponType == 1)
                {
                    player.ChangeEquipped(5, data["equipment name"]);
                }
            }
            else
            {
                int correspondingIndex = (new Dictionary<string, int> { })[data["equipment type"]];
            }
        }

        // asking for stats because the game told them to
        else if (data["request"].Equals("stats")){ // overall max stats
            DBPlayer fakeDB = new DBPlayer(playerDB[data["token"]], "unimportant hash");
            string xmlString = Encode_XML(fakeDB, typeof(DBPlayer));
            ConstructResponse(context, xmlString);
        }

        else // they did not match any of the criteria
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
        //StartCoroutine(Register(null, "boiNew", "123"));
        //StartCoroutine(LogIn(null, "boiNew", "123"));

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
    public static string Encode_XML(object obj, Type required_type)
    { // Given an object that can be serilized and the type it is
        XmlSerializer serializer = new XmlSerializer(required_type);
        StringWriter sw = new StringWriter();
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        serializer.Serialize(sw, obj, ns);
        string converted_string = sw.ToString();

        return converted_string;
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
        builder.Append(RandomNumber(1000, 99999999));
        builder.Append(RandomString(4, false));
        return builder.ToString();
    }


    public IEnumerator Logout(HttpListenerContext sender, string token)
    {
        Player player = playerDB[token];
        DBPlayer saveDava = new DBPlayer(player, playerHash[token]);
        RestClient.Put<ResponseHelper>("https://" + firebaseExtension + ".firebaseio.com/" + saveDava.username + ".json", saveDava);

        if (sender != null) // sender is null during testing
        {
            ConstructResponse(sender, "You have been logged out");
        }

        yield return 0;
    }

    public IEnumerator Register(HttpListenerContext sender, string username, string password)
    {
        Debug.Log("Registration starting");
        RestClient.Get<DBPlayer>("https://" + firebaseExtension + ".firebaseio.com/" + username + ".json").Then(response =>
        {
            if (response != null)
            {
                Debug.Log("Username taken");
                ConstructResponse(sender, "Username taken");
            }
            else
            {
                try
                {
                    DBPlayer player = new DBPlayer(username, Hash(password));
                    RestClient.Put<ResponseHelper>("https://" + firebaseExtension + ".firebaseio.com/" + username + ".json", player);
                    Debug.Log("Account made");

                    // return the http response now
                    string randToken = RandomToken();
                    Player newPlayer = new Player(player, randToken);

                    playerHash[randToken] = player.hash;
                    playerDB[randToken] = newPlayer;
                    game.PlayerEnter(newPlayer, randToken);

                    string xmlString = Encode_XML(player, typeof(DBPlayer));
                    string[][] equipKeys = new string[3][] {
                        (new List<string>(game.equipments["Clothing"].Keys)).ToArray(),
                        (new List<string>(game.equipments["Weapons"].Keys)).ToArray(),
                        (new List<string>(game.equipments["Items"].Keys)).ToArray() };

                    string[][] equipVals = new string[3][] {
                        (new List<string>(game.equipments["Clothing"].Values)).ToArray(),
                        (new List<string>(game.equipments["Weapons"].Values)).ToArray(),
                        (new List<string>(game.equipments["Items"].Values)).ToArray() };

                    string EquipmentXML1 = Encode_XML(equipKeys, typeof(string[][]));
                    string EquipmentXML2 = Encode_XML(equipVals, typeof(string[][]));

                    if (sender != null) // sender is null during testing
                    {
                        ConstructResponse(sender, "Creation success, token:" + randToken + ", Equipment Keys:" + EquipmentXML1 +
                            ", Equipment Values:" + EquipmentXML2 + ", Player Data:" + xmlString);
                    }
                }catch (Exception e)
                {
                    Debug.Log(e);
                }
                
            }
        });

        yield return 0;
    }

    public IEnumerator LogIn(HttpListenerContext sender, string username, string password)
    {
        RestClient.Get<DBPlayer>("https://" + firebaseExtension + ".firebaseio.com/" + username + ".json").Then(response =>
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

                    // game.equipments is Dictionary<string, Dictionary<string, string>> which will be formatted as two seperate string[][]
                    string xmlString = Encode_XML(response, typeof(DBPlayer));
                    string[][] equipKeys = new string[3][] {
                        (new List<string>(game.equipments["Clothing"].Keys)).ToArray(),
                        (new List<string>(game.equipments["Weapons"].Keys)).ToArray(),
                        (new List<string>(game.equipments["Items"].Keys)).ToArray() };

                    string[][] equipVals = new string[3][] {
                        (new List<string>(game.equipments["Clothing"].Values)).ToArray(),
                        (new List<string>(game.equipments["Weapons"].Values)).ToArray(),
                        (new List<string>(game.equipments["Items"].Values)).ToArray() };

                    string EquipmentXML1 = Encode_XML(equipKeys, typeof(string[][]));
                    string EquipmentXML2 = Encode_XML(equipVals, typeof(string[][]));

                    if (sender != null) // sender is null during testing
                    {
                        ConstructResponse(sender, "Login success, token:" + randToken + ", Equipment Keys:" + EquipmentXML1 + 
                            ", Equipment Values:"+ EquipmentXML2 + ", Player Data:" + xmlString);
                    }

                    // StartCoroutine(Logout(null, randToken)); // testing
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
