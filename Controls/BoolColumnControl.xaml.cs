using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace SQL_Dummy_Data_Generator.Controls
{
    /// <summary>
    /// Interaction logic for BoolColumnControl.xaml
    /// </summary>
    public partial class BoolColumnControl : UserControl
    {
        private static readonly Random Random = new Random();
        private DataTable _dataTable = new DataTable();

        public string ColumnName
        {
            get => ColumnNameLabel.Content.ToString();
            set => ColumnNameLabel.Content = value;
        }

        public string ColumnType
        {
            get => ColumnTypeLabel.Content.ToString();
            set => ColumnTypeLabel.Content = value;
        }
        public ForeignKeyModel Referenced;
        public int Value { get; set; }

        public BoolColumnControl()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            if (selectedItem.Content is "Static")
                Value = Convert.ToInt16(ValueCheckBox.IsChecked);
            else if (selectedItem.Content is "Random")
            {
                Value = Random.Next(2);
            }
            else if (selectedItem.Content.ToString() == $"Referenced Table ({Referenced.ReferencedTableName}.{Referenced.ReferencedColumnName})")
            {
                if (_dataTable.Rows.Count == 0)
                    using (var con = new SqlConnection(Referenced.ConnectionString))
                    {
                        con.Open();
                        using (var cmd =
                            new SqlCommand(
                                $"SELECT {Referenced.ReferencedColumnName} FROM {Referenced.ReferencedTableName}", con))
                        {
                            var da = new SqlDataAdapter(cmd);
                            da.Fill(_dataTable);
                            da.Dispose();
                        }

                        con.Close();
                    }

                if (_dataTable.Rows.Count > 0)
                    Value = (int)_dataTable.Rows[Random.Next(0, _dataTable.Rows.Count)][
                        $"{Referenced.ReferencedColumnName}"];
            }
        }

        private void ValueChanged()
        {
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            if (ValueCheckBox == null) return;
            switch (selectedItem.Content)
            {
                case "Static":
                    ValueCheckBox.Visibility = Visibility.Visible;
                    break;
                case "Random":
                    ValueCheckBox.Visibility = Visibility.Hidden;
                    break;
                default:
                    ValueCheckBox.Visibility = Visibility.Hidden;
                    break;
            }

        }

        private void DuplicateTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValueChanged();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ValueChanged();
            if (Referenced != null)
            {
                DuplicateTypeCombo.Items.Add(new ComboBoxItem
                {
                    Content = $"Referenced Table ({Referenced.ReferencedTableName}.{Referenced.ReferencedColumnName})"
                });
            }
        }
    }
}
