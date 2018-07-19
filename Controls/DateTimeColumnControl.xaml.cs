using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace SQL_Dummy_Data_Generator.Controls
{
    /// <summary>
    /// Interaction logic for DateTimeColumnControl.xaml
    /// </summary>
    public partial class DateTimeColumnControl : UserControl
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
        public string Value { get; set; }

        public DateTimeColumnControl()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            if (selectedItem.Content is "Static")
            {
                Value = $"'{MinDateTimePicker.Value}'";
            }
            else if (selectedItem.Content is "Random")
            {
                if (MaxDateTimePicker.Value < MinDateTimePicker.Value)
                    throw new Exception($"{ColumnName}: Max date sholud be greater than min date");
                var range = (MaxDateTimePicker.Value - MinDateTimePicker.Value)?.Days;
                var randomVal = Random.Next(range ?? 0);
                Value = $"'{MinDateTimePicker.Value?.AddDays(randomVal)}'";
            }
            else if (selectedItem.Content is "Repeat")
            {
                if (MaxDateTimePicker.Value < MinDateTimePicker.Value)
                    throw new Exception($"{ColumnName}: Max date sholud be greater than min date");
                var date = Convert.ToDateTime(Value);
                if (date < MinDateTimePicker.Value || date > MaxDateTimePicker.Value)
                    Value = $"'{MinDateTimePicker.Value}'";
                else
                    Value = $"'{date.AddDays(1)}'";
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
                    Value = _dataTable.Rows[Random.Next(0, _dataTable.Rows.Count)][
                        $"{Referenced.ReferencedColumnName}"].ToString();
            }
        }

        private void ValueChanged()
        {
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            if (MaxDateTimePicker == null || MinDateTimePicker == null) return;
            switch (selectedItem.Content)
            {
                case "Static":
                    MaxDateTimePicker.Visibility = Visibility.Hidden;
                    MinDateTimePicker.Visibility = Visibility.Visible;
                    MinDateTimePicker.Watermark = "Value";
                    break;
                case "Random":
                    MaxDateTimePicker.Visibility = Visibility.Visible;
                    MinDateTimePicker.Visibility = Visibility.Visible;
                    MinDateTimePicker.Watermark = "Minimum";
                    break;
                case "Repeat":
                    MaxDateTimePicker.Visibility = Visibility.Visible;
                    MinDateTimePicker.Visibility = Visibility.Visible;
                    MinDateTimePicker.Watermark = "Minimum";
                    break;
                default:
                    MaxDateTimePicker.Visibility = Visibility.Hidden;
                    MinDateTimePicker.Visibility = Visibility.Hidden;
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
