using System.IO;
using System.Net.Sockets;

public class NetworkClient
{
    public int ClientId;
    public TcpClient Client;
    public StreamReader StreamReader;
    public StreamWriter StreamWriter;
}

