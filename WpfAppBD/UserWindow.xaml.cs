using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

    public partial class UserWindow : Window
    {     
        SqlConnection sqlConnection;
        private int userId;
        public UserWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Connect1"].ConnectionString);
            LoadUserCars();
        }
        private void LoadUserCars()
        {
            try
            {
                sqlConnection.Open();
                string query = @"SELECT Cars.car_id, Cars.model
                                 FROM Cars
                                 INNER JOIN Sales ON Cars.car_id = Sales.car_id
                                 WHERE Sales.client_id = @client_id";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@client_id", userId);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                UserCarsComboBox.ItemsSource = dataTable.DefaultView;
                UserCarsComboBox.DisplayMemberPath = "model"; 
                UserCarsComboBox.SelectedValuePath = "car_id";  
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження авто: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
        }
        private void UserCarsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserCarsComboBox.SelectedValue == null) return;

            int carId = Convert.ToInt32(UserCarsComboBox.SelectedValue);

            try
            {
                sqlConnection.Open();
                string query = "SELECT [date service] AS [Дата], description AS [Опис], price AS [Ціна] FROM Service WHERE car_id = @car_id";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@car_id", carId);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                ServiceHistoryDataGrid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження історії обслуговування: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
        }


        private void LoadCarsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sqlConnection.Open();
                string query = @"SELECT car_id AS [ID], 
                                model AS [Модель], 
                                [body type] AS [Тип кузова], 
                                [year of graduation] AS [Рік], 
                                [color] AS [Колір], 
                                engine AS [Двигун],                              
                                price AS [Ціна]                           
                         FROM Cars
                         WHERE [availability for order] = 1";
                SqlDataAdapter adapter = new SqlDataAdapter(query, sqlConnection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                CarsDataGrid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час завантаження авто: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void OrderCarButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedCarIdTextBox.Text))
            {
                MessageBox.Show("Будь ласка, виберіть ID авто для замовлення!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PaymentTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, виберіть спосіб оплати!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string paymentType = (PaymentTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            try
            {
                sqlConnection.Open();

                string getMaxIdQuery = "SELECT ISNULL(MAX(sale_id), 0) + 1 FROM Sales";
                SqlCommand idCommand = new SqlCommand(getMaxIdQuery, sqlConnection);
                int newSaleId = (int)idCommand.ExecuteScalar();

                string priceQuery = "SELECT price FROM Cars WHERE car_id = @CarId";
                SqlCommand priceCommand = new SqlCommand(priceQuery, sqlConnection);
                priceCommand.Parameters.AddWithValue("@CarId", SelectedCarIdTextBox.Text);
                decimal price = (decimal)priceCommand.ExecuteScalar();

                string insertQuery = @"INSERT INTO Sales (sale_id, client_id, car_id, [date of sale], sum, [payment method])
                               VALUES (@SaleId, @ClientId, @CarId, GETDATE(), @Amount, @PaymentType)";
                SqlCommand insertCommand = new SqlCommand(insertQuery, sqlConnection);
                insertCommand.Parameters.AddWithValue("@SaleId", newSaleId);
                insertCommand.Parameters.AddWithValue("@ClientId", userId);
                insertCommand.Parameters.AddWithValue("@CarId", SelectedCarIdTextBox.Text); 
                insertCommand.Parameters.AddWithValue("@Amount", price); 
                insertCommand.Parameters.AddWithValue("@PaymentType", paymentType); 

                insertCommand.ExecuteNonQuery();

                string updateQuery = "UPDATE Cars SET [availability for order] = 0 WHERE car_id = @CarId";
                SqlCommand updateCommand = new SqlCommand(updateQuery, sqlConnection);
                updateCommand.Parameters.AddWithValue("@CarId", SelectedCarIdTextBox.Text);
                updateCommand.ExecuteNonQuery();

                MessageBox.Show("Авто успішно замовлено та стало недоступним для інших користувачів!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                sqlConnection.Close();
                LoadUserCars();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час замовлення авто: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }    
    }
}
