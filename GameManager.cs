using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

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

    public GameObject serverPrefab;
    public GameObject clientPrefab;

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
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.init();
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
        string hostAddress = GameObject.Find("Host Input").GetComponent<InputField>().text;
        if (hostAddress == "")
            hostAddress = "127.0.0.1";

        try
        {
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.ConnectToServer(hostAddress, 6321);
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
    }

    public void toggleForceJump()
    {
        // Figure out how to send the state of the button to the game scene
    }
}
