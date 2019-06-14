using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
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

    public async Task TimeoutAfter(Task task, int millisecondsTimeout)
    {
        if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
            await task;
        else
            throw new TimeoutException();
    }

    UdpClient client;
    IPEndPoint ep = new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005);
    void SendMessage()
    {
        //Byte[] buffer = null;
        string message = Encode_XML(new string[] {"no responses" }, typeof(string[]));
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        client.Send(buffer, buffer.Length); //, ep);

        Debug.Log("Message: " + message +  " has been sent");

        
        // if you are expecting a message, include this: 
        //byte[] b2 = client.Receive(ref ep);
        //string str2 = System.Text.Encoding.ASCII.GetString(b2, 0, b2.Length);
        //Debug.Log("Recieved: " + str2);
    }

    private void Start()
    {
        client = new UdpClient();
        client.Connect(new IPEndPoint(IPAddress.Parse("209.182.232.50"), 3005));
        Debug.Log("Setup is done");
    }
    

    float lastClick = 0;
    private void Update()
    {
        if (Input.GetKey(KeyCode.T) && Time.time - lastClick > 0.5f)
        {
            lastClick = Time.time;
            Debug.Log("You have clicked T");

            for (int i=0; i<100; i++)
            {
                Thread newThread = new Thread(SendMessage);
                newThread.Start();
            }
            

        }
    }

    public static string Encode_XML(object obj_tohide, Type required_type)
    { // Given an object that can be serilized and the type it is
        XmlSerializer serializer = new XmlSerializer(required_type);
        StringWriter sw = new StringWriter();
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        serializer.Serialize(sw, obj_tohide, ns);
        string converted_string = sw.ToString();

        return converted_string;
    }
}