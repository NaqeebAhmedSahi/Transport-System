using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace ServerApp1
{
    public class ClientRequest
    {
        public RequestType Type { get; set; }
        public User UserData { get; set; } // Assuming you also want to include user data in the request
    }
    public enum RequestType
    {
        Display,
        Add,
        Delete,
        Update,
        SignIn
    }
    public class ServerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }


    // Class to represent the structure of JSON data sent from the server to the client
    public class ServerData
    {
        public string IPAddress { get; set; }
        public string Message { get; set; }
        public List<User> UserData { get; set; } // Property to hold user data

        public ServerData()
        {
            UserData = new List<User>();
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
    }

    public partial class MainWindow : Window
    {
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

            try
            {
                listener = new TcpListener(ipAddress, port);
                listener.Start();
                UpdateStatusDisplay($"Server started at {ipAddress}:{port}");
                Console.WriteLine($"Server started at {ipAddress}:{port}");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (SocketException ex)
            {
                HandleError($"Socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                HandleError($"Error: {ex.Message}");
            }
        }

        private void HandleError(string message)
        {
            UpdateStatusDisplay(message);
            Console.WriteLine(message);
        }

        private void UpdateStatusDisplay(string message)
        {
            Dispatcher.Invoke(() => {
                StatusTextBlock.Text = message;
            });
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    // Receive the request from the client
                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var request = JsonConvert.DeserializeObject<ClientRequest>(requestJson);

                    // Check the request type and handle accordingly
                    if (request.Type == RequestType.Display)
                    {
                        await HandleDisplayRequest(stream);
                    }
                    else if (request.Type == RequestType.Add)
                    {
                        await HandleAddRequest(stream, request.UserData);
                    }
                    else if (request.Type == RequestType.Delete)
                    {
                        await HandleDeleteRequest(stream, request.UserData.Id);
                    }
                    else if (request.Type == RequestType.Update) // Add this block
                    {
                        await HandleUpdateRequest(stream, request.UserData);
                    }
                    else if (request.Type == RequestType.SignIn) // Add this block
                    {
                        await HandleSignInRequest(stream, request.UserData);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Client handling error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
        private async Task HandleSignInRequest(NetworkStream stream, User userData)
        {
            try
            {
                bool isAuthenticated = AuthenticateUser(userData.Username, userData.Password);

                var response = new ServerResponse
                {
                    Success = isAuthenticated,
                    Message = isAuthenticated ? "Sign in successful" : "Invalid username or password"
                };

                string responseJson = JsonConvert.SerializeObject(response);
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
            catch (Exception ex)
            {
                HandleError($"Error handling sign-in request: {ex.Message}");
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source=(localdb)\\ProjectModels;Initial Catalog=RoutedSystem;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"))
                {
                    string query = "SELECT COUNT(*) FROM [dbo].[User] WHERE username = @Username AND password = @Password";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        connection.Open();
                        int userCount = (int)command.ExecuteScalar();

                        return userCount > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error authenticating user: {ex.Message}");
                return false;
            }
        }
        private async Task HandleUpdateRequest(NetworkStream stream, User updatedUserData)
{
    try
    {
        // Call a method to update the user data in the database
        bool success = UpdateUserData(updatedUserData);

        // Send a response to the client indicating success or failure
        string response = success ? "User data updated successfully" : "Failed to update user data";
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        Console.WriteLine(response);
    }
    catch (Exception ex)
    {
        HandleError($"Error handling update request: {ex.Message}");
    }
}

// Method to update user data in the database
private bool UpdateUserData(User updatedUser)
{
    try
    {
        // Establish connection to the database
        using (SqlConnection connection = new SqlConnection("Data Source=(localdb)\\ProjectModels;Initial Catalog=RoutedSystem;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"))
        {
            // SQL query to update user data in the database
            string query = "UPDATE [dbo].[User] SET username = @Username, password = @Password, role = @Role WHERE Id = @UserId";

            // Create SQL command with parameters
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // Add parameters
                command.Parameters.AddWithValue("@UserId", updatedUser.Id);
                command.Parameters.AddWithValue("@Username", updatedUser.Username);
                command.Parameters.AddWithValue("@Password", updatedUser.Password);
                command.Parameters.AddWithValue("@Role", updatedUser.Role);

                // Open connection
                connection.Open();

                // Execute the SQL command
                int rowsAffected = command.ExecuteNonQuery();

                // Check if any rows were affected (i.e., user was updated)
                return rowsAffected > 0;
            }
        }
    }
    catch (Exception ex)
    {
        // Handle any exceptions that occur during database operation
        Console.WriteLine($"Error updating user data in database: {ex.Message}");
        return false;
    }
}


        private async Task HandleDeleteRequest(NetworkStream stream, int userId)
        {
            try
            {
                // Call a method to delete the user data from the database
                bool success = DeleteUserData(userId);

                // Send a response to the client indicating success or failure
                string response = success ? "User data deleted successfully" : "Failed to delete user data";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                HandleError($"Error handling delete request: {ex.Message}");
            }
        }

        // Method to delete user data from the database
        private bool DeleteUserData(int userId)
        {
            try
            {
                // Establish connection to the database
                using (SqlConnection connection = new SqlConnection("Data Source=(localdb)\\ProjectModels;Initial Catalog=RoutedSystem;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"))
                {
                    // SQL query to delete user data from the database
                    string query = "DELETE FROM [dbo].[User] WHERE Id = @UserId";

                    // Create SQL command with parameters
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameter
                        command.Parameters.AddWithValue("@UserId", userId);

                        // Open connection
                        connection.Open();

                        // Execute the SQL command
                        int rowsAffected = command.ExecuteNonQuery();

                        // Check if any rows were affected (i.e., user was deleted)
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during database operation
                Console.WriteLine($"Error deleting user data from database: {ex.Message}");
                return false;
            }
        }

        private async Task HandleDisplayRequest(NetworkStream stream)
        {
            try
            {
                // Fetch user data from the database
                List<User> userData = FetchUserDataFromDatabase();

                // Create an instance of ServerData with user data
                var serverData = new ServerData
                {
                    IPAddress = GetLocalIPAddress().ToString(),
                    Message = "Welcome to the server!",
                    UserData = userData
                };

                // Serialize ServerData to JSON
                string json = JsonConvert.SerializeObject(serverData);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                // Send JSON data to client
                await stream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                Console.WriteLine("JSON data sent to client");

                // Update the JsonTextBox with the JSON data
                Dispatcher.Invoke(() => {
                    JsonTextBox.Text = json;
                });
            }
            catch (Exception ex)
            {
                HandleError($"Error handling display request: {ex.Message}");
            }
        }

        private async Task HandleAddRequest(NetworkStream stream, User userData)
        {
            try
            {
                // Call a method to save the user data to the database
                bool success = SaveUserData(userData);

                // Send a response to the client indicating success or failure
                string response = success ? "User data added successfully" : "Failed to add user data";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                HandleError($"Error handling add request: {ex.Message}");
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

        // Method to fetch user data from the database
        private List<User> FetchUserDataFromDatabase()
        {
            List<User> userData = new List<User>();

            // Establish connection to the database
            using (SqlConnection connection = new SqlConnection("Data Source=(localdb)\\ProjectModels;Initial Catalog=RoutedSystem;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"))
            {
                // SQL query to fetch user data
                string query = "SELECT Id, username, password, role FROM [dbo].[User]";

                // Create SQL command
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        // Open connection
                        connection.Open();

                        // Execute SQL command and read results
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Iterate through the result set and populate the userData list
                            while (reader.Read())
                            {
                                User user = new User
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Username = reader["username"].ToString(),
                                    Password = reader["password"].ToString(),
                                    Role = Convert.ToInt32(reader["role"])
                                };
                                userData.Add(user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions that occur during database operation
                        Console.WriteLine($"Error fetching user data: {ex.Message}");
                    }
                }
            }

            return userData;
        }
        // Method to save user data into the database
        private bool SaveUserData(User newUser)
        {
            try
            {
                // Establish connection to the database
                using (SqlConnection connection = new SqlConnection("Data Source=(localdb)\\ProjectModels;Initial Catalog=RoutedSystem;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"))
                {
                    // SQL query to insert user data into the database
                    string query = "INSERT INTO [dbo].[User] (username, password, role) VALUES (@Username, @Password, @Role)";

                    // Create SQL command with parameters
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@Username", newUser.Username);
                        command.Parameters.AddWithValue("@Password", newUser.Password);
                        command.Parameters.AddWithValue("@Role", newUser.Role);

                        // Open connection
                        connection.Open();

                        // Execute the SQL command
                        int rowsAffected = command.ExecuteNonQuery();

                        // Check if the data was successfully inserted
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during database operation
                Console.WriteLine($"Error saving user data to database: {ex.Message}");
                return false;
            }
        }

  
    }
}
