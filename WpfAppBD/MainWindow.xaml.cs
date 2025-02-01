using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using System.Xml.Linq;

namespace WpfAppBD
{
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection;
        public MainWindow()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Connect1"].ConnectionString);
        }

        private void Del_Record(object sender, RoutedEventArgs e)
        {
            TabItem item = (TabItem)tabs.SelectedItem;
            DataGrid dataGrid = (DataGrid)item.Content;

            if (dataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;
                int carId = (int)selectedRow.Row.ItemArray[0]; 
                int clientId = (int)selectedRow.Row.ItemArray[0]; 
                int producerId = (int)selectedRow.Row.ItemArray[0]; 
                int saleId = (int)selectedRow.Row.ItemArray[0]; 
                int serviceId = (int)selectedRow.Row.ItemArray[0]; 

                switch (item.Header.ToString())
                {
                    case "Cars":
                        Delete_Car(carId); 
                        break;
                    case "Clients":
                        Delete_Client(clientId);
                        break;
                    case "Producer":
                        Delete_Producer(producerId);
                        break;
                    case "Sales":
                        Delete_Sale(saleId);
                        break;
                    case "Service":
                        Delete_Service(serviceId);
                        break;
                }
            }
        }

        private void Upd_Record(object sender, RoutedEventArgs e)
        {
            TabItem item = (TabItem)tabs.SelectedItem;
            DataGrid dataGrid = (DataGrid)item.Content;

            if (dataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;
                int recordId = (int)selectedRow.Row.ItemArray[0];
                var updateActions = new Dictionary<string, Action<int>>()
                {
                    { "Cars", Upd_car },
                    { "Clients", Upd_Client },
                    { "Producer", Upd_Producer },
                    { "Sales", Upd_sale },
                    { "Service", Upd_Service }
                };

                if (updateActions.ContainsKey(item.Header.ToString()))
                {
                    updateActions[item.Header.ToString()](recordId);
                }
            }
        }

        private void Upd_Table(object sender, RoutedEventArgs e)
        {
            TabItem item = (TabItem)tabs.SelectedItem;
            switch (item.Header.ToString())
            {
                case "Cars":
                    Upd_cars_Table(); 
                    break;
                case "Clients":
                    Upd_Clients_Table();
                    break;
                case "Producer":
                    Upd_Producers_Table();
                    break;
                case "Sales":
                    Upd_sales_Table();
                    break;
                case "Service":
                    Upd_Services_Table();
                    break;
            }
        }

        private void Add_Record(object sender, RoutedEventArgs e)
        {
            TabItem item = (TabItem)tabs.SelectedItem;
            switch (item.Header.ToString())
            {
                case "Cars": Add_car(); break;
                case "Clients": Add_Client(); break;
                case "Producer": Add_Producer(); break;
                case "Sales": Add_sale(); break;
                case "Service": Add_Service(); break;
            }
        }

        private void Nav_Last(object sender, RoutedEventArgs e)
        {
            TabItem item = (TabItem)tabs.SelectedItem;
            DataGrid dataGrid = (DataGrid)item.Content;
            dataGrid.SelectedIndex = dataGrid.Items.Count - 2;
        }

        private void Nav_Prev(object sender, RoutedEventArgs e)
        {
            TabItem item = (TabItem)tabs.SelectedItem;
            DataGrid dataGrid = (DataGrid)item.Content;
            if (dataGrid.SelectedIndex == 0)
            {
                dataGrid.SelectedIndex = dataGrid.Items.Count - 2;
            }
            else
            {
                dataGrid.SelectedIndex--;
            }
        }
        private void Nav_Next(object sender, RoutedEventArgs e)
        {
            TabItem item = (TabItem)tabs.SelectedItem;
            DataGrid dataGrid = (DataGrid)item.Content;
            if (dataGrid.SelectedIndex == dataGrid.Items.Count - 2)
            {
                dataGrid.SelectedIndex = 0;
            }
            else
            {
                dataGrid.SelectedIndex++;
            }
        }

        private void Nav_First(object sender, RoutedEventArgs e)
        {
            TabItem item = (TabItem)tabs.SelectedItem;
            DataGrid dataGrid = (DataGrid)item.Content;
            dataGrid.SelectedIndex = 0;
        }

        private void TreeViewItem_Expand(object sender, RoutedEventArgs e)
        {
            SqlCommand sqlCommand = new SqlCommand();
            TreeViewItem parentTreeViewItem = (TreeViewItem)sender;
            parentTreeViewItem.Items.Clear();
            if (parentTreeViewItem.Header.ToString() == "Таблиці")
            {
                sqlCommand.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where Not(TABLE_NAME like 'sysdiagrams')";
            sqlCommand.Connection = sqlConnection;
            }
            sqlConnection.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            if ((sqlDataReader.HasRows) & (parentTreeViewItem.Header.ToString() == "Таблиці"))
            {
                while (sqlDataReader.Read())
                {
                    TreeViewItem childTreeViewItem = new TreeViewItem();
                    childTreeViewItem.Header = sqlDataReader[0].ToString();
                    childTreeViewItem.Selected += Table_Open;
                    parentTreeViewItem.Items.Add(childTreeViewItem);
                }
            }
            sqlConnection.Close();
        }

        private void tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void CloseTab(object sender, RoutedEventArgs e)
        {
            
        }
        private void Table_Open(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)sender;
            SqlCommand sqlCommand = new SqlCommand("SELECT * from " + treeViewItem.Header.ToString(), sqlConnection);
            sqlConnection.Open();
            if (!tabs.Items.OfType<TabItem>().Any(item => item.Header.ToString() == treeViewItem.Header.ToString()))
            {
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
                DataGrid dataGrid = new DataGrid();
                dataGrid.SelectionMode = DataGridSelectionMode.Single;
                dataGrid.AutoGenerateColumns = true;
                dataGrid.ItemsSource = dataTable.DefaultView;
                ContextMenu contextMenu = new ContextMenu();
                MenuItem menuItem = new MenuItem();
                menuItem.Header = "Закрити";
                menuItem.Click += CloseTab;
                contextMenu.Items.Add(menuItem);
                tabs.Items.Add(
                new TabItem
                {
                    Header = treeViewItem.Header.ToString(),
                    Content = dataGrid,
                    ContextMenu = contextMenu
                }
                );
            }
            sqlConnection.Close();
            Status.Text = "Завантажено таблицю " + treeViewItem.Header.ToString();
        }
        private void Close_Tab(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TabItem tabItem = (TabItem)contextMenu.PlacementTarget;
            if (tabItem != null && tabs.Items.Contains(tabItem))
            {
                tabs.Items.Remove(tabItem);
            }
        }      
        private void Add_car()
        {
            Window1 datawindow = new Window1();
            List<string> data = new List<string>
        {
                "@model",
                "@year_of_graduation",
                "@body_type",
                "@color",
                "@engine",
                "@price",
                "@availability_for_order",
                "@status",
                "@producer_id"
            };
            datawindow.dataToShow = data;
            datawindow.Fill();
            datawindow.ShowDialog();
            List<string> inputdata = datawindow.dataCollect;

            if (inputdata.Count == 9) 
            {
                sqlConnection.Open();

                SqlCommand getMaxIdCommand = new SqlCommand("SELECT ISNULL(MAX(car_id), 0) + 1 FROM Cars", sqlConnection);
                int newCarId = (int)getMaxIdCommand.ExecuteScalar();

                SqlCommand s = new SqlCommand("insert_car", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@model", SqlDbType.NVarChar, 50).Value = inputdata[0] ?? "";
                s.Parameters.Add("@year_of_graduation", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[1]) ? (object)DBNull.Value : int.Parse(inputdata[1]);
                s.Parameters.Add("@body_type", SqlDbType.NVarChar, 50).Value = inputdata[2] ?? "";
                s.Parameters.Add("@color", SqlDbType.NVarChar, 30).Value = inputdata[3] ?? "";
                s.Parameters.Add("@engine", SqlDbType.NVarChar, 50).Value = inputdata[4] ?? "";
                s.Parameters.Add("@price", SqlDbType.Decimal).Value = string.IsNullOrEmpty(inputdata[5]) ? (object)DBNull.Value : decimal.Parse(inputdata[5]);
                s.Parameters.Add("@availability_for_order", SqlDbType.Bit).Value = string.IsNullOrEmpty(inputdata[6]) ? (object)DBNull.Value : ParseToBool(inputdata[6]);
                s.Parameters.Add("@status", SqlDbType.Bit).Value = string.IsNullOrEmpty(inputdata[7]) ? (object)DBNull.Value : ParseToBool(inputdata[7]);
                s.Parameters.Add("@producer_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[8]) ? (object)DBNull.Value : int.Parse(inputdata[8]);
                s.Parameters.Add("@car_id", SqlDbType.Int).Value = newCarId;

                s.ExecuteNonQuery();
                sqlConnection.Close();
                Upd_cars_Table();
            }
        }
        private bool ParseToBool(string value)
        {
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        private void Upd_car(int carId)
        {
            sqlConnection.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Cars WHERE car_id = @car_id", sqlConnection);
            cmd.Parameters.Add("@car_id", SqlDbType.Int).Value = carId;

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                List<string> initialData = new List<string>
                {
                    reader["model"].ToString(),
                    reader["year of graduation"].ToString(),
                    reader["body type"].ToString(),
                    reader["color"].ToString(),
                    reader["engine"].ToString(),
                    reader["price"].ToString(),
                    reader["availability for order"].ToString(),
                    reader["status"].ToString(),
                    reader["producer_id"].ToString()
                };
                reader.Close();
                Window1 datawindow = new Window1
                {
                    dataToShow = new List<string>
                {
                    "@model",
                    "@year_of_graduation",
                    "@body_type",
                    "@color",
                    "@engine",
                    "@price",
                    "@availability_for_order",
                    "@status",
                    "@producer_id"
                },
                    initialData = initialData 
                };
                datawindow.Fill();
                datawindow.ShowDialog();

                List<string> inputdata = datawindow.dataCollect;

                SqlCommand s = new SqlCommand("update_car", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@car_id", SqlDbType.Int).Value = carId;
                s.Parameters.Add("@model", SqlDbType.NVarChar, 50).Value = inputdata[0] ?? "";
                s.Parameters.Add("@year_of_graduation", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[1]) ? (object)DBNull.Value : int.Parse(inputdata[1]);
                s.Parameters.Add("@body_type", SqlDbType.NVarChar, 50).Value = inputdata[2] ?? "";
                s.Parameters.Add("@color", SqlDbType.NVarChar, 30).Value = inputdata[3] ?? "";
                s.Parameters.Add("@engine", SqlDbType.NVarChar, 50).Value = inputdata[4] ?? "";
                decimal priceValue;
                s.Parameters.Add("@price", SqlDbType.Decimal).Value = decimal.TryParse(inputdata[5], out priceValue) ? (object)priceValue : DBNull.Value;
                s.Parameters.Add("@availability_for_order", SqlDbType.Bit).Value = string.IsNullOrEmpty(inputdata[6]) ? (object)DBNull.Value : ParseToBool(inputdata[6]);
                s.Parameters.Add("@status", SqlDbType.Bit).Value = string.IsNullOrEmpty(inputdata[7]) ? (object)DBNull.Value : ParseToBool(inputdata[7]);
                s.Parameters.Add("@producer_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[8]) ? (object)DBNull.Value : int.Parse(inputdata[8]);

                s.ExecuteNonQuery();
                sqlConnection.Close();
                Upd_cars_Table();
            }
            else
            {
                reader.Close();
                MessageBox.Show("Автомобіль не знайдено", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Upd_cars_Table()
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
            menuItem.Click += Close_Tab; 
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
        private void Delete_Car(int car_id)
        {
            try
            {
                sqlConnection.Open();

                SqlCommand deleteSales = new SqlCommand("DELETE FROM Sales WHERE car_id = @car_id", sqlConnection);
                deleteSales.Parameters.Add("@car_id", SqlDbType.Int).Value = car_id;
                deleteSales.ExecuteNonQuery();

                SqlCommand deleteService = new SqlCommand("DELETE FROM Service WHERE car_id = @car_id", sqlConnection);
                deleteService.Parameters.Add("@car_id", SqlDbType.Int).Value = car_id;
                deleteService.ExecuteNonQuery();

                SqlCommand deleteCar = new SqlCommand("DELETE FROM Cars WHERE car_id = @car_id", sqlConnection);
                deleteCar.Parameters.Add("@car_id", SqlDbType.Int).Value = car_id;
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
            Upd_cars_Table();
        }


        private void Add_Client()
        {
            Window1 datawindow = new Window1();
            List<string> data = new List<string>
            {
                "@name",
                "@surname",
                "@phone",
                "@email",
                "@address",
                "@Password"
            };
            datawindow.dataToShow = data;
            datawindow.Fill();
            datawindow.ShowDialog();
            List<string> inputdata = datawindow.dataCollect;

            if (inputdata.Count == 6)
            {
                sqlConnection.Open();

                SqlCommand getMaxIdCommand = new SqlCommand("SELECT ISNULL(MAX(client_id), 0) + 1 FROM Clients", sqlConnection);
                int newClientId = (int)getMaxIdCommand.ExecuteScalar();

                SqlCommand s = new SqlCommand("insert_client", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = inputdata[0] ?? "";
                s.Parameters.Add("@surname", SqlDbType.NVarChar, 50).Value = inputdata[1] ?? "";
                s.Parameters.Add("@phone", SqlDbType.NVarChar, 20).Value = inputdata[2] ?? "";
                s.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = inputdata[3] ?? "";
                s.Parameters.Add("@address", SqlDbType.NVarChar, 255).Value = inputdata[4] ?? "";
                s.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = inputdata[5] ?? "";
                s.Parameters.Add("@client_id", SqlDbType.Int).Value = newClientId;

                s.ExecuteNonQuery();
                sqlConnection.Close();
                Upd_Clients_Table();
            }
        }

        private void Upd_Client(int clientId)
        {
            sqlConnection.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Clients WHERE client_id = @client_id", sqlConnection);
            cmd.Parameters.Add("@client_id", SqlDbType.Int).Value = clientId;

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                List<string> initialData = new List<string>
                {
                    reader["name"].ToString(),
                    reader["surname"].ToString(),
                    reader["phone"].ToString(),
                    reader["email"].ToString(),
                    reader["address"].ToString(),
                    reader["Password"].ToString()
                };
                reader.Close();
                Window1 datawindow = new Window1
                {
                    dataToShow = new List<string>
                    {
                        "@name",
                        "@surname",
                        "@phone",
                        "@email",
                        "@address",
                        "@Password"
                    },
                    initialData = initialData
                };
                datawindow.Fill();
                datawindow.ShowDialog();

                List<string> inputdata = datawindow.dataCollect;

                SqlCommand s = new SqlCommand("update_client", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@client_id", SqlDbType.Int).Value = clientId;
                s.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = inputdata[0] ?? "";
                s.Parameters.Add("@surname", SqlDbType.NVarChar, 50).Value = inputdata[1] ?? "";
                s.Parameters.Add("@phone", SqlDbType.NVarChar, 20).Value = inputdata[2] ?? "";
                s.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = inputdata[3] ?? "";
                s.Parameters.Add("@address", SqlDbType.NVarChar, 255).Value = inputdata[4] ?? "";
                s.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = inputdata[5] ?? "";

                s.ExecuteNonQuery();
                sqlConnection.Close();
            }
            else
            {
                reader.Close();
                MessageBox.Show("Клієнта не знайдено", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Upd_Clients_Table();
        }

        private void Upd_Clients_Table()
        {
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Clients", sqlConnection);
            sqlConnection.Open();
            foreach (TabItem item in tabs.Items)
            {
                if (string.Equals(item.Header.ToString(), "Clients", StringComparison.OrdinalIgnoreCase))
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
            menuItem.Click += Close_Tab;
            contextMenu.Items.Add(menuItem);

            tabs.Items.Add(new TabItem
            {
                Header = "Clients",
                Content = dataGrid,
                ContextMenu = contextMenu,
                IsSelected = true
            });
            sqlConnection.Close();
        }

        private void Delete_Client(int clientId)
        {
            try
            {
                sqlConnection.Open();

                SqlCommand deleteClient = new SqlCommand("DELETE FROM Clients WHERE client_id = @client_id", sqlConnection);
                deleteClient.Parameters.Add("@client_id", SqlDbType.Int).Value = clientId;
                deleteClient.ExecuteNonQuery();

                MessageBox.Show("Клієнта успішно видалено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при видаленні клієнта: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
            Upd_Clients_Table();
        }


        private void Add_Producer()
        {
            Window1 datawindow = new Window1();
            List<string> data = new List<string>
            {
                "@name",
                "@contact_person",
                "@phone",
                "@email",
                "@address"
            };
            datawindow.dataToShow = data;
            datawindow.Fill();
            datawindow.ShowDialog();
            List<string> inputdata = datawindow.dataCollect;

            if (inputdata.Count == 5)
            {
                sqlConnection.Open();

                SqlCommand getMaxIdCommand = new SqlCommand("SELECT ISNULL(MAX(producer_id), 0) + 1 FROM Producer", sqlConnection);
                int newProducerId = (int)getMaxIdCommand.ExecuteScalar();

                SqlCommand s = new SqlCommand("insert_producer", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = inputdata[0] ?? "";
                s.Parameters.Add("@contact_person", SqlDbType.NVarChar, 50).Value = inputdata[1] ?? "";
                s.Parameters.Add("@phone", SqlDbType.NVarChar, 20).Value = inputdata[2] ?? "";
                s.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = inputdata[3] ?? "";
                s.Parameters.Add("@address", SqlDbType.NVarChar, 255).Value = inputdata[4] ?? "";
                s.Parameters.Add("@producer_id", SqlDbType.Int).Value = newProducerId;

                s.ExecuteNonQuery();
                sqlConnection.Close();
                Upd_Producers_Table();
            }
        }

        private void Upd_Producer(int producerId)
        {
            sqlConnection.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Producer WHERE producer_id = @producer_id", sqlConnection);
            cmd.Parameters.Add("@producer_id", SqlDbType.Int).Value = producerId;

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                List<string> initialData = new List<string>
                {
                    reader["name"].ToString(),
                    reader["contact person"].ToString(),
                    reader["phone"].ToString(),
                    reader["email"].ToString(),
                    reader["address"].ToString()
                };
                reader.Close();

                Window1 datawindow = new Window1
                {
                    dataToShow = new List<string>
                    {
                        "@name",
                        "@contact_person",
                        "@phone",
                        "@email",
                        "@address"
                    },
                    initialData = initialData
                };
                datawindow.Fill();
                datawindow.ShowDialog();

                List<string> inputdata = datawindow.dataCollect;

                SqlCommand s = new SqlCommand("update_producer", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@producer_id", SqlDbType.Int).Value = producerId;
                s.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = inputdata[0] ?? "";
                s.Parameters.Add("@contact_person", SqlDbType.NVarChar, 50).Value = inputdata[1] ?? "";
                s.Parameters.Add("@phone", SqlDbType.NVarChar, 20).Value = inputdata[2] ?? "";
                s.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = inputdata[3] ?? "";
                s.Parameters.Add("@address", SqlDbType.NVarChar, 255).Value = inputdata[4] ?? "";

                s.ExecuteNonQuery();
                sqlConnection.Close();
            }
            else
            {
                reader.Close();
                MessageBox.Show("Постачальника не знайдено", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Upd_Producers_Table();
        }

        private void Upd_Producers_Table()
        {
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Producer", sqlConnection);
            sqlConnection.Open();
            foreach (TabItem item in tabs.Items)
            {
                if (string.Equals(item.Header.ToString(), "Producer", StringComparison.OrdinalIgnoreCase))
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
            menuItem.Click += Close_Tab;
            contextMenu.Items.Add(menuItem);

            tabs.Items.Add(new TabItem
            {
                Header = "Producer",
                Content = dataGrid,
                ContextMenu = contextMenu,
                IsSelected = true
            });
            sqlConnection.Close();
        }

        private void Delete_Producer(int producerId)
        {
            try
            {
                sqlConnection.Open();

                SqlCommand deleteProducer = new SqlCommand("DELETE FROM Producer WHERE producer_id = @producer_id", sqlConnection);
                deleteProducer.Parameters.Add("@producer_id", SqlDbType.Int).Value = producerId;
                deleteProducer.ExecuteNonQuery();

                MessageBox.Show("Постачальника успішно видалено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при видаленні постачальника: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
            Upd_Producers_Table();
        }
        
        
        private void Add_sale()
        {
            Window1 datawindow = new Window1();
            List<string> data = new List<string>
            {
                "@date_of_sale",
                "@sum",
                "@payment_method",
                "@car_id",
                "@client_id"
            };
            datawindow.dataToShow = data;
            datawindow.Fill();
            datawindow.ShowDialog();
            List<string> inputdata = datawindow.dataCollect;

            if (inputdata.Count == 5)
            {
                sqlConnection.Open();

                SqlCommand getMaxIdCommand = new SqlCommand("SELECT ISNULL(MAX(sale_id), 0) + 1 FROM Sales", sqlConnection);
                int newSaleId = (int)getMaxIdCommand.ExecuteScalar();

                SqlCommand s = new SqlCommand("insert_sale", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@sale_id", SqlDbType.Int).Value = newSaleId;
                s.Parameters.Add("@date_of_sale", SqlDbType.Date).Value = string.IsNullOrEmpty(inputdata[0]) ? (object)DBNull.Value : DateTime.Parse(inputdata[0]);
                s.Parameters.Add("@sum", SqlDbType.Decimal).Value = string.IsNullOrEmpty(inputdata[1]) ? (object)DBNull.Value : decimal.Parse(inputdata[1]);
                s.Parameters.Add("@payment_method", SqlDbType.NVarChar, 50).Value = inputdata[2] ?? "";
                s.Parameters.Add("@car_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[3]) ? (object)DBNull.Value : int.Parse(inputdata[3]);
                s.Parameters.Add("@client_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[4]) ? (object)DBNull.Value : int.Parse(inputdata[4]);

                s.ExecuteNonQuery();
                sqlConnection.Close();
                Upd_sales_Table();
            }
        }
        private void Upd_sale(int saleId)
        {
            sqlConnection.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Sales WHERE sale_id = @sale_id", sqlConnection);
            cmd.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                List<string> initialData = new List<string>
                {
                    reader["date of sale"].ToString(),
                    reader["sum"].ToString(),
                    reader["payment method"].ToString(),
                    reader["car_id"].ToString(),
                    reader["client_id"].ToString()
                };
                reader.Close();

                Window1 datawindow = new Window1
                {
                    dataToShow = new List<string>
                    {
                        "@date_of_sale",
                        "@sum",
                        "@payment_method",
                        "@car_id",
                        "@client_id"
                    },
                    initialData = initialData
                };
                datawindow.Fill();
                datawindow.ShowDialog();

                List<string> inputdata = datawindow.dataCollect;

                SqlCommand s = new SqlCommand("update_sale", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                s.Parameters.Add("@date_of_sale", SqlDbType.Date).Value = string.IsNullOrEmpty(inputdata[0]) ? (object)DBNull.Value : DateTime.Parse(inputdata[0]);
                s.Parameters.Add("@sum", SqlDbType.Decimal).Value = string.IsNullOrEmpty(inputdata[1]) ? (object)DBNull.Value : decimal.Parse(inputdata[1]);
                s.Parameters.Add("@payment_method", SqlDbType.NVarChar, 50).Value = inputdata[2] ?? "";
                s.Parameters.Add("@car_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[3]) ? (object)DBNull.Value : int.Parse(inputdata[3]);
                s.Parameters.Add("@client_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[4]) ? (object)DBNull.Value : int.Parse(inputdata[4]);

                s.ExecuteNonQuery();
                sqlConnection.Close();
            }
            else
            {
                reader.Close();
                MessageBox.Show("Продаж не знайдено", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Upd_sales_Table();
        }
        private void Upd_sales_Table()
        {
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Sales", sqlConnection);
            sqlConnection.Open();
            foreach (TabItem item in tabs.Items)
            {
                if (string.Equals(item.Header.ToString(), "Sales", StringComparison.OrdinalIgnoreCase))
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
            menuItem.Click += Close_Tab;
            contextMenu.Items.Add(menuItem);

            tabs.Items.Add(new TabItem
            {
                Header = "Sales",
                Content = dataGrid,
                ContextMenu = contextMenu,
                IsSelected = true
            });
            sqlConnection.Close();
        }
        private void Delete_Sale(int saleId)
        {
            try
            {
                sqlConnection.Open();

                SqlCommand deleteSale = new SqlCommand("DELETE FROM Sales WHERE sale_id = @sale_id", sqlConnection);
                deleteSale.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                deleteSale.ExecuteNonQuery();

                MessageBox.Show("Продаж успішно видалено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при видаленні продажу: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
            Upd_sales_Table();
        }

        private void Add_Service()
        {
            Window1 datawindow = new Window1();
            List<string> data = new List<string>
            {
                "@date_service",
                "@description",
                "@price",
                "@car_id"
            };
            datawindow.dataToShow = data;
            datawindow.Fill();
            datawindow.ShowDialog();
            List<string> inputdata = datawindow.dataCollect;

            if (inputdata.Count == 4)
            {
                sqlConnection.Open();

                SqlCommand getMaxIdCommand = new SqlCommand("SELECT ISNULL(MAX(service_id), 0) + 1 FROM Service", sqlConnection);
                int newServiceId = (int)getMaxIdCommand.ExecuteScalar();

                SqlCommand s = new SqlCommand("insert_service", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@service_id", SqlDbType.Int).Value = newServiceId;
                s.Parameters.Add("@date_service", SqlDbType.Date).Value = string.IsNullOrEmpty(inputdata[0]) ? (object)DBNull.Value : DateTime.Parse(inputdata[0]);
                s.Parameters.Add("@description", SqlDbType.NVarChar, 50).Value = inputdata[1] ?? "";
                s.Parameters.Add("@price", SqlDbType.Decimal).Value = string.IsNullOrEmpty(inputdata[2]) ? (object)DBNull.Value : decimal.Parse(inputdata[2]);
                s.Parameters.Add("@car_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[3]) ? (object)DBNull.Value : int.Parse(inputdata[3]);

                s.ExecuteNonQuery();
                sqlConnection.Close();
                Upd_Services_Table();
            }
        }
        private void Upd_Service(int serviceId)
        {
            sqlConnection.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Service WHERE service_id = @service_id", sqlConnection);
            cmd.Parameters.Add("@service_id", SqlDbType.Int).Value = serviceId;

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                List<string> initialData = new List<string>
                {
                    reader["date service"].ToString(),
                    reader["description"].ToString(),
                    reader["price"].ToString(),
                    reader["car_id"].ToString()
                };
                reader.Close();

                Window1 datawindow = new Window1
                {
                    dataToShow = new List<string> { "@date_service", "@description", "@price", "@car_id" },
                    initialData = initialData
                };
                datawindow.Fill();
                datawindow.ShowDialog();

                List<string> inputdata = datawindow.dataCollect;

                SqlCommand s = new SqlCommand("update_service", sqlConnection);
                s.CommandType = CommandType.StoredProcedure;

                s.Parameters.Add("@service_id", SqlDbType.Int).Value = serviceId;
                s.Parameters.Add("@date_service", SqlDbType.Date).Value = string.IsNullOrEmpty(inputdata[0]) ? (object)DBNull.Value : DateTime.Parse(inputdata[0]);
                s.Parameters.Add("@description", SqlDbType.NVarChar, 50).Value = inputdata[1] ?? "";
                s.Parameters.Add("@price", SqlDbType.Decimal).Value = string.IsNullOrEmpty(inputdata[2]) ? (object)DBNull.Value : decimal.Parse(inputdata[2]);
                s.Parameters.Add("@car_id", SqlDbType.Int).Value = string.IsNullOrEmpty(inputdata[3]) ? (object)DBNull.Value : int.Parse(inputdata[3]);

                s.ExecuteNonQuery();
                sqlConnection.Close();
            }
            else
            {
                reader.Close();
                MessageBox.Show("Послугу не знайдено", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Upd_Services_Table();
        }
        private void Upd_Services_Table()
        {
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Service", sqlConnection);
            sqlConnection.Open();

            foreach (TabItem item in tabs.Items)
            {
                if (string.Equals(item.Header.ToString(), "Service", StringComparison.OrdinalIgnoreCase))
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
            MenuItem menuItem = new MenuItem { Header = "Закрити" };
            menuItem.Click += Close_Tab;
            contextMenu.Items.Add(menuItem);

            tabs.Items.Add(new TabItem
            {
                Header = "Service",
                Content = dataGrid,
                ContextMenu = contextMenu,
                IsSelected = true
            });

            sqlConnection.Close();
        }
        private void Delete_Service(int serviceId)
        {
            try
            {
                sqlConnection.Open();

                SqlCommand deleteService = new SqlCommand("delete_service", sqlConnection);
                deleteService.CommandType = CommandType.StoredProcedure;
                deleteService.Parameters.Add("@service_id", SqlDbType.Int).Value = serviceId;
                deleteService.ExecuteNonQuery();

                MessageBox.Show("Послуга успішно видалена.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при видаленні послуги: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
            Upd_Services_Table();
        }

    }

}
