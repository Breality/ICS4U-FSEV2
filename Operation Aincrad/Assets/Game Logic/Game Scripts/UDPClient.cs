/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * UDPClient class
 * Description:
 * This class handles the UDP requests send to and recieved back from the server. This includes updating a player's equipment as they join and damage
 */

 // Importing modules
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

public class UDPClient : MonoBehaviour
{
    // public variables
    public InfoCenter Info; // the main info center that calls upon this class
    public HTTPClient HTTP; // the http client with the player tokens
    public UpdatePlayer UP; // the updateplayer class with functions for Mirror networking

    public static string Encode_XML(object obj, Type required_type)
    {
        // Given an object that can be serilized and the type it is, encode it into an xml string to send over to the server
        XmlSerializer serializer = new XmlSerializer(required_type);
        StringWriter sw = new StringWriter();
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        serializer.Serialize(sw, obj, ns);
        string converted_string = sw.ToString();

        return converted_string;
    }


    private IEnumerator SendUDP(string[] parameters, bool askResponse)
    {   
        // Sends a UDP message to the server with the parameters requested
        string message = Encode_XML(parameters, typeof(string[])); // encoding the parameters as a string

        // sending the string as a buffer to the server
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        client.Send(buffer, buffer.Length); 

        Debug.Log("Message: " + message + " has been sent via UDP");

        if (askResponse) // if we are expecting a response, wait for one
        {
            // yield until we can get a response
            byte[] b2 = client.Receive(ref ep);
            string response = System.Text.Encoding.ASCII.GetString(b2, 0, b2.Length); // the response the server sent
            Debug.Log(response);

            // act on response
            if (parameters[1] == "Stat Update") // we asked for a stat update (health, mana, etc) and this was sent back
            {
                // decrypt the string into the correct format of int[] sent by the server
                XmlSerializer serilize_object = new XmlSerializer(typeof(int[]));
                StringReader open_string = new StringReader(response);
                int[] newStats = (int[])serilize_object.Deserialize(open_string); // the new statistics

                // set the hp, mana and stamina
                Info.Hp = newStats[0];
                Info.Mana = newStats[1];
                Info.Stamina = newStats[2];
                Info.ReDraw(); // redraw

                if (newStats[3] == 1) // something about their player changed, reload data from server 
                {
                    HTTP.AskServer(new Dictionary<string, string> { { "request" , "stats" } });
                }else if (newStats[4] == 1) // A player has just joined, update our equipment globally so they see what we are wearing 
                {
                    UP.UpdateEquip();
                }
            }
        }

        yield return 0; // this is required to keep it as an IEnumerator instead of a void function, which is needed to start coroutines 
    }

    // private variables for networking
    UdpClient client;
    IPEndPoint ep = new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005);
    private string token;
    
    private IEnumerator HandleMessage() 
    {
        // this function sends a message (asking for battle stats) that needs a response, but cuts off the if it takes too long, to avoid crashing
        Coroutine messageSent = StartCoroutine(SendUDP(new string[] { token, "Stat Update"}, true));
        yield return new WaitForSeconds(1.5f);
        StopCoroutine(messageSent);
    }

    private IEnumerator EternalThread()
    {
        // this function will continously ask the server for an update on the player battle stats 
        while (true) // 4 times per second, call the handlemessage()
        {
            StartCoroutine(HandleMessage());
            yield return new WaitForSeconds(0.25f);
        }
    }


    public void PlayerHit(string hitName, int weaponUsed) // public function invoked when a weapon thinks it hit a player, and this function relays the message to the server
    {   
        StartCoroutine(SendUDP(new string[] { token, "Player Hit", hitName, weaponUsed.ToString() }, false));
    }

    public void StartAsking() // The player has just been logged in by the HTTPClient, now start using UDP
    {
        client = new UdpClient();  // initialize the UDP client 
        client.Connect(new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005)); // connect to the server
        Debug.Log("UDP Setup is done");

        StartCoroutine(EternalThread()); // start the eternal thread to ask the server for updates
    }

    public void SetToken(string token) // this token is needed to validate the player and is set by the httpclient
    {
        this.token = token;
    }

    private void Start()
    {
        // a start function is needed for monobehaviours to maintain all of their properties
    }
}
