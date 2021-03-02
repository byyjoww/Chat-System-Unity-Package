using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendData : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();

        if (!Client.IsConnected) { return; }
        Client.Instance.tcp.SendData(_packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(UIManager.Instance.username.text);
            _packet.Write(Client.Instance.id);

            SendTCPData(_packet);
        }
    }

    public static void SendChatMessage(string message, Message.MessageType messageType)
    {
        using (Packet _packet = new Packet((int)ClientPackets.chatMessage))
        {
            _packet.Write((int)messageType);
            _packet.Write(message);

            SendTCPData(_packet);
        }
    }

    public static void SendCommand(int commandId)
    {
        // REQUEST USERS = 0

        using (Packet _packet = new Packet((int)ClientPackets.clientCommand))
        {
            _packet.Write(commandId);

            SendTCPData(_packet);
        }
    }
    #endregion
}