using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

class ClientProgram
{
    static List<Socket> _clientSockets = new();
    static Socket _serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    const string SERVER_IP = "192.168.101.7";
    const int PORT = 8080;

    static void Main(string[] args)
    {
        Console.Title = "Server";
        SetupServer();
        Console.ReadLine();
        CloseAllSockets();
    }

    private static void SetupServer()
    {
        Console.WriteLine("Setting up server...");
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), PORT);
        _serverSocket.Bind(endPoint);
        _serverSocket.Listen(5);
        _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        Console.WriteLine("Server setup complete on: " + endPoint);
    }

    private static void AcceptCallback(IAsyncResult result)
    {
        Socket socket = _serverSocket.EndAccept(result);
        _clientSockets.Add(socket);
        socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, ReceiveCallback, socket);
        Console.WriteLine("Client connected");
        _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
    }

    private static void ReceiveCallback(IAsyncResult result)
    {
        Socket current = (Socket)result.AsyncState;
        int received;
        try { received = current.EndReceive(result); }
        catch (SocketException)
        {
            Console.WriteLine("Client forcibly disconnected");
            current.Close();
            _clientSockets.Remove(current);
            return;
        }

        if (received > 0)
        {
            byte[] buffer = new byte[received];
            current.Receive(buffer, buffer.Length, SocketFlags.None);
            string text = Encoding.ASCII.GetString(buffer);
            Console.WriteLine("Received Text: " + text);

            string response = $"{current.RemoteEndPoint} - {DateTime.Now}: {text}";
            byte[] data = Encoding.ASCII.GetBytes(response);
        
            foreach (Socket socket in _clientSockets) { socket.Send(data); }
        }

        current.BeginReceive(new byte[] { 0 }, 0, 0, 0, ReceiveCallback, current);
    }

    private static void CloseAllSockets()
    {
        foreach (Socket socket in _clientSockets)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        _serverSocket.Close();
    }
}