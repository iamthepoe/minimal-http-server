using System.Net.Sockets;
using System.Net;
using System.Text;

namespace SimpleHttpServer
{
    public class HttpServer
    {
        private TcpListener Controller { get; set; }
        private int Port { get; set; }
        private int RequestsLength { get; set; }

        public HttpServer(int port = 8080)
        {
            this.Port = port;
            try
            {
                this.Controller = new TcpListener(IPAddress.Parse("127.0.0.1"), this.Port);
                this.Controller.Start();
                Console.WriteLine($"Server running at http://localhost:{this.Port}");
                Task httpServerTask = Task.Run(() => AwaitRequests());
                httpServerTask.GetAwaiter().GetResult();
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
                Task task = Task.Run(() => ProcessRequest(connection, this.RequestsLength));
            }
        }

        private void ProcessRequest(Socket connection, int requestNumber)
        {
            Console.WriteLine($"Processing request #{requestNumber}");
            if (connection.Connected)
            {
                byte[] requestBytes = new Byte[1024];
                connection.Receive(requestBytes, requestBytes.Length, 0);
                string requestText = Encoding.UTF8.GetString(requestBytes)
                  .Replace((char)0, ' ').Trim();

                if (requestText.Length > 0)
                {
                    Console.WriteLine($"\n{requestText}\n");
                    connection.Close();
                }
            }

            Console.WriteLine($"\nRequest #{requestNumber} finalized.");
        }
    }
}
