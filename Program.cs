using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static TcpListener server;
    static TcpClient client;

    static int ballX = Console.WindowWidth / 2;
    static int ballY = Console.WindowHeight / 2;
    static int ballSpeedX = 1;
    static int ballSpeedY = 1;
    static int player1Y = Console.WindowHeight / 2 - 2;
    static int player2Y = Console.WindowHeight / 2 - 2;
    static int playerHeight = 5;
    static int playerSpeed = 1;
    static string IP;

    static bool czySerwer;

    static void DrawPaddles()
    {
        for (int i = 0; i < playerHeight; i++)
        {
            Console.SetCursorPosition(1, player1Y + i);
            Console.Write("|");
            Console.SetCursorPosition(Console.WindowWidth - 2, player2Y + i);
            Console.Write("|");
        }
    }

    static void DrawBall()
    {
        Console.SetCursorPosition(ballX, ballY);
        Console.Write("O");
    }

    static void Draw()
    {
        Console.Clear();
        DrawPaddles();
        DrawBall();
    }

    static void StartServer()
    {
        czySerwer = true;
        IPAddress ipAddress = IPAddress.Parse(GetLocalIPAddress()); 
        int port = 12345; 

        server = new TcpListener(ipAddress, port);
        server.Start();
        Console.WriteLine("Serwer uruchomiony...");
        Console.WriteLine("IP serwera: " + ipAddress);
        Console.WriteLine("Oczekiwanie na drugiego gracza...");
        client = server.AcceptTcpClient();

        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        Thread sendThread = new Thread(SendMessages);
        sendThread.Start();

        Thread graThread = new Thread(Gra);
        graThread.Start();

        receiveThread.Join();
        sendThread.Join();
        graThread.Join();
    }

    static void StartClient()
    {
        czySerwer = false;
        string serverIp = IP; 
        int serverPort = 12345; 

        client = new TcpClient();
        client.Connect(serverIp, serverPort);
        Console.WriteLine("Połączono z serwerem.");

        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        Thread sendThread = new Thread(SendMessages);
        sendThread.Start();

        Thread graThread = new Thread(Gra);
        graThread.Start();

        receiveThread.Join();
        sendThread.Join();
        graThread.Join();
    }

    static string GetLocalIPAddress()
    {
        string ipAddress = "";
        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in hostEntry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = ip.ToString();
                break;
            }
        }
      
        return ipAddress;
    }
    static void ReceiveMessages()
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        if (czySerwer)
        {
            while (true)
            {
               
               int bytesRead = stream.Read(buffer, 0, buffer.Length);
               string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
               int.TryParse(message, out int y);
               player2Y = y;
            }
        }
        else
        {
            while(true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                string[] tab = message.Split(" ");
                int.TryParse(tab[0], out int pal);
                int.TryParse(tab[1], out int x);
                int.TryParse(tab[2], out int y);
                ballX = x;
                ballY = y;
                player1Y = pal;
            }
        }
    }

    static void SendMessages()
    {
        NetworkStream stream = client.GetStream();

        if (czySerwer)
        {
            while (true)
            {
                Thread.Sleep(20);
                string message = player1Y.ToString()+" "+ballX.ToString()+" "+ballY.ToString();
               
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }
        else 
        {
            while (true)
            {
                Thread.Sleep(20);
                string message = player2Y.ToString();
               
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

       
    }
    static void Update()
    {
        if (czySerwer)
        {
            ballX += ballSpeedX;
            ballY += ballSpeedY;
        }

        if (ballY <= 0 || ballY >= Console.WindowHeight - 1)
        {
            ballSpeedY *= -1;
        }

        if (ballX <= 2 && ballY >= player1Y && ballY <= player1Y + playerHeight)
        {
            ballSpeedX *= -1;
        }

        if (ballX >= Console.WindowWidth - 3 && ballY >= player2Y && ballY <= player2Y + playerHeight)
        {
            ballSpeedX *= -1;
        }

        if (ballX <= 0 || ballX >= Console.WindowWidth - 1)
        {
            ballX = Console.WindowWidth / 2;
            ballY = Console.WindowHeight / 2;
        }
    }
    static void Gra()
    {
        Console.WindowHeight = 30;
        Console.WindowWidth = 60;
        Console.BufferHeight = Console.WindowHeight;
        Console.BufferWidth = Console.WindowWidth;

        while (true)
        {
            Thread.Sleep(120);
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey();

                if (czySerwer)
                {
                    if (key.Key == ConsoleKey.W && player1Y > 0)
                    {
                        player1Y -= playerSpeed;
                    }
                    if (key.Key == ConsoleKey.S && player1Y < Console.WindowHeight - playerHeight)
                    {
                        player1Y += playerSpeed;
                    }
                }
                else
                {
                    if (key.Key == ConsoleKey.UpArrow && player2Y > 0)
                    {
                        player2Y -= playerSpeed;
                    }
                    if (key.Key == ConsoleKey.DownArrow && player2Y < Console.WindowHeight - playerHeight)
                    {
                        player2Y += playerSpeed;
                    }
                }
            }
            Update();
            Draw();
        }
    }

    static void Main(string[] args)
    {
        Console.Write("Gracz hostujący gre porusza się W,S\nGracz dołączający do gry porusza się strzałkami góra,dół\n\n");

        Console.Write("Wybierz opcję:\n1. Uruchom jako serwer\n2. Uruchom jako klient\nWybór: ");
        int choice = int.Parse(Console.ReadLine());

        switch (choice)
        {
            case 1:
                StartServer();
                break;
            case 2:
                Console.Write("Podaj IP: \n");
                IP = Console.ReadLine();
                StartClient();
                break;
            default:
                Console.WriteLine("Niepoprawny wybór. Program zostanie zamknięty.");
                break;
        }
    }
}