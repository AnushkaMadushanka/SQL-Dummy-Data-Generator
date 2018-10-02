using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Bogus;
using SQL_Dummy_Data_Generator.Models;

namespace SQL_Dummy_Data_Generator.Controls
{
    /// <summary>
    /// Interaction logic for StringColumnControl.xaml
    /// </summary>
    public partial class StringColumnControl : UserControl
    {
        private Fare.Xeger _rxrdg;
        private string _currentText;
        private DataTable _dataTable = new DataTable();
        private static readonly Random Random = new Random();


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
        public StringColumnControl()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            var textSize = TextSizeIntUpDown.Value ?? 0;
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            var faker = DashboardPage.Fakers.FirstOrDefault(i => i.Id == textSize);
            var isNew = faker == null;
            var bogusFaker = new Faker();
            faker = faker ?? new FakerModel{Id = textSize};

            if (selectedItem.Content is "Static")
            {
                Value = $"{StaticTextBox.Text}";
            }
            else if (selectedItem.Content is "Static Text + GUID")
            {
                if (textSize < StaticTextBox.Text.Length)
                    throw new Exception(
                        $"{ColumnName}: Max character count should be greater than no. Of character in textbox");
                var guid = Guid.NewGuid().ToString("N")
                    .Substring(0, textSize - StaticTextBox.Text.Length);
                Value = $"{StaticTextBox.Text + guid}";
            }
            else if (selectedItem.Content is "GUID")
            {
                Value = $"{Guid.NewGuid().ToString("N").Substring(0, textSize)}";
            }
            else if (selectedItem.Content is "RegEx")
            {
                if (_currentText != StaticTextBox.Text)
                {
                    _rxrdg = new Fare.Xeger(StaticTextBox.Text);
                    _currentText = StaticTextBox.Text;
                }

                Value = $"{_rxrdg.Generate()}";
            }
            else if (selectedItem.Content.ToString() == $"Referenced Table ({Referenced?.ReferencedTableName}.{Referenced?.ReferencedColumnName})")
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
            else if (selectedItem.Content is "FirstName")
            {
                faker.FirstName = Value = string.IsNullOrEmpty(faker.FirstName) ? bogusFaker.Name.FirstName(faker.IsMale
                    ? Bogus.DataSets.Name.Gender.Male
                    : Bogus.DataSets.Name.Gender.Female) : faker.FirstName;
            }
            else if (selectedItem.Content is "LastName") {
                faker.LastName = Value = string.IsNullOrEmpty(faker.LastName) ? bogusFaker.Name.LastName(faker.IsMale
                    ? Bogus.DataSets.Name.Gender.Male
                    : Bogus.DataSets.Name.Gender.Female) : faker.LastName;
            }
            else if (selectedItem.Content is "FullName")
            {
                faker.FirstName = string.IsNullOrEmpty(faker.FirstName)? bogusFaker.Name.FirstName(faker.IsMale
                    ? Bogus.DataSets.Name.Gender.Male
                    : Bogus.DataSets.Name.Gender.Female) : faker.FirstName;
                faker.LastName = string.IsNullOrEmpty(faker.LastName) ? bogusFaker.Name.LastName(faker.IsMale
                    ? Bogus.DataSets.Name.Gender.Male
                    : Bogus.DataSets.Name.Gender.Female): faker.LastName;
                faker.FullName = Value = faker.FirstName + " " + faker.LastName;
            }
            else if (selectedItem.Content is "Prefix") { faker.Prefix = Value = bogusFaker.Name.Prefix(faker.IsMale ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female); }
            else if (selectedItem.Content is "Suffix") { faker.Suffix = Value = bogusFaker.Name.Suffix(); }
            else if (selectedItem.Content is "JobTitle") { faker.JobTitle = Value = bogusFaker.Name.JobTitle(); }
            else if (selectedItem.Content is "JobDescriptor") { faker.JobDescriptor = Value = bogusFaker.Name.JobDescriptor(); }
            else if (selectedItem.Content is "JobArea") { faker.JobArea = Value = bogusFaker.Name.JobArea(); }
            else if (selectedItem.Content is "JobType") { faker.JobType = Value = bogusFaker.Name.JobType(); }

            if (isNew)
                DashboardPage.Fakers.Add(faker);
            else
            {
                var index = DashboardPage.Fakers.FindIndex(i => i.Id == faker.Id);
                DashboardPage.Fakers[index] = faker;
            }
        }

        private void ValueChanged()
        {
            var selectedItem = (ComboBoxItem)DuplicateTypeCombo.SelectedItem;
            if (StaticTextBox == null || TextSizeIntUpDown == null) return;
            switch (selectedItem.Content)
            {
                case "Static":
                    TextSizeIntUpDown.Visibility = Visibility.Hidden;
                    StaticTextBox.Visibility = Visibility.Visible;
                    break;
                case "Static Text + GUID":
                    TextSizeIntUpDown.Visibility = Visibility.Visible;
                    StaticTextBox.Visibility = Visibility.Visible;
                    break;
                case "GUID":
                    TextSizeIntUpDown.Visibility = Visibility.Visible;
                    StaticTextBox.Visibility = Visibility.Hidden;
                    break;
                case "RegEx":
                    TextSizeIntUpDown.Visibility = Visibility.Hidden;
                    StaticTextBox.Visibility = Visibility.Visible;
                    break;
                case "FirstName":
                case "LastName":
                case "FullName":
                case "Prefix":
                case "Suffix":
                case "JobTitle":
                case "JobDescriptor":
                case "JobArea":
                case "JobType":
                    TextSizeIntUpDown.Visibility = Visibility.Visible;
                    StaticTextBox.Visibility = Visibility.Hidden;
                    break;
                default:
                    TextSizeIntUpDown.Visibility = Visibility.Hidden;
                    StaticTextBox.Visibility = Visibility.Hidden;
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
