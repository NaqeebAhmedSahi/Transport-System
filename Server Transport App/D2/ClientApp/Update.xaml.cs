using System.Windows;

namespace ClientApp
{
    public partial class Update : Window
    {
        public User UpdatedUser { get; private set; }

        public Update(User user)
        {
            InitializeComponent();

            // Initialize UpdatedUser with the provided user's data
            UpdatedUser = new User
            {
                Id = user.Id,
                Username = user.Username,
                Password = user.Password,
                Role = user.Role
            };

            // Pre-fill text boxes with current user data
            UsernameTextBox.Text = user.Username;
            PasswordTextBox.Text = user.Password;
            RoleTextBox.Text = user.Role.ToString();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the UpdatedUser property with the new values from text boxes
            UpdatedUser.Username = UsernameTextBox.Text;
            UpdatedUser.Password = PasswordTextBox.Text;
            UpdatedUser.Role = int.Parse(RoleTextBox.Text);

            DialogResult = true;
            Close();
        }
    }
}
