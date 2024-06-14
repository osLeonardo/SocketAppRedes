using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

class HostProgram
{
    private static TcpListener _server = new(GetLocalIpAddress(), 8000);
    private static List<TcpClient> _clients = new();

    static void Main(string[] args)
    {
        new HostProgram().Start();
    }

    public void Start()
    {
        _server.Start();
        Console.Clear();
        Console.WriteLine("Server started on " + GetLocalIpAddress() + ":8000");

        while (true)
        {
            TcpClient client = _server.AcceptTcpClient();
            _clients.Add(client);

            Console.WriteLine(client.Client.RemoteEndPoint + " connected to the server.");
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private void HandleClient(TcpClient client)
    {
        while (true)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
            {
                Console.WriteLine(client.Client.RemoteEndPoint + " disconnected from the server.");
                _clients.Remove(client);
                client.Close();
                break;
            }

            string username = client.Client.RemoteEndPoint.ToString().Split(":")[1];
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            string formattedMessage = $"{username}: {message}";

            BroadcastMessage(formattedMessage, client);
        }
    }

    private void BroadcastMessage(string message, TcpClient senderClient)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);

        foreach (TcpClient client in _clients)
        {
            if (client == senderClient) continue;
            
            NetworkStream stream = client.GetStream();
            stream.Write(buffer, 0, buffer.Length);
        }
    }
    
    public static IPAddress GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) return ip;
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}