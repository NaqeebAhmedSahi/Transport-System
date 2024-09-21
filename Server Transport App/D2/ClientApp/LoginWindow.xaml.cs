using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace ClientApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            string serverIp = "127.0.0.1";
            int port = 12345;

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(serverIp, port);
                    NetworkStream stream = client.GetStream();

                    var request = new ClientRequest
                    {
                        Type = RequestType.SignIn,
                        UserData = new User { Username = username, Password = password }
                    };

                    string requestJson = JsonConvert.SerializeObject(request);
                    byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var serverResponse = JsonConvert.DeserializeObject<ServerResponse>(jsonData);

                    if (serverResponse.Success)
                    {
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password.");
                    }
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show($"Socket error: {se.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Client error: {ex.Message}");
            }
        }
    }

        public class ServerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
