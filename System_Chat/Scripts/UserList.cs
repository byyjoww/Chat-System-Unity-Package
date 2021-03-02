using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UserList : Singleton<UserList>
{
    [SerializeField] private Transform panel;
    [SerializeField] private GameObject pfTextContainer;

    public bool IsLoading { get; set; }

    [SerializeField] private const int MAX_USERS = 8;
    [SerializeField] private List<User> connectedUserList;

    public UnityEvent OnUserListStartUpdate;
    public UnityEvent OnUserListEndUpdate;

    private void Start()
    {
        connectedUserList = new List<User>();        
        IsLoading = false;
    }

    private void OnEnable()
    {
        RequestConnectedUsers();
    }

    public void RequestConnectedUsers()
    {
        if (IsLoading) { Debug.Log("Already requesting data from the server."); return; }
        IsLoading = true;

        Debug.Log("Requesting updated user list.");
        SendData.SendCommand(0);

        OnUserListStartUpdate?.Invoke();
    }

    public static void ReceiveConnectedUsersFromServer(string[] usernames, int[] ids, int numOfusers)
    {
        Debug.Log("Received updated user list.");

        ThreadManager.ExecuteOnMainThread(() => Instance.UpdateUserList(usernames, ids, numOfusers));        
    }

    private void UpdateUserList(string[] usernames, int[] ids, int numOfusers)
    {
        if (usernames.Length >= MAX_USERS)
        {
            Debug.LogError("Hit max users limit!");
        }

        foreach (var c in connectedUserList)
        {
            if (c == null) { Debug.LogError("Connected user is null!"); }
            else { Destroy(c.textComponent.transform.parent.gameObject); }                   
        }
        connectedUserList.Clear();

        for (int i = 0; i < usernames.Length; i++)
        {
            GameObject obj = Instantiate(pfTextContainer, panel);
            var textComponent = obj.transform.GetChild(0).GetComponent<TMP_Text>();
            User newUser = new User(usernames[i], ids[i], textComponent);
            connectedUserList.Add(newUser);
        }

        IsLoading = false;
        OnUserListEndUpdate?.Invoke();
    }
}

public class User
{
    public int userID;
    public string username;
    public TMP_Text textComponent;
    public Color textColor = Color.black;

    public User(string name, int id, TMP_Text component)
    {
        username = name;
        userID = id;
        textComponent = component;

        textComponent.text = $"ID: {userID} - {username}";
        textComponent.color = textColor;
    }
}