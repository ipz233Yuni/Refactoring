using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class Window1 : Window
    {
        public List<string> dataToShow = new List<string>();
        public List<string> dataCollect = new List<string>();
        public List<string> initialData = new List<string>();
        public event EventHandler<List<string>> OnSaveData;
        public Window1()
        {
            InitializeComponent();
        }
        public void Fill()
        {
            ObservableCollection<DataItem> items = new ObservableCollection<DataItem>();
            for (int i = 0; i < dataToShow.Count; i++)
            {
                var dataItem = new DataItem
                {
                    ValueFromDataToShow = dataToShow[i],
                    UserInputValue = initialData.ElementAtOrDefault(i) 
                };
                items.Add(dataItem);
            }
            dataAsk.ItemsSource = items;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<string> userInputData = new List<string>();
            foreach (DataItem item in dataAsk.ItemsSource)
            {
                userInputData.Add(item.UserInputValue);
            }
            if (userInputData.Any(string.IsNullOrEmpty))
            {
                var emptyFields = dataToShow
                    .Where((field, index) => string.IsNullOrEmpty(userInputData.ElementAtOrDefault(index)))
                    .Select(field => field.TrimStart('@'))
                    .ToList();

                string errorMessage = "Не заповнені обов'язкові поля: " + string.Join(", ", emptyFields);
                MessageBox.Show($"{errorMessage}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dataCollect = userInputData;
            OnSaveData?.Invoke(this, dataCollect);
            this.Close();
        }

    }
    public class DataItem
    {
        public string ValueFromDataToShow { get; set; }
        public string UserInputValue { get; set; }
    }

}
