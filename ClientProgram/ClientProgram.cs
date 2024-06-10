using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ClientProgram
{
    private TcpClient _client;
    private NetworkStream _stream;

    static void Main(string[] args)
    {
        int attempts = 0;
        while (true)
        {
            try
            {
                attempts++;
                Console.Clear();
                new ClientProgram().Start();
                break;
            }
            catch (SocketException)
            {
                Console.WriteLine($"Connection attempt {attempts} failed. Retrying in 5 seconds...");
                Thread.Sleep(5000);
            }
        }
    }

    public ClientProgram()
    {
        _client = new TcpClient("127.0.0.1", 8000);
        _stream = _client.GetStream();
    }

    public void Start()
    {
        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();
        
        Console.WriteLine("Client connected successfully to the server.");
        Console.WriteLine("============================================\n");
        
        Thread sendThread = new Thread(SendMessages);
        sendThread.Start();
    }

    private void SendMessages()
    {
        while (true)
        {
            Console.Write("> You: ");
            string? message = Console.ReadLine();
            
            if (message == null) break;
            if (message.StartsWith("/")) CheckForCommands(message);
            
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            _stream.Write(buffer, 0, buffer.Length);
        }
    }

    private void ReceiveMessages()
    {
        while (true)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
            {
                _client.Close();
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("\n> " + message);
            Console.Write("> You: ");
        }
    }
    
    private void CheckForCommands(string message)
    {
        switch (message)
        {
            case "/exit":
                _client.Close();
                Console.WriteLine("Disconnected from the server.");
                Environment.Exit(0);
                break;
            
            default:
                Console.WriteLine("Unknown command.");
                break;
        }
    }
}