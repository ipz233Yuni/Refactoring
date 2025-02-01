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

        public CarsHelp(SqlConnection sqlConnection)
        {
            this.sqlConnection = sqlConnection;
        }

        public void AddCar(List<string> inputData)
        {
            if (inputData.Count == 9)
            {
                int newCarId = GetNewCarId();
                ExecuteCarCommand("insert_car", inputData, newCarId);
            }
        }

        public void UpdateCar(int carId, List<string> inputData)
        {
            ExecuteCarCommand("update_car", inputData, carId);
        }

        private int GetNewCarId()
        {
            sqlConnection.Open();
            SqlCommand getMaxIdCommand = new SqlCommand("SELECT ISNULL(MAX(car_id), 0) + 1 FROM Cars", sqlConnection);
            int newCarId = (int)getMaxIdCommand.ExecuteScalar();
            sqlConnection.Close();
            return newCarId;
        }

        private void ExecuteCarCommand(string commandText, List<string> inputData, int carId)
        {
            try
            {
                sqlConnection.Open();
                SqlCommand command = new SqlCommand(commandText, sqlConnection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                AddCarParameters(command, inputData, carId);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при виконанні команди: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void AddCarParameters(SqlCommand command, List<string> inputData, int carId)
        {
            command.Parameters.Add("@car_id", SqlDbType.Int).Value = carId;
            command.Parameters.Add("@model", SqlDbType.NVarChar, 50).Value = inputData[0] ?? "";
            command.Parameters.Add("@year_of_graduation", SqlDbType.Int).Value = string.IsNullOrEmpty(inputData[1]) ? (object)DBNull.Value : int.Parse(inputData[1]);
            command.Parameters.Add("@body_type", SqlDbType.NVarChar, 50).Value = inputData[2] ?? "";
            command.Parameters.Add("@color", SqlDbType.NVarChar, 30).Value = inputData[3] ?? "";
            command.Parameters.Add("@engine", SqlDbType.NVarChar, 50).Value = inputData[4] ?? "";
            command.Parameters.Add("@price", SqlDbType.Decimal).Value = string.IsNullOrEmpty(inputData[5]) ? (object)DBNull.Value : decimal.Parse(inputData[5]);
            command.Parameters.Add("@availability_for_order", SqlDbType.Bit).Value = ParseToBool(inputData[6]);
            command.Parameters.Add("@status", SqlDbType.Bit).Value = ParseToBool(inputData[7]);
            command.Parameters.Add("@producer_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputData[8]) ? (object)DBNull.Value : int.Parse(inputData[8]);
        }

        private bool ParseToBool(string value)
        {
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
