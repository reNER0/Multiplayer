using Assets.Scripts.Network.Commands;
using System;
using System.Collections.Generic;
using System.Linq;


public static class ServerRepository
{
    private static List<CommandTimeFrame> _commandsHistory = new List<CommandTimeFrame>();


    public static void AddCommandInCommandsTimeline(ICommand cmd)
    {
        var cmdTimeFrame = new CommandTimeFrame
        {
            Command = cmd,
            Time = DateTime.Now,
        };

        _commandsHistory.Add(cmdTimeFrame);
    }

    public static ICommand[] GetCommands()
    {
        return _commandsHistory.Select(x => x.Command).ToArray();
    }
}

public struct CommandTimeFrame
{
    public ICommand Command;
    public DateTime Time;
}