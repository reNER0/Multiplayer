using Assets.Scripts.Network.Commands;
using System;
using UnityEngine;

public static class NetworkBus
{
    public static Action OnGameReadyChecked;
    public static Action<ICommand> OnPerformCommand;
    public static Action<ICommand> OnCommandSendToServer;
    public static Action<ICommand, NetworkClient> OnCommandSendToClient;
    public static Action<ICommand> OnCommandSendToClients;
    public static Action<NetworkClient> OnClientConnected;
    public static Action<NetworkClient> OnClientDisconnected;
    public static Action<int> OnInputsSetToTick;
    public static Action<int> OnAllStatesSaved;

    public static Action<Predictable, PlayerInputs> OnPredictableInput;
    public static Action<Predictable> OnPredictableSpawned;

    public static Action<int> OnPingUpdated;
}