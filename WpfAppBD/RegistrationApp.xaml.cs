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
    public partial class RegistrationApp : Window
    {
        SqlConnection sqlConnection;
        public RegistrationApp()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Connect1"].ConnectionString);
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string firstName = txtFirstName.Text;
            string lastName = txtLastName.Text;
            string phoneNumber = txtPhoneNumber.Text;
            string email = txtEmail.Text;
            string adress = txtAdress.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(adress))
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.");
                return;
            }

            using (sqlConnection)
            {
                try
                {
                    sqlConnection.Open();

                    string maxIdQuery = "SELECT MAX(client_id) FROM Clients";
                    SqlCommand maxIdCommand = new SqlCommand(maxIdQuery, sqlConnection);
                    object maxIdResult = maxIdCommand.ExecuteScalar();

                    int newId = (maxIdResult != DBNull.Value) ? Convert.ToInt32(maxIdResult) + 1 : 1;

                    string query = "INSERT INTO Clients (client_id, name, surname, phone, email, address, Password) " +
                                   "VALUES (@Id, @FirstName, @LastName, @PhoneNumber, @Email, @Address, @Password)";

                    using (SqlCommand command = new SqlCommand(query, sqlConnection))
                    {
                        command.Parameters.AddWithValue("@Id", newId);  
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Address", adress);
                        command.Parameters.AddWithValue("@Password", password);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Реєстрація успішна!");
                            AuthorizationApp authorizationApp = new AuthorizationApp();
                            authorizationApp.Show();  
                            this.Close();  
                        }
                        else
                        {
                            MessageBox.Show("Помилка при реєстрації.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка: " + ex.Message);
                }
            }
        }

        private void NavigateToAuthorization(object sender, RoutedEventArgs e)
        {
            AuthorizationApp authorizationApp = new AuthorizationApp();
            authorizationApp.Show();
            this.Close();
        }
    }
}
