using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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

                    string resource = GetRequestInfo(requestText, "resource");

                    var contentBytes = ReadFile(resource);

                    if (contentBytes.Length == 0)
                        contentBytes = ReadFile("/404.html");

                    var headerBytes = CreateHeader("HTTP/1.1", "text/html;charset=utf-8",
                        "200", contentBytes.Length);

                    int sendedBytes = connection.Send(headerBytes, headerBytes.Length, 0);

                    sendedBytes += connection.Send(contentBytes, contentBytes.Length, 0);

                    connection.Close();

                    Console.WriteLine($"\n{sendedBytes} bytes sended for requisition #{requestNumber}");
                }
            }

            Console.WriteLine($"\nRequest #{requestNumber} finalized.");
        }

        public byte[] CreateHeader(string httpVersion, string mimeType,
            string code, int bytesLength)
        {
            StringBuilder text = new StringBuilder();
            text.Append($"{httpVersion} {code}{Environment.NewLine}");
            text.Append($"Server: Simple Http Server 1.0{Environment.NewLine}");
            text.Append($"Content-Type: {mimeType}{Environment.NewLine}");
            text.Append($"Content-Length: {bytesLength}{Environment.NewLine}{Environment.NewLine}");
            return Encoding.UTF8.GetBytes(text.ToString());
        }

        public byte[] ReadFile(string source)
        {
            string path = Path.GetFullPath("./www" + source);

            if (File.Exists(path))
                return File.ReadAllBytes(path);
            else
                return new Byte[0];
        }

        public string GetRequestInfo(string requestText, string item)
        {
            Regex regex = new Regex(@"^([A-Z]+)\s+([^ ]+)\s+HTTP/(\d\.\d)", RegexOptions.IgnoreCase);
            Match match = regex.Match(requestText);

            if (!match.Success)
                return "";

            switch (item)
            {
                case "method":
                    return match.Groups[1].Value;

                case "resource":
                    if (match.Groups[2].Value == "/") return "/index.html";

                    return match.Groups[2].Value;

                default:
                    return match.Groups[3].Value;
            }
        }
    }
}
