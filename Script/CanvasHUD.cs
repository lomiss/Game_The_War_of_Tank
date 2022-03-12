using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class CanvasHUD : MonoBehaviour
{
    public Button buttonHost, buttonClinet;
    
    public void Start()
    {
        buttonHost.onClick.AddListener(ButtonHost);
        buttonClinet.onClick.AddListener(ButtonClinet);
    }

    public void ButtonHost()
    {
        NetworkManager.singleton.StartHost();
    }

    public void ButtonClinet()
    {
        NetworkManager.singleton.StartClient();
    }
    
}
