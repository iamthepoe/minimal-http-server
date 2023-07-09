using System.Net.Sockets;
using System.Net;

class HttpServer
{
    private TcpListener Controller { get; set; }
    private int Port { get; set; }
    private int RequestsLength { get; set; }

    HttpServer(int port = 8080)
    {
        try
        {
            this.Controller = new TcpListener(IPAddress.Parse("127.0.0.1"), this.Port);
            this.Controller.Start();
            Console.WriteLine($"Server running at http://localhost:{this.Port}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error on start server at port {this.Port}\n{e.Message}");
        }
    }

    private async Task AwaitRequests()
    {
        while (true)
        {
            Socket connection = await this.Controller.AcceptSocketAsync();
            this.RequestsLength++;
        }
    }
}
