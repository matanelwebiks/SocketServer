using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static Socket serverSocket;
    static async Task Main(string[] args)
    {
        serverSocket = initServer();
        while (true) {
            //קבלת חיבור מלקוח
            Socket clientSocket = await Task.Factory.FromAsync(serverSocket.BeginAccept, serverSocket.EndAccept, null);
            Console.WriteLine("Client connected.");
            
            //מקף תחתון לסימון שהפונקציה רצה באופן א-סינכרוני
            _ = ReciveMessageAsync(clientSocket);
            _ = Task.Run(() =>
            {
                while (true)
                {
                    string? message = Console.ReadLine();
                    _ = SendMessageAsync(clientSocket, message);
                }
            });

        }
    }

    private static Socket initServer()
    {
        // יצירת אובייקט סוקט לשרת
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // הגדרת כתובת IP ופורט שהשרת יאזין להם
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);

        // קשירת הסוקט לכתובת
        serverSocket.Bind(endPoint);

        // הגדרת הסוקט להאזנה (עד 10 לקוחות בתור)
        serverSocket.Listen(10);

        Console.WriteLine("Waiting for a connection...");

        return serverSocket;
    }

    static async Task ReciveMessageAsync(Socket clientSocket)
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            int bytesRead = await Task.Factory.FromAsync<int>(
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, clientSocket),
                clientSocket.EndReceive);

            if (bytesRead == 0) break; // הלקוח סגר את החיבור

            string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: " + receivedMessage);
        }

        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }

    private static async Task SendMessageAsync(Socket clientSocket, string? message)
    {
        // שליחת תשובה ללקוח

        byte[] responseBytes = Encoding.ASCII.GetBytes(message);
        await Task.Factory.FromAsync(
            clientSocket.BeginSend(responseBytes, 0, responseBytes.Length, SocketFlags.None, null, clientSocket),
            clientSocket.EndSend);

    }
}
