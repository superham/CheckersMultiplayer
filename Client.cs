using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;

public class Client : MonoBehaviour
{
    public string clientName;
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public bool ConnectToServer(string host, int port)
    {

        bool debugV = true;

        if(debugV)
            Debug.Log("Starting connection...");

        if (socketReady)
        {
            if(debugV)
                Debug.Log("WARNING: SOCKET NOT READY. ENDING CONNECTION ATTEMPT.");
            return false;
        }
        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;
            if(debugV)
                Debug.Log("SOCKET READY.");
        }
        catch (Exception e)
        {
            Debug.Log("Socket error" + e.Message);
        }

        return socketReady;
    }

    private void update()
    {
        if(socketReady)
        {
            if(stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
    }

    // Sending messages to the server
    public void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    // Read messages from the server
    private void OnIncomingData(string data)
    {
        Debug.Log(data);
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }
}

public class GameClient
{
    public string name;
    public bool isHost;
}
