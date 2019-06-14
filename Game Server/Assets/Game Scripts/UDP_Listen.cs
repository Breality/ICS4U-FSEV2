using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDP_Listen : MonoBehaviour
{

    UDPListener listener;
    void Start()
    {

        listener = new UDPListener();
        listener.Main();
    }
    
}