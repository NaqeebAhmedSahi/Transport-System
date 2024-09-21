using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Server
{
    public partial class MainWindow : Window
    {
        private volatile int clientCount = 0; // Use volatile to ensure visibility across threads
        private TcpListener listener;
        private readonly object countLock = new object(); // Lock object for synchronization

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ipAddress = IPAddress.Loopback; // Use 127.0.0.1 for server IP
            int port = 12345;

            listener = new TcpListener(ipAddress, port);
            try
            {
                listener.Start();
                UpdateStatusDisplay($"Server started at {ipAddress}:{port}");
                Console.WriteLine($"Server started at {ipAddress}:{port}");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    lock (countLock) // Lock when updating the client count
                    {
                        clientCount++;
                    }
                    //UpdateClientCountDisplay();
                    UpdateClientConnectedTextBlock("Client connected"); // Update client connected message
                    Console.WriteLine("Client connected");

                    _ = Task.Run(() => HandleClient(client));
                }
            }
            catch (SocketException ex)
            {
                UpdateStatusDisplay($"Socket error: {ex.Message}");
                Console.WriteLine($"Socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                UpdateStatusDisplay($"Error: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            //UpdateClientCountDisplay();
        }

        private void UpdateStatusDisplay(string message)
        {
            Dispatcher.Invoke(() => {
                StatusTextBlock.Text = message;
            });
        }



        private void UpdateClientConnectedTextBlock(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ClientConnectedTextBlock.Text = message;
            });
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                IPAddress actualIpAddress = GetLocalIPAddress();
                byte[] ipBytes = Encoding.ASCII.GetBytes(actualIpAddress.ToString());
                await stream.WriteAsync(ipBytes, 0, ipBytes.Length);
                Console.WriteLine("Actual IP address sent to client");

                // Send a response message to the client
                string responseMessage = "Welcome to the server!";
                byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                Console.WriteLine("Response message sent to client");

                client.Close();
                lock (countLock) // Lock when updating the client count
                {
                    clientCount--;
                }
               // UpdateClientCountDisplay();
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => {
                    StatusTextBlock.Text = $"Client handling error: {ex.Message}";
                });
                Console.WriteLine($"Client handling error: {ex.Message}");
            }
        }

        private IPAddress GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
