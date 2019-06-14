using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

public class UDP_Listen : MonoBehaviour
{
    public Game game;
    
    private const int listenPort = 3005;
    private void StartListener() // server version, listens to messages and replies if needed to
    {
        Debug.Log("UDP is starting");
        UdpClient listener = new UdpClient(listenPort);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
        try
        {
            while (true)
            {
                Debug.Log("Waiting for client message");
                byte[] bytes = listener.Receive(ref groupEP);

                string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                Debug.Log($"Received UDP client message from {groupEP} :");

                XmlSerializer serilize_object = new XmlSerializer(typeof(string[]));
                StringReader open_string = new StringReader(message);
                string[] parameters = (string[])serilize_object.Deserialize(open_string);

                string returnString = null; // the string that we want to return if that is ever the case
                if (parameters.Length >= 2)
                {
                    if (parameters[1].Equals("Player Hit") && parameters.Length == 4) // token, "Player Hit", player name, left hand or right hand (0 or 1)
                    {
                        game.WeaponHit(parameters[0], parameters[2], parameters[3]);
                    }
                    else if (parameters[1] == "Stat Update") // client asking for their health, mana and stamina every .25 seconds
                    {
                        int[] stats = game.BattleStats(parameters[0]);
                        returnString = HTTP_Listen.Encode_XML(stats, typeof(int[]));
                    }
                }

                if (returnString != null)
                {
                    byte[] sendbuf = Encoding.ASCII.GetBytes(returnString);
                    listener.Send(sendbuf, sendbuf.Length, groupEP);
                    Debug.Log("Message sent back to the client");
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log(e);
        }
        finally
        {
            listener.Close();
        }
    }

    void Start()
    {
        Debug.Log("Starting");
        Thread thread = new Thread(StartListener);
        thread.Start();
    }
}