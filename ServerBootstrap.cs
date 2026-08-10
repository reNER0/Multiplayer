using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServerBootstrap
{
    private static ServerConfig config;


    public static ServerConfig GetConfig()
    {
        if (config == null)
            config = ParseConfig();

        return config;
    }

    private static ServerConfig ParseConfig()
    {
        var args = Environment.GetCommandLineArgs();
        var dict = ParseArgs(args);

        var config = new ServerConfig
        {
            MatchId = GetString(dict, "--matchId", "dev_match"),
            BindIp = GetString(dict, "--bindIp", "localhost"),
            Secret = GetString(dict, "--secret", ""),
            Port = GetInt(dict, "--port", NetworkSettings.ServerPort),
            ExpectedPlayers = GetInt(dict, "--expectedPlayers", 5),
        };

        Debug.Log($"[Dedicated] matchId={config.MatchId} ip={config.BindIp} port={config.Port} expected={config.ExpectedPlayers}");

        return config;
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            var key = args[i];

            if (!key.StartsWith("--"))
                continue;

            // флаг без значения
            if (i + 1 >= args.Length || args[i + 1].StartsWith("--"))
            {
                dict[key] = "true";
                continue;
            }

            dict[key] = args[i + 1];
            i++;
        }

        return dict;
    }

    private static string GetString(Dictionary<string, string> d, string key, string fallback)
        => d.TryGetValue(key, out var v) ? v : fallback;

    private static int GetInt(Dictionary<string, string> d, string key, int fallback)
        => d.TryGetValue(key, out var v) && int.TryParse(v, out var n) ? n : fallback;
}

public class ServerConfig
{
    public string MatchId;
    public string BindIp;
    public int Port;
    public string Secret;
    public int ExpectedPlayers;
}
