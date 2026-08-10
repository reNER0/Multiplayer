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
using UnityEngine.UIElements;

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

    private static SynchronizationContext unityContext;


    private void Awake()
    {
        unityContext = SynchronizationContext.Current;

        Application.runInBackground = true;

        ConnectClient();

        NetworkBus.OnPerformCommand += PerformCommand;
        NetworkBus.OnCommandSendToServer += SendCommandToServer;

        NetworkBus.OnPingUpdated += OnPingUpdated;

        NetworkBus.OnLocalClientDisconnected += OnDisconnect;
    }


    public async void PerformCommand(ICommand cmd)
    {
        cmd.Execute();
    }


    public async void SendCommandToServer(ICommand cmd)
    {
        if (_streamWriter == null || client == null || !client.Connected)
            return;

        var data = CommandToString(cmd);

        try
        {
            int delay = NetworkSettings.AdditivePing + UnityEngine.Random.Range(-NetworkSettings.AdditiveJitter, NetworkSettings.AdditiveJitter + 1);
            delay = Mathf.Max(0, delay);

            await Task.Delay(delay);

            _streamWriter.WriteLine(data);
        }
        catch
        {
            HandleDisconnect();
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

                await client.ConnectAsync(NetworkSettings.ServerIP, NetworkSettings.ServerPort);

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
                await Task.Delay(1000);
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
        try
        {
            while (!_disposed)
            {
                var data = await _streamReader.ReadLineAsync();

                if (data == null)
                {
                    HandleDisconnect();
                    return;
                }

                var cmd = StringToCommand(data);

                if (cmd == null)
                {
                    HandleDisconnect(new InvalidDataException("Can't parse command"));
                    return;
                }

                _cmds.Enqueue(cmd);
            }
        }
        catch (ObjectDisposedException e)
        {
            // Мы сами закрыли стрим при выходе/смене сцены — это ок
            HandleDisconnect(e);
            return;
        }
        catch (IOException e)
        {
            // Сетевой разрыв — это ок
            HandleDisconnect(e);
            return;
        }
        catch (SocketException e)
        {
            // Сетевой разрыв — это ок
            HandleDisconnect(e);
            return;
        }
        catch (Exception e)
        {
            // Реально неожиданное
            HandleDisconnect(e);
            return;
        }
    }


    private int _disconnectOnce;

    private void HandleDisconnect(Exception reason = null)
    {
        if (Interlocked.Exchange(ref _disconnectOnce, 1) == 1)
            return;

        if (reason != null)
            Debug.LogWarning(reason);

        unityContext.Post(_ => NetworkBus.OnLocalClientDisconnected?.Invoke(), null);

        NetworkRepository.Reset();
    }

    public static void OnDisconnect()
    {
        client.Close();
        SceneLoader.LoadMainMenuScene();
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

        NetworkBus.OnLocalClientDisconnected -= OnDisconnect;
    }

    public void Update()
    {
        ProcessCmds();
    }
}