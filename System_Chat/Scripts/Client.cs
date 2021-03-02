using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using GameServer;
using System.Text;

public class Client : Singleton<Client>
{
    public static int dataBufferSize = 4096;

    public const string IP_ADDRESS = "79.125.17.207";
    public const int PORT = 8080;

    public int id;
    public TCP tcp;

    public static bool IsConnected => Instance.tcp != null && Instance.tcp.socket != null && Instance.tcp.socket.Connected;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    public void ConnectToServer(string addr = IP_ADDRESS)
    {
        Debug.Log($"Calling tcp.Connect() | Socket: {addr}:{PORT}");
        InitializeClientData();
        tcp = new TCP();
        tcp.Connect(addr, PORT);
    }

    public void Disconnect()
    {
        Debug.Log("Disconnected from the server.");
        Chat.SendLocalMessage("Session ended.", "System", 4);
        if (IsConnected) { tcp.socket.Close(); }        
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP()
        {

        }

        public void Connect(string addr, int port)
        {
            Debug.Log($"Starting client on socket {addr}:{port}.");

            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(addr, port, ConnectCallback, socket);            
        }        

        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);

            if (!socket.Connected)
            {
                Debug.Log($"Unable to connect to server.");
                Chat.SendLocalMessage("Error: Unable to connect to the server.", "System", 4);
                return;
            }            

            stream = socket.GetStream();

            receivedData = new Packet();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            Chat.SendLocalMessage("Successfully connected to the server.", "System", 4);
            Debug.Log($"Successfully connected to server. System IsLittleEndian: {BitConverter.IsLittleEndian}");            
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}.");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0) { return; }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                // Instance.PrintBytes(packetBytes);

                receivedData.Reset(HandleData(data));

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error receiving TCP data:{ex}.");

                // TODO: disconnect
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4)
            {                
                packetLength = receivedData.ReadInt();

                if (packetLength <= 0)
                {
                    Debug.Log("Reset Handler, packet length = 0");
                    return true;
                }
            }

            Debug.Log($"Unread Data Size: {receivedData.UnreadLength()} | Packet Length: {packetLength}");

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);

                // Instance.PrintBytes(packetBytes);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Debug.Log("Packet ID:" + packetId);
                        packetHandlers[packetId](packet);
                    }
                });

                packetLength = 0;

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();

                    if (packetLength <= 0)
                    {
                        // Debug.Log("Reset Handler, packet length = 0");
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                // Debug.Log("Reset Handler!");
                return true;
            }

            // Debug.Log("Don't Reset Handler Yet");
            return false;
        }
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { 0, ReadData.Echo },
            { 1, ReadData.Welcome },
            { 2, ReadData.ChatMessage },
            { 3, ReadData.ConnectedUsers },
        };

        Debug.Log("Initialized Packets.");
    }

    public void PrintBytes(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var d in bytes)
        {
            sb.Append(d + " ");
        }

        Debug.Log(sb.ToString());
    }

    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        Disconnect();
#endif
    }
}