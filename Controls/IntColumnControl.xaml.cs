using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace SQL_Dummy_Data_Generator.Controls
{
    /// <summary>
    /// Interaction logic for IntColumnControl.xaml
    /// </summary>
    public partial class IntColumnControl : UserControl
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

        public IntColumnControl()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            var min = MinIntUpDown.Value ?? 0;
            var max = MaxIntUpDown.Value ?? 0;
            if (selectedItem.Content is "Static")
            {
                Value = min;
            }
            else if (selectedItem.Content is "Random")
            {
                if (max < min) throw new Exception($"{ColumnName}: Max number should be greater than min number");
                Value = Random.Next(min, max);
            }
            else if (selectedItem.Content is "Repeat")
            {
                if (max < min) throw new Exception($"{ColumnName}: Max number should be greater than min number");
                if (Value < min || Value > max)
                    Value = min;
                else
                    Value++;
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

                if(_dataTable.Rows.Count > 0)
                    Value = (int)_dataTable.Rows[Random.Next(0, _dataTable.Rows.Count)][
                        $"{Referenced.ReferencedColumnName}"];
            }
        }

        private void ValueChanged()
        {
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            if (MaxIntUpDown == null || MinIntUpDown == null) return;
            switch (selectedItem.Content)
            {
                case "Static":
                    MaxIntUpDown.Visibility = Visibility.Hidden;
                    MinIntUpDown.Visibility = Visibility.Visible;
                    MinIntUpDown.Watermark = "Value";
                    break;
                case "Random":
                    MaxIntUpDown.Visibility = Visibility.Visible;
                    MinIntUpDown.Visibility = Visibility.Visible;
                    MinIntUpDown.Watermark = "Minimum";
                    break;
                case "Repeat":
                    MaxIntUpDown.Visibility = Visibility.Visible;
                    MinIntUpDown.Visibility = Visibility.Visible;
                    MinIntUpDown.Watermark = "Minimum";
                    break;
                default:
                    MaxIntUpDown.Visibility = Visibility.Hidden;
                    MinIntUpDown.Visibility = Visibility.Hidden;
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
