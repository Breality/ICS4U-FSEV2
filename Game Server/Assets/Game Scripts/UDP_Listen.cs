using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;

public class UDP_Listen : MonoBehaviour
{
    private UdpClient udpReciever;

    //
    private void Network()
    {
        Debug.Log("Socket for UDP is being initiated");
        udpReciever = new UdpClient(11000);

        try
        {
            udpReciever.Connect("209.182.232.50", 11000);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0); // changed every time we get a new udp message
            Debug.Log("Waiting for UDP request");
            while (true)
            {
                try
                {
                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = udpReciever.Receive(ref RemoteIpEndPoint); // recieve data and change the remote ip
                    string message = System.Text.Encoding.ASCII.GetString(receiveBytes);

                    Debug.Log("This is the message you received " + message.ToString());
                    Debug.Log("This message was sent from " + RemoteIpEndPoint.Address.ToString() + " on their port number " + RemoteIpEndPoint.Port.ToString());
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }

            }

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        //UdpClient udpClientB = new UdpClient();
        //udpClientB.Send(sendBytes, sendBytes.Length, "AlternateHostMachineName", 11000);
    }

    Thread listenerThread;
    void Start()
    {
        listenerThread = new Thread(Network);
        listenerThread.Start();
    }

    void OnApplicationQuit()
    {
        listenerThread.Abort();
        udpReciever.Close();
        Debug.Log("Closed, we are not taking udp.");
    }
}
