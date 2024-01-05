using Assets.Scripts.Network.Commands;
using Assets.Scripts.Network;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using ThreadPriority = System.Threading.ThreadPriority;

public class ClientHub : Hub
{
    private static TcpClient client;
    private static StreamReader _streamReader;
    private static StreamWriter _streamWriter;

    private DateTime lastPingTime = DateTime.Now;



    private Thread _serverListenerThread;
    private ConcurrentQueue<ICommand> _cmds = new();
    private bool _disposed = false;



    private void Awake()
    {
#if UNITY_SERVER
            return;
#endif

        Application.runInBackground = true;

        ConnectClient();

        NetworkBus.OnPerformCommand += PerformCommand;
        NetworkBus.OnCommandSendToServer += SendCommandToServer;

        NetworkBus.OnPongReceived += SendPing;

        Application.runInBackground = true;
    }

    public async void PerformCommand(ICommand cmd)
    {
        //await Task.Delay(80);

        cmd.Execute();
    }

    public async void SendCommandToServer(ICommand cmd)
    {
        var data = CommandToString(cmd);

        try
        {
            _streamWriter.WriteLine(data);
            _streamWriter.Flush();
        }
        catch (Exception e)
        {
            client?.Close();
            Debug.LogError(e);
            return;
        }
    }

    private async Task ConnectClient()
    {
        while (true)
        {
            try
            {
                Debug.Log("Starting client socket");

                client = new TcpClient();
                client.NoDelay = true;
                client.ReceiveBufferSize = 16384;
                client.SendBufferSize = 16384;

                await client.ConnectAsync(NetworkSettings.ServerIP, _port);

                _streamReader = new StreamReader(client.GetStream());
                _streamWriter = new StreamWriter(client.GetStream());

                _streamWriter.AutoFlush = true;

                // Create client loop thread here

                _serverListenerThread = new Thread(ListenServerLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal,
                };
                _serverListenerThread.Start();

                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }
        }
    }

    private void ProcessCmds()
    {
        while (_cmds.Any())
        {
            try
            {
                _cmds.TryDequeue(out var command);

                PerformCommand(command);
            }
            catch(Exception e) 
            {
                Debug.LogError(e.Message);
            }
        }
    }

    private async void ListenServerLoop()
    {
        SendPing();

        while (!_disposed)
        {
            try
            {
                if (client?.Connected == true)
                {
                    var data = await _streamReader.ReadLineAsync();

                    var cmd = StringToCommand(data);
                    _cmds.Enqueue(cmd);
                }
                else
                {
                    Debug.Log("Client disconnected!");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                return;
            }
        }
    }

    private async void SendPing()
    {
        var ping = (DateTime.Now - lastPingTime).TotalMilliseconds;
        NetworkBus.OnPingUpdated?.Invoke((int)ping);

        // Delay between pings;
        await Task.Delay(500);

        lastPingTime = DateTime.Now;
        SendCommandToServer(new PingCmd());
    }

    private void OnDestroy()
    {
        _disposed = true;

        client?.Close();
        _serverListenerThread.Abort();

        NetworkBus.OnPerformCommand -= PerformCommand;
        NetworkBus.OnCommandSendToServer -= SendCommandToServer;

        NetworkBus.OnPongReceived -= SendPing;
    }

    public void Update()
    {
        ProcessCmds();
    }
}