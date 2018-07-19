using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace SQL_Dummy_Data_Generator.Controls
{
    /// <summary>
    /// Interaction logic for DecimalColumnControl.xaml
    /// </summary>
    public partial class DecimalColumnControl : UserControl
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
        public double Value { get; set; }
        public DecimalColumnControl()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            var min = (double)(MinDecimalUpDown.Value ?? 0);
            var max = (double)(MaxDecimalUpDown.Value ?? 0);
            if (selectedItem.Content is "Static")
            {
                Value = min;
            }
            else if (selectedItem.Content is "Random")
            {
                if (max < min) throw new Exception($"{ColumnName}: Max number should be greater than min number");
                Value = Random.Next((int) min, (int) max) + Random.NextDouble();
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

                if (_dataTable.Rows.Count > 0)
                    Value = (double)_dataTable.Rows[Random.Next(0, _dataTable.Rows.Count)][
                        $"{Referenced.ReferencedColumnName}"];
            }
        }

        private void ValueChanged()
        {
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            if (MaxDecimalUpDown == null || MinDecimalUpDown == null) return;
            switch (selectedItem.Content)
            {
                case "Static":
                    MaxDecimalUpDown.Visibility = Visibility.Hidden;
                    MinDecimalUpDown.Visibility = Visibility.Visible;
                    MinDecimalUpDown.Watermark = "Value";
                    break;
                case "Random":
                    MaxDecimalUpDown.Visibility = Visibility.Visible;
                    MinDecimalUpDown.Visibility = Visibility.Visible;
                    MinDecimalUpDown.Watermark = "Minimum";
                    break;
                case "Repeat":
                    MaxDecimalUpDown.Visibility = Visibility.Visible;
                    MinDecimalUpDown.Visibility = Visibility.Visible;
                    MinDecimalUpDown.Watermark = "Minimum";
                    break;
                default:
                    MaxDecimalUpDown.Visibility = Visibility.Hidden;
                    MinDecimalUpDown.Visibility = Visibility.Hidden;
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
