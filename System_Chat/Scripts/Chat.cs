using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chat : Singleton<Chat>
{
    [SerializeField] private Transform chatPanel;
    [SerializeField] private GameObject pfTextContainer;
    private bool isSending { get; set; }

    [SerializeField] private const int MAX_MESSAGES = 25;
    [SerializeField] private List<Message> messageList = new List<Message>();
    [SerializeField] private TMP_InputField input;
    
    public event Action OnMessageSend;

    private void Start()
    {
        isSending = false;
    }

    public void Send()
    {
        if (input.text == "") { Debug.Log("Empty message."); return; }
        if (isSending) { Debug.Log("Currently sending message."); return; }        

        isSending = true;

        string msg = input.text;

        SendMessageToChat("You: " + msg, Message.MessageType.SelfMessage);
        ResetInputField();

        SendData.SendChatMessage(msg, Message.MessageType.PlayerMessage);

        isSending = false;
    }

    public static void ReceiveMessageFromServer(string message, string sender, int messageType)
    {
        Debug.Log("Received new message from server.");

        string msg = $"{sender}: {message}";
        var type = (Message.MessageType)messageType;

        ThreadManager.ExecuteOnMainThread(() => Instance.SendMessageToChat(msg, type));
    }

    public static void SendLocalMessage(string message, string sender, int messageType)
    {
        Debug.Log("Received new local message.");

        string msg = $"{sender}: {message}";
        var type = (Message.MessageType)messageType;

        ThreadManager.ExecuteOnMainThread(() => Instance.SendMessageToChat(msg, type));        
    }

    private void SendMessageToChat(string text, Message.MessageType messageType)
    {
        if(messageList.Count >= MAX_MESSAGES)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.Remove(messageList[0]);
            Debug.Log("Max messages in list, removing message at index 0.");
        }

        // Create messaage
        Message newMessage = new Message();
        newMessage.text = text;
        GameObject textObject = Instantiate(pfTextContainer, chatPanel);

        // Set text
        newMessage.textObject = textObject.transform.GetChild(0).GetComponent<TMP_Text>();
        newMessage.textObject.text = newMessage.text;

        // Apply color
        newMessage.messageType = messageType;
        newMessage.textObject.color = newMessage.messageColor;

        // Save reference and fire event
        messageList.Add(newMessage);
        OnMessageSend?.Invoke();
    }

    private void ResetInputField()
    {
        input.text = "";
    }    
}

[System.Serializable]
public class Message
{
    public string text;
    public TMP_Text textObject;
    public MessageType messageType;

    // COLORS
    public Color messageColor => messageColorDictionary[messageType];
    private Dictionary<Message.MessageType, Color> messageColorDictionary;

    public Message()
    {
        messageColorDictionary = new Dictionary<Message.MessageType, Color>()
        {
            { MessageType.SelfMessage, Color.black },
            { MessageType.PlayerMessage, Color.blue },
            { MessageType.ServerMessage, Color.yellow },
            { MessageType.SystemMessage, Color.green },
        };
    }

    public enum MessageType
    {
        SelfMessage = 0,
        PlayerMessage = 1,
        ServerMessage = 2,
        SystemMessage = 4,
    }
}
