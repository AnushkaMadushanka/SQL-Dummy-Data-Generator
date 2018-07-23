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
using MessageBox = System.Windows.MessageBox;

namespace SQL_Dummy_Data_Generator
{
    /// <summary>
    /// Interaction logic for ConnectionPage.xaml
    /// </summary>
    public partial class ConnectionPage : Page
    {
        public ConnectionPage()
        {
            InitializeComponent();
        }

        private async void SaveBt_Click(object sender, RoutedEventArgs e)
        {
            var connectionString = "";
            if (string.IsNullOrEmpty(ServerNameTb.Text))
            {
                MessageBox.Show("Server Name is required");
                return;
            }

            if (AuthCb.SelectedIndex == 1 && string.IsNullOrEmpty(UsernameTb.Text))
            {
                MessageBox.Show("Username is required");
                return;
            }

            if (AuthCb.SelectedIndex == 1 && string.IsNullOrEmpty(PasswordPb.Password))
            {
                MessageBox.Show("Password is required");
                return;
            }

            connectionString += $"Data Source={ServerNameTb.Text};";
            if (AuthCb.SelectedIndex == 0)
                connectionString += "Integrated Security=True;";
            else
                connectionString += $"User ID={UsernameTb.Text};Password={PasswordPb.Password};";
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindow.ConnectionString = connectionString;
                        });
                        con.Close();
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var dashboardPage = new DashboardPage();
                        NavigationService?.Navigate(dashboardPage);
                    });
                }
                catch (Exception exception)
                {
                    Dispatcher.Invoke(() => { MessageBox.Show(exception.Message); });
                }
            });
        }
    }
}
