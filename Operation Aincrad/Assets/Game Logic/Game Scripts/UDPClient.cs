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
    public InfoCenter Info;
    public HTTPClient HTTP;
    public UpdatePlayer UP;

    public static string Encode_XML(object obj, Type required_type)
    { // Given an object that can be serilized and the type it is
        XmlSerializer serializer = new XmlSerializer(required_type);
        StringWriter sw = new StringWriter();
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        serializer.Serialize(sw, obj, ns);
        string converted_string = sw.ToString();

        return converted_string;
    }

    private void SendUDP(string[] parameters, bool askResponse)
    {
        string message = Encode_XML(parameters, typeof(string[]));
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        client.Send(buffer, buffer.Length); 

        Debug.Log("Message: " + message + " has been sent via UDP");

        if (askResponse)
        {
            byte[] b2 = client.Receive(ref ep);
            string response = System.Text.Encoding.ASCII.GetString(b2, 0, b2.Length);
            Debug.Log(response);

            // act on response
            if (parameters[1] == "Stat Update")
            {
                XmlSerializer serilize_object = new XmlSerializer(typeof(int[]));
                StringReader open_string = new StringReader(response);
                int[] newStats = (int[])serilize_object.Deserialize(open_string);

                Info.Hp = newStats[0];
                Info.Mana = newStats[1];
                Info.Stamina = newStats[2];
                Info.ReDraw();

                if (newStats[3] == 1) // something about their player changed, reload data from server
                {
                    HTTP.AskServer(new Dictionary<string, string> { { "request" , "stats" } });
                }else if (newStats[4] == 1)
                {
                    UP.UpdateEquip();
                }
            }
        }

    }

    UdpClient client;
    IPEndPoint ep = new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005);
    private string token;
    
    private IEnumerator EternalThread()
    {
        while (true) // ask for any updates
        {
            SendUDP(new string[] { token, "Stat Update" }, true);
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void PlayerHit(string hitName, int weaponUsed)
    {
        SendUDP(new string[] { token, "Player Hit", hitName, weaponUsed.ToString() }, false);
    }

    public void StartAsking() // continously asks the server for new info every .25 seconds
    {
        client = new UdpClient();
        client.Connect(new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005));
        Debug.Log("UDP Setup is done");

        StartCoroutine(EternalThread());
    }

    public void SetToken(string token)
    {
        this.token = token;
    }

    private void Start()
    {
        
    }
}
