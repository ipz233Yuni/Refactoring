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
        private readonly Action<string> errorHandler;

        public CarsHelp(SqlConnection sqlConnection, Action<string> errorHandler = null)
        {
            this.sqlConnection = sqlConnection ?? throw new ArgumentNullException(nameof(sqlConnection));
            this.errorHandler = errorHandler ?? DefaultErrorHandler;
        }

        public void AddCar(List<string> inputData)
        {
            if (inputData?.Count == 9)
            {
                int newCarId = GetNewCarId();
                ExecuteCarCommand("insert_car", inputData, newCarId);
            }
        }

        public void UpdateCar(int carId, List<string> inputData)
        {
            if (inputData?.Count == 9)
            {
                ExecuteCarCommand("update_car", inputData, carId);
            }
        }

        private int GetNewCarId()
        {
            return ExecuteWithConnection(connection =>
            {
                using (var command = new SqlCommand("SELECT ISNULL(MAX(car_id), 0) + 1 FROM Cars", connection))
                {
                    return (int)command.ExecuteScalar();
                }
            });
        }

        private void ExecuteCarCommand(string commandText, List<string> inputData, int carId)
        {
            try
            {
                ExecuteWithConnection(connection =>
                {
                    using (var command = CreateCarCommand(connection, commandText, inputData, carId))
                    {
                        command.ExecuteNonQuery();
                    }
                    return 0; 
                });
            }
            catch (Exception ex)
            {
                errorHandler($"Помилка при виконанні команди: {ex.Message}");
            }
        }

        private SqlCommand CreateCarCommand(SqlConnection connection, string commandText, List<string> inputData, int carId)
        {
            var command = new SqlCommand(commandText, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            AddCarParameters(command, inputData, carId);
            return command;
        }

        private T ExecuteWithConnection<T>(Func<SqlConnection, T> operation)
        {
            sqlConnection.Open();
            try
            {
                return operation(sqlConnection);
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
            command.Parameters.Add("@year_of_graduation", SqlDbType.Int).Value = ParseToInt(inputData[1]);
            command.Parameters.Add("@body_type", SqlDbType.NVarChar, 50).Value = inputData[2] ?? "";
            command.Parameters.Add("@color", SqlDbType.NVarChar, 30).Value = inputData[3] ?? "";
            command.Parameters.Add("@engine", SqlDbType.NVarChar, 50).Value = inputData[4] ?? "";
            command.Parameters.Add("@price", SqlDbType.Decimal).Value = ParseToDecimal(inputData[5]);
            command.Parameters.Add("@availability_for_order", SqlDbType.Bit).Value = ParseToBool(inputData[6]);
            command.Parameters.Add("@status", SqlDbType.Bit).Value = ParseToBool(inputData[7]);
            command.Parameters.Add("@producer_id", SqlDbType.Int).Value = ParseToInt(inputData[8]);
        }

        private object ParseToInt(string value)
        {
            return string.IsNullOrEmpty(value) ? (object)DBNull.Value : int.Parse(value);
        }

        private object ParseToDecimal(string value)
        {
            return string.IsNullOrEmpty(value) ? (object)DBNull.Value : decimal.Parse(value);
        }

        private bool ParseToBool(string value)
        {
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private void DefaultErrorHandler(string message)
        {
            System.Diagnostics.Debug.WriteLine($"CarsHelp Error: {message}");
        }
    }
}