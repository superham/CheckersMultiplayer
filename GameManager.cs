using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.Net;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    { set;
      get;
    }

    public GameObject mainMenu;
    public GameObject serverMenu;
    public GameObject connectMenu;
    public GameObject forceJumpToggle;

    public GameObject hostIPText;

    public GameObject serverPrefab;
    public GameObject clientPrefab;
    public InputField nameInput;

    private void Start()
    { 
        Instance = this;
        mainMenu.SetActive(true);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        DontDestroyOnLoad(gameObject);
    }

    public void ConnectButton()
    {
        //Debug.Log("Connect");
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }

    public void HostButton()
    {
        try
        {
            Debug.Log("creating server...");
            string externalip = new WebClient().DownloadString("http://icanhazip.com");            
            hostIPText.GetComponent<Text>().text = externalip.ToString();
            
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.init();

            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = nameInput.text;
            if(c.clientName == "")
                c.clientName = "Host";
            
            c.ConnectToServer("127.0.0.1", 7412);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }

        //Debug.Log("Host");
        mainMenu.SetActive(false);
        serverMenu.SetActive(true);


        // Get the host's IP address adn display it
    }

    public void OfflineButton()
    {
        Debug.Log("Offline");
        SceneManager.LoadScene("game");
    }

    public void ConnectToServerButton()
    {
        string hostAddress = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (hostAddress == "")
            hostAddress = "127.0.0.1";

        try
        {
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = nameInput.text;
            if(c.clientName == "")
                c.clientName = "Client";
            c.ConnectToServer(hostAddress, 7412);
            connectMenu.SetActive(false);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }

        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }

    public void BackButton()
    {
        mainMenu.SetActive(true);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);

        Server s = FindObjectOfType<Server>();
        if(s != null)
            Destroy(s.gameObject);

        Client c = FindObjectOfType<Client>();
        if(c != null)
            Destroy(c.gameObject);
    }

    public void toggleForceJump()
    {
        // Figure out how to send the state of the button to the game scene
    }
}
