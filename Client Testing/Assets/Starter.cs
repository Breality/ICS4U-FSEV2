using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class Starter : MonoBehaviour // the client version: Listens to messages and considers as long as its from the server
{
    /*

    private const int listenPort = 3005;
    private static void StartListener()
    {
        Debug.Log("Work is starting");
        UdpClient listener = new UdpClient(listenPort);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

        try
        {
            while (true)
            {
                Debug.Log("Waiting for broadcast");
                byte[] bytes = listener.Receive(ref groupEP);

                Debug.Log($"Received broadcast from {groupEP} :");
                Debug.Log($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
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

    float lastClick = 0;
    private void Update() // sending a message
    {
        if (Input.GetKey(KeyCode.T) && Time.time - lastClick > 0.5f)
        {
            Debug.Log("You have clicked");
            lastClick = Time.time;
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress broadcast = IPAddress.Parse("209.182.232.50");

            byte[] sendbuf = Encoding.ASCII.GetBytes("HeLlO");
            IPEndPoint ep = new IPEndPoint(broadcast, 3005);

            s.SendTo(sendbuf, ep);

            Debug.Log("Message sent to the broadcast address");
        }
    }
    */

    void Test()
    {
        Debug.Log("Starting");
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005);

        string i = "Helllooo";
        UdpClient client = new UdpClient();

        client.Connect(new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005));

        Byte[] buffer = null;
        buffer = Encoding.Unicode.GetBytes(i.ToString());
        client.Send(buffer, buffer.Length); //, ep);
        Debug.Log("Message sent>?");
        byte[] b2 = client.Receive(ref ep);
        string str2 = System.Text.Encoding.ASCII.GetString(b2, 0, b2.Length);
        Debug.Log("Recieved: " + str2);
    }

    private void Start()
    {
        Thread thread = new Thread(Test);
        thread.Start();
    }
}