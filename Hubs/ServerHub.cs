using Assets.Scripts.Network;
using Assets.Scripts.Network.Commands;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class ServerHub : Hub
{
    private static TcpListener tcpListener;

    private bool _disposed = false;

    private int availableId;

    private static CmdRecorder CmdRecorder;


    private void Awake()
    {
        CmdRecorder = gameObject.AddComponent<CmdRecorder>();
        CmdRecorder.SetIsServer();

        Application.runInBackground = true;

        NetworkBus.OnCommandSendToServer += PerformCommand;
        NetworkBus.OnCommandSendToClient += SendCommandToClient;
        NetworkBus.OnCommandSendToClients += SendCommandToAllClients;
        NetworkBus.OnCommandSendToClientsExcept += SendCommandToAllClientsExcept;
        NetworkBus.OnPerformCommand += PerformCommand;

        ConnectingClientsLoopTask();
    }


    private async void ConnectingClientsLoopTask()
    {
        Debug.Log("Starting server socket");

        var config = ServerBootstrap.GetConfig();
        tcpListener = new TcpListener(IPAddress.Any, config.Port);

        tcpListener.Start();

        while (!_disposed)
        {
            try
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                /*
                client.NoDelay = true;
                Console.WriteLine("The delay was set successfully to " + client.NoDelay.ToString());
                client.ReceiveBufferSize = 16384;
                client.SendBufferSize = 16384;
                */
                AddNewClient(client);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    private void AddNewClient(TcpClient client)
    {
        var connectedClient = new NetworkClient
        {
            ClientId = availableId,
            Client = client,
            StreamReader = new StreamReader(client.GetStream()),
            StreamWriter = new StreamWriter(client.GetStream()),
        };

        connectedClient.StreamWriter.AutoFlush = true;

        var initCmd = new InitClientCmd(availableId, NetworkTime.CurrentTick);

        SendCommandToClient(initCmd, connectedClient);

        foreach (var cmd in ServerRepository.GetCommands().Where(x => publicCommandTypes.Contains(x.GetType())))
        {
            SendCommandToClient(cmd, connectedClient);
        }

        NetworkRepository.Current.ConnectedClients.Add(connectedClient);

        ClientReadingTask(connectedClient);

        Debug.Log($"{client.Client.RemoteEndPoint} connected!");

        NetworkBus.OnClientConnected?.Invoke(connectedClient);

        availableId++;
    }

    private async void ClientReadingTask(NetworkClient client)
    {
        try
        {
            while (!_disposed)
            {
                string data = await client.StreamReader.ReadLineAsync();

                // Клиент корректно закрыл соединение -> data == null
                if (data == null)
                {
                    HandleDisconnect(client);
                    return;
                }

                var cmd = StringToCommand(data);
                if (cmd == null)
                {
                    // Это уже реально странно, но тоже можно просто дисконнектнуть
                    HandleDisconnect(client);
                    return;
                }

                PerformCommand(cmd);
            }
        }
        catch (ObjectDisposedException)
        {
            // Нормально: мы сами закрыли stream
            HandleDisconnect(client);
        }
        catch (IOException)
        {
            // Нормально: разрыв соединения
            HandleDisconnect(client);
        }
        catch (SocketException)
        {
            // Нормально: разрыв сокета
            HandleDisconnect(client);
        }
        catch (Exception e)
        {
            // Вот это уже неожиданное — можно логнуть
            HandleDisconnect(client, e);
        }
    }


    public void PerformCommand(ICommand cmd)
    {
        try
        {
            cmd.Execute();

            if (publicCommandTypes.Contains(cmd.GetType()))
                SendCommandToAllClients(cmd);

            ServerRepository.AddCommandInCommandsTimeline(cmd);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while performing command on server: {e}");
        }
    }

    private void HandleDisconnect(NetworkClient client, Exception reason = null)
    {
        if (client == null)
            return;

        if (reason != null)
            Debug.LogWarning($"Client {client.ClientId} disconnected: {reason.GetType().Name}");

        try { client.StreamWriter?.Dispose(); } catch { }
        try { client.StreamReader?.Dispose(); } catch { }
        try { client.Client?.Close(); } catch { }

        NetworkRepository.Current.ConnectedClients.Remove(client);
        NetworkBus.OnClientDisconnected?.Invoke(client);

        Debug.Log($"Disconnected player: {client.ClientId}");
    }



    public void SendCommandToClient(ICommand cmd, NetworkClient client)
    {
        try
        {
            var data = CommandToString(cmd);

            client.StreamWriter.WriteLine(data);
        }
        catch (Exception ex)
        {
            HandleDisconnect(client, ex);
        }
    }

    public void SendCommandToAllClients(ICommand cmd)
    {
        if (cmd.GetType() == typeof(SyncPredictablesCmd))
        {
            CmdRecorder.RecordCmd(cmd);
        }

        foreach (var client in NetworkRepository.Current.ConnectedClients)
        {
            SendCommandToClient(cmd, client);
        }
    }

    public void SendCommandToAllClientsExcept(ICommand cmd, NetworkClient exceptClient)
    {
        foreach (var client in NetworkRepository.Current.ConnectedClients)
        {
            if (client.ClientId == exceptClient.ClientId)
                continue;

            SendCommandToClient(cmd, client);
        }
    }

    public static void DisconnectClient(NetworkClient client)
    {
        client.Client.Close();
    }

    public static void DisconnectAllClients()
    {
        foreach (var client in NetworkRepository.Current.ConnectedClients)
        {
            DisconnectClient(client);
        }
    }

    // TODO : refactor dispose
    public void OnDestroy()
    {
        _disposed = true;

        DisconnectAllClients();

        NetworkBus.OnCommandSendToServer -= PerformCommand;

        NetworkBus.OnCommandSendToClient -= SendCommandToClient;
        NetworkBus.OnCommandSendToClients -= SendCommandToAllClients;

        NetworkBus.OnPerformCommand -= PerformCommand;

        tcpListener?.Stop();
    }
}
