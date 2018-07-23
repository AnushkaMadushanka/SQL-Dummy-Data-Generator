using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SQL_Dummy_Data_Generator.Controls;

namespace SQL_Dummy_Data_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string ConnectionString = "Data Source=DESKTOP-K34O5QH\\SQLEXPRESS; Integrated Security=True;";

        public MainWindow()
        {
            InitializeComponent();
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ConnectionPage();
        }
    }

    public class ForeignKeyModel{
        public string ReferencingColumnName { get; set; }
        public string ConnectionString { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }

    }
}
