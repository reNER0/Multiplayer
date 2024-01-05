using Assets.Scripts.Network;
using Assets.Scripts.Network.Commands;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows;

public class ServerHub : Hub
{
    private static TcpListener tcpListener;

    private bool _disposed = false;


    private void Awake()
    {
        Application.runInBackground = true;

        NetworkBus.OnCommandSendToClient += SendCommandToClient;
        NetworkBus.OnCommandSendToClients += SendCommandToAllClients;
        NetworkBus.OnPerformCommand += PerformCommand;

        ConnectingClientsLoopTask();
    }


    private async Task ConnectingClientsLoopTask()
    {
        Debug.Log("Starting server socket");

        tcpListener = new TcpListener(IPAddress.Any, _port);
        tcpListener.Start();

        while (!_disposed)
        {
            try
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                client.NoDelay = true;
                Console.WriteLine("The delay was set successfully to " + client.NoDelay.ToString());
                client.ReceiveBufferSize = 16384;
                client.SendBufferSize = 16384;

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
        var availableId = NetworkRepository.ConnectedClients.Count;

        var connectedClient = new NetworkClient
        {
            ClientId = availableId,
            Client = client,
            StreamReader = new StreamReader(client.GetStream()),
            StreamWriter = new StreamWriter(client.GetStream()),
        };

        connectedClient.StreamWriter.AutoFlush = true;

        const string FMT = "O";
        DateTime now1 = NetworkSettings.ServerStartupTime;
        string strDate = now1.ToString(FMT);
        var initCmd = new InitClientCmd(availableId, strDate, NetworkSettings.CurrentTick);

        Debug.Log($"Server Utc: {now1}");
        Debug.Log($"Tick: {NetworkSettings.CurrentTick}");

        SendCommandToClient(initCmd, connectedClient);

        foreach (var cmd in ServerRepository.GetCommands().Where(x => x.GetType().Equals(typeof(SpawnCmd))))
        {
            SendCommandToClient(cmd, connectedClient);
        }

        NetworkRepository.ConnectedClients.Add(connectedClient);

        ClientReadingTask(connectedClient);

        Debug.Log($"{client.Client.RemoteEndPoint} connected!");

        NetworkBus.OnClientConnected?.Invoke(connectedClient);
    }

    private async Task ClientReadingTask(NetworkClient client)
    {
        while (!_disposed)
        {
            if (client.Client.Connected == false)
            {
                NetworkBus.OnClientDisconnected?.Invoke(client);
                NetworkRepository.ConnectedClients.Remove(client);
                Debug.LogError("Disconnected player: " + client.ClientId);
                return;
            }

            var data = await client.StreamReader.ReadLineAsync();

            var cmd = StringToCommand(data);

            PerformCommand(cmd);
        }
    }

    public async void PerformCommand(ICommand cmd)
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

    public void SendCommandToClient(ICommand cmd, NetworkClient client)
    {
        var data = CommandToString(cmd);

        client.StreamWriter.WriteLine(data);
        client.StreamWriter.Flush();
    }

    public void SendCommandToAllClients(ICommand cmd)
    {
        foreach (var client in NetworkRepository.ConnectedClients)
        {
            SendCommandToClient(cmd, client);
        }
    }

    public void SendCommandToAllClientsExcept(ICommand cmd, NetworkClient exceptClient)
    {
        foreach (var client in NetworkRepository.ConnectedClients)
        {
            if (client.ClientId == exceptClient.ClientId)
                continue;

            SendCommandToClient(cmd, client);
        }
    }

    public void DisconnectClient(NetworkClient client)
    {
        client.Client.Close();
    }

    public void DisconnectAllClients()
    {
        foreach (var client in NetworkRepository.ConnectedClients)
        {
            DisconnectClient(client);
        }
    }

    public void OnDestroy()
    {
        _disposed = true;

        DisconnectAllClients();

        NetworkBus.OnCommandSendToClient -= SendCommandToClient;
        NetworkBus.OnCommandSendToClients -= SendCommandToAllClients;

        NetworkBus.OnPerformCommand -= PerformCommand;

        tcpListener?.Stop();
    }
}
