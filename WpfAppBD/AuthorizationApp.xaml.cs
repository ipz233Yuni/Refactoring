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
        SqlConnection sqlConnection;
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

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Будь ласка, заповніть всі поля!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int userId = AuthenticateUser(username, surname, password);
            if (userId > 0)
            {
                if (password == "admin")
                {
                    MainWindow adminWindow = new MainWindow();
                    adminWindow.Show();
                }
                else
                {
                    UserWindow userWindow = new UserWindow(userId);
                    userWindow.Show();
                }

                this.Close();
            }
            else
            {
                MessageBox.Show("Невірний логін або пароль!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private int AuthenticateUser(string username, string surname,string password)
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
                MessageBox.Show($"Помилка підключення до бази даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void NavigateToRegistration(object sender, RoutedEventArgs e)
        {
            RegistrationApp registrationWindow = new RegistrationApp();
            registrationWindow.Show(); 
            this.Close(); 
        }
    }
}
