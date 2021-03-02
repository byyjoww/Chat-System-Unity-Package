using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameServer;

public class ReadData : MonoBehaviour
{
    #region Packets
    public static void Echo(Packet packet)
    {
        string message = packet.ReadString();

        Debug.Log($"Server received input: {message}");
    }

    public static void Welcome (Packet packet)
    {
        string message = packet.ReadString();
        int id = packet.ReadInt();

        Debug.Log($"Message from server: {message}");

        Client.Instance.id = id;
        
        SendData.WelcomeReceived();
    }

    public static void ChatMessage(Packet packet)
    {
        int messageType = packet.ReadInt();
        string sender = packet.ReadString();
        string message = packet.ReadString();

        Chat.ReceiveMessageFromServer(message, sender, messageType);
    }

    public static void ConnectedUsers(Packet packet)
    {
        int numUsers = packet.ReadInt();

        string[] usernames = new string[numUsers];
        int[] userIds = new int[numUsers];

        for (int i = 0; i < numUsers; i++)
        {
            var v = packet.ReadString();
            usernames[i] = v;
        }

        for (int i = 0; i < numUsers; i++)
        {
            var v = packet.ReadInt();
            userIds[i] = v;
        }        

        UserList.ReceiveConnectedUsersFromServer(usernames, userIds, numUsers);
    }
    #endregion
}
