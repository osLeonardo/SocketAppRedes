using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ClientProgram
{
    static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    const string IP = "192.168.101.7";
    const int PORT = 8080;

    static void Main(string[] args)
    {
        Console.Title = "Client";
        LoopConnect();
        SendLoop();
        Console.ReadLine();
        _clientSocket.Shutdown(SocketShutdown.Both);
        _clientSocket.Close();
    }

    private static void LoopConnect()
    {
        int attempts = 0;

        while (!_clientSocket.Connected)
        {
            try
            {
                attempts++;
                _clientSocket.Connect(IPAddress.Parse(IP), PORT);
            }
            catch (SocketException)
            {
                Console.Clear();
                Console.WriteLine("Connection attempts: " + attempts);
            }
        }

        Console.Clear();
        Console.WriteLine("Connected");
        Thread receiveThread = new Thread(ReceiveLoop);
        receiveThread.Start();
    }

    private static void SendLoop()
    {
        while (true)
        {
            string message = Console.ReadLine();
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            _clientSocket.Send(buffer);
        }
    }

    private static void ReceiveLoop()
    {
        while (true)
        {
            byte[] receivedBuffer = new byte[1024];
            int received = _clientSocket.Receive(receivedBuffer);
            if (received > 0)
            {
                byte[] data = new byte[received];
                Array.Copy(receivedBuffer, data, received);
                string text = Encoding.ASCII.GetString(data);
                Console.WriteLine(text);
            }
        }
    }
}