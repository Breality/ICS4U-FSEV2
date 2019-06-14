using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDP_Listen : MonoBehaviour
{
    UdpClient client;
    int receivePort = 11000;
    string serverIP;
    IPAddress groupIP = IPAddress.Parse("209.182.232.50");
    IPEndPoint remoteEP;

    void Start()
    {
        Debug.Log("Starting UDP Client");
        remoteEP = new IPEndPoint(IPAddress.Any, receivePort);

        client = new UdpClient(remoteEP);
        client.JoinMulticastGroup(groupIP);

        client.BeginReceive(new AsyncCallback(ReceiveServerInfo), null);

    }
    void ReceiveServerInfo(IAsyncResult result)
    {
        Debug.Log("Received Server Info");
        byte[] receivedBytes = client.EndReceive(result, ref remoteEP);
        serverIP = Encoding.ASCII.GetString(receivedBytes);
    }
}