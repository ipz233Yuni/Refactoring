using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class CarsHelp
    {
        private readonly SqlConnection sqlConnection;

        public CarRepository(SqlConnection sqlConnection)
        {
            this.sqlConnection = sqlConnection;
        }

        public void AddCar(List<string> inputData)
        {
            if (inputData.Count == 9)
            {
                sqlConnection.Open();

                SqlCommand getMaxIdCommand = new SqlCommand("SELECT ISNULL(MAX(car_id), 0) + 1 FROM Cars", sqlConnection);
                int newCarId = (int)getMaxIdCommand.ExecuteScalar();

                SqlCommand s = new SqlCommand("insert_car", sqlConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                s.Parameters.Add("@model", SqlDbType.NVarChar, 50).Value = inputData[0] ?? "";
                s.Parameters.Add("@year_of_graduation", SqlDbType.Int).Value = string.IsNullOrEmpty(inputData[1]) ? (object)DBNull.Value : int.Parse(inputData[1]);
                s.Parameters.Add("@body_type", SqlDbType.NVarChar, 50).Value = inputData[2] ?? "";
                s.Parameters.Add("@color", SqlDbType.NVarChar, 30).Value = inputData[3] ?? "";
                s.Parameters.Add("@engine", SqlDbType.NVarChar, 50).Value = inputData[4] ?? "";
                s.Parameters.Add("@price", SqlDbType.Decimal).Value = string.IsNullOrEmpty(inputData[5]) ? (object)DBNull.Value : decimal.Parse(inputData[5]);
                s.Parameters.Add("@availability_for_order", SqlDbType.Bit).Value = ParseToBool(inputData[6]);
                s.Parameters.Add("@status", SqlDbType.Bit).Value = ParseToBool(inputData[7]);
                s.Parameters.Add("@producer_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputData[8]) ? (object)DBNull.Value : int.Parse(inputData[8]);
                s.Parameters.Add("@car_id", SqlDbType.Int).Value = newCarId;

                s.ExecuteNonQuery();
                sqlConnection.Close();
            }
        }

        public void UpdateCar(int carId, List<string> inputData)
        {
            sqlConnection.Open();

            SqlCommand s = new SqlCommand("update_car", sqlConnection)
            {
                CommandType = CommandType.StoredProcedure
            };

            s.Parameters.Add("@car_id", SqlDbType.Int).Value = carId;
            s.Parameters.Add("@model", SqlDbType.NVarChar, 50).Value = inputData[0] ?? "";
            s.Parameters.Add("@year_of_graduation", SqlDbType.Int).Value = string.IsNullOrEmpty(inputData[1]) ? (object)DBNull.Value : int.Parse(inputData[1]);
            s.Parameters.Add("@body_type", SqlDbType.NVarChar, 50).Value = inputData[2] ?? "";
            s.Parameters.Add("@color", SqlDbType.NVarChar, 30).Value = inputData[3] ?? "";
            s.Parameters.Add("@engine", SqlDbType.NVarChar, 50).Value = inputData[4] ?? "";
            s.Parameters.Add("@price", SqlDbType.Decimal).Value = string.IsNullOrEmpty(inputData[5]) ? (object)DBNull.Value : decimal.Parse(inputData[5]);
            s.Parameters.Add("@availability_for_order", SqlDbType.Bit).Value = ParseToBool(inputData[6]);
            s.Parameters.Add("@status", SqlDbType.Bit).Value = ParseToBool(inputData[7]);
            s.Parameters.Add("@producer_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputData[8]) ? (object)DBNull.Value : int.Parse(inputData[8]);

            s.ExecuteNonQuery();
            sqlConnection.Close();
        }

        public void UpdateCarsTable(TabControl tabs)
        {
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Cars", sqlConnection);
            sqlConnection.Open();

            foreach (TabItem item in tabs.Items)
            {
                if (string.Equals(item.Header.ToString(), "Cars", StringComparison.OrdinalIgnoreCase))
                {
                    tabs.Items.Remove(item);
                    break;
                }
            }

            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            DataTable dataTable = new DataTable();
            sqlDataAdapter.Fill(dataTable);

            DataGrid dataGrid = new DataGrid
            {
                SelectionMode = DataGridSelectionMode.Single,
                AutoGenerateColumns = true,
                ItemsSource = dataTable.DefaultView
            };

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem
            {
                Header = "Закрити"
            };
            menuItem.Click += (sender, e) => tabs.Items.Remove(item);
            contextMenu.Items.Add(menuItem);

            tabs.Items.Add(new TabItem
            {
                Header = "Cars",
                Content = dataGrid,
                ContextMenu = contextMenu,
                IsSelected = true
            });
            sqlConnection.Close();
        }

        public void DeleteCar(int carId)
        {
            try
            {
                sqlConnection.Open();

                SqlCommand deleteSales = new SqlCommand("DELETE FROM Sales WHERE car_id = @car_id", sqlConnection);
                deleteSales.Parameters.Add("@car_id", SqlDbType.Int).Value = carId;
                deleteSales.ExecuteNonQuery();

                SqlCommand deleteService = new SqlCommand("DELETE FROM Service WHERE car_id = @car_id", sqlConnection);
                deleteService.Parameters.Add("@car_id", SqlDbType.Int).Value = carId;
                deleteService.ExecuteNonQuery();

                SqlCommand deleteCar = new SqlCommand("DELETE FROM Cars WHERE car_id = @car_id", sqlConnection);
                deleteCar.Parameters.Add("@car_id", SqlDbType.Int).Value = carId;
                deleteCar.ExecuteNonQuery();

                MessageBox.Show("Автомобіль успішно видалено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при видаленні автомобіля: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private bool ParseToBool(string value)
        {
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
