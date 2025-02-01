using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfAppBD
{
    public partial class AuthorizationApp : Window
    {
        private readonly SqlConnection sqlConnection;

        public AuthorizationApp()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Connect1"].ConnectionString);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string surname = UserSurnameTextBox.Text;
            string password = PasswordBox.Password;

            if (IsLoginDataValid(username, surname, password))
            {
                int userId = AuthenticateUser(username, surname, password);
                HandleLoginResult(userId, password);
            }
            else
            {
                ShowErrorMessage("Будь ласка, заповніть всі поля!");
            }
        }

        private bool IsLoginDataValid(string username, string surname, string password)
        {
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(surname) && !string.IsNullOrEmpty(password);
        }

        private int AuthenticateUser(string username, string surname, string password)
        {
            try
            {
                sqlConnection.Open();
                string query = "SELECT client_id FROM Clients WHERE name = @name AND surname = @surname AND Password = @Password";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@name", username);
                command.Parameters.AddWithValue("@surname", surname);
                command.Parameters.AddWithValue("@Password", password);

                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Помилка підключення до бази даних: {ex.Message}");
                return -1;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void HandleLoginResult(int userId, string password)
        {
            if (userId > 0)
            {
                if (password == "admin")
                {
                    OpenAdminWindow();
                }
                else
                {
                    OpenUserWindow(userId);
                }

                this.Close();
            }
            else
            {
                ShowErrorMessage("Невірний логін або пароль!");
            }
        }

        private void OpenAdminWindow()
        {
            MainWindow adminWindow = new MainWindow();
            adminWindow.Show();
        }

        private void OpenUserWindow(int userId)
        {
            UserWindow userWindow = new UserWindow(userId);
            userWindow.Show();
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void NavigateToRegistration(object sender, RoutedEventArgs e)
        {
            RegistrationApp registrationWindow = new RegistrationApp();
            registrationWindow.Show();
            this.Close();
        }
    }
}
