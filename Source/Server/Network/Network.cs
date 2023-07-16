﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GameServer.Managers;

namespace GameServer
{
    public static class Network
    {
        public static List<Client> connectedClients = new List<Client>();
        private static TcpListener server;
        private static IPAddress localAddress = IPAddress.Parse(Program.serverConfig.IP);
        private static int port = int.Parse(Program.serverConfig.Port);

        public static bool isServerOpen;

        public static void ReadyServer()
        {
            server = new TcpListener(localAddress, port);
            server.Start();
            isServerOpen = true;

            Threader.GenerateServerThread(Threader.ServerMode.Heartbeat);
            Threader.GenerateServerThread(Threader.ServerMode.Sites);
            Logger.WriteToConsole($"Listening for users at {localAddress}:{port}");
            Logger.WriteToConsole("Server launched");
            Titler.ChangeTitle();

            while (true) ListenForIncomingUsers();
        }

        private static void ListenForIncomingUsers()
        {
            Client newServerClient = new Client(server.AcceptTcpClient());

            if (connectedClients.ToArray().Count() >= int.Parse(Program.serverConfig.MaxPlayers))
            {
                //FIXME
                //LOOP?

                UserManager_Joinings.SendLoginResponse(newServerClient, UserManager_Joinings.LoginResponse.ServerFull);
                Logger.WriteToConsole($"[Disconnect] > {newServerClient.username} | {newServerClient.SavedIP} > Server Full", Logger.LogMode.Warning);
            }

            else
            {
                connectedClients.Add(newServerClient);

                Titler.ChangeTitle();

                Threader.GenerateClientThread(Threader.ClientMode.Start, newServerClient);

                Logger.WriteToConsole($"[Connect] > {newServerClient.username} | {newServerClient.SavedIP}");
            }
        }

        public static void ListenToClient(Client client)
        {
            try
            {
                while (!client.disconnectFlag)
                {
                    string data = client.streamReader.ReadLine();
                    if (data == null) break;

                    Packet receivedPacket = Serializer.SerializeToPacket(data);
                    if (receivedPacket == null) break;

                    try { PacketHandler.HandlePacket(client, receivedPacket); }
                    catch { ResponseShortcutManager.SendIllegalPacket(client, true); }
                }
            }
            catch { }
        }

        public static void SendData(Client client, Packet packet)
        {
            while (client.isBusy) Thread.Sleep(100);

            try
            {
                client.isBusy = true;

                client.streamWriter.WriteLine(Serializer.SerializeToString(packet));
                client.streamWriter.Flush();

                client.isBusy = false;
            }
            catch { }
        }

        public static void KickClient(Client client)
        {
            client.disconnectFlag = true;
            connectedClients.Remove(client);
            client.tcp.Dispose();

            UserManager.SendPlayerRecount();

            Titler.ChangeTitle();

            Logger.WriteToConsole($"[Disconnect] > {client.username} | {client.SavedIP}");
        }

        public static void HearbeatClients()
        {
            while (true)
            {
                Thread.Sleep(100);

                try
                {
                    Client[] actualClients = connectedClients.ToArray();
                    foreach (Client client in actualClients)
                    {
                        if (!CheckIfConnected(client) || client.disconnectFlag)
                        {
                            KickClient(client);
                        }
                    }
                }

                catch { }
            }
        }

        private static bool CheckIfConnected(Client client)
        {
            if (!client.tcp.Connected) return false;
            else
            {
                if (client.tcp.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (client.tcp.Client.Receive(buff, SocketFlags.Peek) == 0) return false;
                    else return true;
                }

                else return true;
            }
        }
    }
}
