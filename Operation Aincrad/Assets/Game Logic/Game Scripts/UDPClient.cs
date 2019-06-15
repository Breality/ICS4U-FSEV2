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

    public static string Encode_XML(object obj, Type required_type)
    { // Given an object that can be serilized and the type it is
        XmlSerializer serializer = new XmlSerializer(required_type);
        StringWriter sw = new StringWriter();
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        serializer.Serialize(sw, obj, ns);
        string converted_string = sw.ToString();

        return converted_string;
    }

    public void SendUDP(string[] parameters, bool askResponse)
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
                }
            }
        }

    }

    UdpClient client;
    IPEndPoint ep = new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005);
    private string token;
    
    private void EternalThread()
    {
        while (true) // ask for any updates
        {
            SendUDP(new string[] { token, "Stat Update" }, true);
            System.Threading.Thread.Sleep(250);
        }
    }

    public void StartAsking(string token) // continously asks the server for new info every .25 seconds
    {
        this.token = token;
        client = new UdpClient();
        client.Connect(new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005));
        Debug.Log("UDP Setup is done");

        Thread newThread = new Thread(EternalThread);
        newThread.Start();
    }

    private void Start()
    {
        
    }
}
