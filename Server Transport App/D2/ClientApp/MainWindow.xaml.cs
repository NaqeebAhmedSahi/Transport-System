using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using System.Collections.Generic; // Added to use List<T>

namespace ClientApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public event EventHandler<int> DeleteButtonClicked;


        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected user ID from the DataGrid
            if (UserDataGrid.SelectedItem is User selectedUser)
            {
                // Send a delete request to the server with the selected user ID
                await SendDeleteRequest(selectedUser.Id);
            }
            else
            {
                MessageBox.Show("Please select a user to delete.");
            }
        }

        private async Task SendDeleteRequest(int userId)
        {
            string serverIp = "127.0.0.1";
            int port = 12345;

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(serverIp, port);
                    NetworkStream stream = client.GetStream();

                    // Create a delete request object
                    var request = new ClientRequest
                    {
                        Type = RequestType.Delete,
                        UserData = new User { Id = userId } // Set the ID of the user to delete
                    };

                    // Serialize the delete request object to JSON
                    string requestJson = JsonConvert.SerializeObject(request);
                    byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

                    // Send the delete request to the server
                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                    // Display a message based on the server's response
                    MessageBox.Show("User deleted successfully.");
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show($"Socket error: {se.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string serverIp = "127.0.0.1"; // Server's loopback IP address
            int port = 12345; // Ensure this matches the port the server is listening on

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(serverIp, port);
                    NetworkStream stream = client.GetStream();

                    // Send a display request to the server
                    var request = new ClientRequest
                    {
                        Type = RequestType.Display
                    };
                    string requestJson = JsonConvert.SerializeObject(request);
                    byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                    // Read the response from the server
                    byte[] buffer = new byte[4096]; // Adjust buffer size as needed
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Deserialize JSON data into an object
                    var serverData = JsonConvert.DeserializeObject<ServerData>(jsonData);

                    // Access the server data
                    IpAddressTextBox.Text = serverData.IPAddress;
                    MessageBox.Show(serverData.Message);

                    // Populate the DataGrid with user data
                    UserDataGrid.ItemsSource = serverData.UserData;
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

        private void AddAdminButton_Click(object sender, RoutedEventArgs e)
        {
            Add add = new Add();
            add.Show();
            this.Hide();
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected user from the DataGrid
            if (UserDataGrid.SelectedItem is User selectedUser)
            {
                // Show the update dialog pre-filled with selected user data
                var updateDialog = new Update(selectedUser);
                if (updateDialog.ShowDialog() == true)
                {
                    var updatedUser = updateDialog.UpdatedUser;

                    // Create an update request object
                    var request = new ClientRequest
                    {
                        Type = RequestType.Update,
                        UserData = updatedUser
                    };

                    // Serialize the request to JSON
                    string requestJson = JsonConvert.SerializeObject(request);
                    byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

                    // Send the update request to the server
                    string serverIp = "127.0.0.1";
                    int port = 12345;

                    try
                    {
                        using (TcpClient client = new TcpClient())
                        {
                            await client.ConnectAsync(serverIp, port);
                            NetworkStream stream = client.GetStream();

                            await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                            // Read the response from the server
                            byte[] responseBuffer = new byte[4096];
                            int responseBytes = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                            string response = Encoding.UTF8.GetString(responseBuffer, 0, responseBytes);

                            // Display the response
                            MessageBox.Show(response);
                        }
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show($"Socket error: {se.Message}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to update.");
            }
        }

    }

    // Enum to represent different types of client requests
    public enum RequestType
    {
        Display,
        Add,
        Delete,
        Update,
        SignIn
    }

    // Class to represent a request sent by the client
    public class ClientRequest
    {
        public RequestType Type { get; set; }
        public User UserData { get; set; } // Property to hold user data

        public ClientRequest()
        {
            UserData = new User(); // Initialize UserData to avoid null reference exceptions
        }
    }

    // Class to represent the structure of JSON data received from the server
    public class ServerData
    {
        public string IPAddress { get; set; }
        public string Message { get; set; }
        public List<User> UserData { get; set; } // Property to hold user data
    }

    // Class to represent user data received from the server
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
    }
}
