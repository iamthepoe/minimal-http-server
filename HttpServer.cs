using System.Net.Sockets;

class HttpServer
{
    private TcpListener Controller { get; set; }
    private int Port { get; set; }
    private int RequestsLength { get; set; }
}
