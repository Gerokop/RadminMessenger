using System.Net;
using System.Net.Sockets;
using System.Text;
namespace RadminMessenger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CONNECT/HOST?");
            string answer = Console.ReadLine();
            IPEndPoint iPEndPoint = new(IPAddress.Any, 0);
            using TcpClient tcpClient = new();
            TcpListener tcpListener = new(iPEndPoint);
            
            try
            {
                if (answer.ToLower() == "host")
                {
                    tcpListener.Start();
                    Console.WriteLine($"Waiting for connection on {tcpListener.LocalEndpoint}");
                    TcpClient friend = tcpListener.AcceptTcpClient();
                    NetworkStream stream = friend.GetStream();
                    Console.WriteLine($"You are connected with {friend.Client.RemoteEndPoint}");

                    Task.Run(() => { GetMessagesInLoopAsync(stream); });
                    SendMessagesInLoop(stream);
                }
                else if (answer.ToLower() == "connect")
                {
                    Console.WriteLine("Enter ip address");
                    string ip = Console.ReadLine();
                    ip = ip == "0" ? "127.0.0.1" : ip;
                    Console.WriteLine("Enter port: ");
                    int port = int.Parse(Console.ReadLine());
                    tcpClient.Connect(ip,port);
                    NetworkStream stream = tcpClient.GetStream();
                    Console.WriteLine("Connected!");

                    Task.Run(() => { GetMessagesInLoopAsync(stream); });
                    SendMessagesInLoop(stream);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        private static void SendMessagesInLoop(NetworkStream stream)
        {
            while (true)
            {
                string messageToSend = "";
                ConsoleKeyInfo keyInfo = new();
                while (keyInfo.Key != ConsoleKey.Enter)
                {
                    keyInfo = Console.ReadKey();
                    if (keyInfo.Key == ConsoleKey.Enter)
                        break;
                    else if (keyInfo.Key == ConsoleKey.Backspace)
                    {
                        Console.Write(" \b");
                        messageToSend = messageToSend.Remove(messageToSend.Length-1);
                        continue;
                    }
                    messageToSend += keyInfo.KeyChar;
                }
                ClearCurrentConsoleLine();
                Console.WriteLine($"[You] {messageToSend}");
                byte[] buffer = Encoding.UTF8.GetBytes(messageToSend);
                stream.Write(buffer);
            }
        }

        private static void GetMessagesInLoopAsync(NetworkStream stream)
        {
            while (true)
            {
                byte[] buffer = new byte[300];
                int bytesRead = stream.Read(buffer);
                Array.Resize(ref buffer,bytesRead);
                ClearCurrentConsoleLine();
                Console.WriteLine($"[{stream.Socket.RemoteEndPoint}] "+Encoding.UTF8.GetString(buffer));
            }
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
