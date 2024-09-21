using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Newtonsoft.Json;

namespace ClientApp
{

    public partial class Add : Window
    {
         private TcpClient client;
        public Add()
        {
            InitializeComponent();
            client = new TcpClient();
        }
        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string serverIp = "127.0.0.1"; // Server's loopback IP address
            int port = 12345; // Ensure this matches the port the server is listening on

            try
            {
                await client.ConnectAsync(serverIp, port);
                NetworkStream stream = client.GetStream();

                // Create a User object with data from text boxes
                User newUser = new User
                {
                    Username = UsernameTextBox.Text,
                    Password = PasswordTextBox.Text,
                    Role = int.Parse(RoleTextBox.Text)
                };

                // Create a request object with the user data and request type
                var request = new ClientRequest
                {
                    Type = RequestType.Add,
                    UserData = newUser
                };

                // Serialize request object to JSON
                string jsonData = JsonConvert.SerializeObject(request);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);

                // Send JSON data to server
                await stream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                Console.WriteLine("User data sent to server");

                // Receive response from the server
                byte[] responseBuffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                Console.WriteLine("Response from server: " + response);

                // Close the connection after receiving the response
                client.Close();
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
               

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
}
