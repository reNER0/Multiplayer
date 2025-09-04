using System.IO;
using System.Net.Sockets;

public class NetworkClient
{
    public int ClientId;
    public int ClientObjectId = -1;
    public TcpClient Client;
    public StreamReader StreamReader;
    public StreamWriter StreamWriter;
}

