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
    [SerializeField]
    private int pingDelayInMilliseconds = 500;

    private static TcpClient client;
    private static StreamReader _streamReader;
    private static StreamWriter _streamWriter;

    private static float ping = int.MaxValue;
    private static DateTime lastPingSentTime = DateTime.Now;

    private Thread _serverListenerThread;
    private ConcurrentQueue<ICommand> _cmds = new();
    private bool _disposed = false;


    public static float Ping => ping;
    public static DateTime LastPingSentTime => lastPingSentTime;


    private void Awake()
    {
        Application.runInBackground = true;

        ConnectClient();

        NetworkBus.OnPerformCommand += PerformCommand;
        NetworkBus.OnCommandSendToServer += SendCommandToServer;

        NetworkBus.OnPingUpdated += OnPingUpdated;

        NetworkBus.OnLocalClientDisconnected += SceneLoader.LoadMainMenuScene;
    }


    public async void PerformCommand(ICommand cmd)
    {
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
        while (!_disposed)
        {
            try
            {
                if (client?.Connected == true)
                {
                    var data = await _streamReader.ReadLineAsync();

                    var cmd = StringToCommand(data);

                    if (cmd == null)
                    {
                        Debug.LogError("Can`t parse Command!");
                        HandleDisconnect();
                        return;
                    }

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
                HandleDisconnect();
                return;
            }
        }
    }

    private void HandleDisconnect(Exception reason = null)
    {
        if (reason != null)
            Debug.LogError(reason);

        NetworkBus.OnLocalClientDisconnected?.Invoke();
        client.Close();
    }

    private async void OnPingUpdated(int newPing)
    {
        ping = newPing;

        // Delay between pings;
        await Task.Delay(pingDelayInMilliseconds);

        SendPing();
    }

    public static void SendPing()
    {
        lastPingSentTime = DateTime.Now;
        NetworkBus.OnCommandSendToServer?.Invoke(new PingCmd());
    }

    private void OnDestroy()
    {
        _disposed = true;

        client?.Close();
        _serverListenerThread.Abort();

        NetworkBus.OnPerformCommand -= PerformCommand;
        NetworkBus.OnCommandSendToServer -= SendCommandToServer;

        NetworkBus.OnPingUpdated -= OnPingUpdated;

        NetworkBus.OnLocalClientDisconnected -= SceneLoader.LoadMainMenuScene;
    }

    public void Update()
    {
        ProcessCmds();
    }
}