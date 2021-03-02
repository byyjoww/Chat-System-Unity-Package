using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject connectionPanel;
    public GameObject userList;

    public TMP_InputField username;
    public TMP_InputField address;

    private void Start()
    {
        connectionPanel.SetActive(true);
        userList.SetActive(false);
    }

    public void ConnectToServer()
    {
        connectionPanel.SetActive(false);
        username.interactable = false;
        address.interactable = false;

        if(address.text != "") { Client.Instance.ConnectToServer(address.text); }
        else { Client.Instance.ConnectToServer(); }
    }

    public void Disconnect()
    {
        username.text = "";
        address.text = Client.IP_ADDRESS;
        
        connectionPanel.SetActive(true);
        username.interactable = true;
        address.interactable = true;
    }

    public void Quit()
    {
        Disconnect();
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
