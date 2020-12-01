using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.IO;

public class Server : MonoBehaviour
{
    public int port = 6312;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectList; // Cycled through when disconnecting clients

    private TcpListener server; // The actual server
    private bool serverStarted; // Have we started the server?

    public void init()
    {
        DontDestroyOnLoad(gameObject); // Don't destory it if we are just changing scenes E.g. Menu > Game
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            //Debug.Log("New TCP Listener on " + IPAddress.Any + " " + port);
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            //Debug.Log("LISTENING");
            startListening();
            serverStarted=true; //try moving to other startListening();
            
        }
        catch (Exception e)
        {
            Debug.Log("Socket eror: " + e.Message);
        }
    }
    /*When we create this for linux builds remove monobevaiour and put run the udate() at all times*/
    private void Update()
    {
        if(!serverStarted)
            return;

        //Debug.Log(clients);
        foreach(ServerClient c in clients)
        {
            // Is the client still connected?
            if(!isConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if(s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                        onIncomingData(c, data);
                }
            }
        }
        
        for(int i = 0; i < disconnectList.Count - 1; i++)
        {
            // Tell our player somebody has disconnected
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }
    private void startListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        clients.Add(sc);

        startListening();
        

        Debug.Log("Somebody has connected.");
    }

    private bool isConnected(TcpClient c)
    {
        try
        {
            if(c != null && c.Client != null && c.Client.Connected )
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    // Server send
    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach(ServerClient sc in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message);
            }
        }
    }

    // Server read
    private void onIncomingData(ServerClient c, string data)
    {
        Debug.Log(c.clientName + " : " + data);
    }
}

public class ServerClient
{
    public string clientName;
    public TcpClient tcp;

    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}