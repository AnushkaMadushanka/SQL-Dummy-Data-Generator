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
        private const string ConnectionString = "Data Source=DESKTOP-K34O5QH\\SQLEXPRESS; Integrated Security=True;";
        private string _database;
        private string _table;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {
                using (var con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("SELECT name from sys.databases where owner_sid <> 1", con))
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                DatabasePanel.Dispatcher.Invoke(() =>
                                {
                                    var newButton = new Button()
                                    {
                                        Name = $"{dr[0]}_button",
                                        Content = dr[0].ToString(),
                                        Height = 50
                                    };
                                    newButton.Click += DatabaseButtonOnClick;
                                    DatabasePanel.Children.Add(newButton);
                                });
                            }
                        }
                    }

                    con.Close();
                }
            });
        }

        public async void DatabaseButtonOnClick(object sender, RoutedEventArgs e)
        {
            TablePanel.Children.Clear();
            var btn = (Button) sender;
            _database = btn.Content.ToString();
            var connectionString = ConnectionString + $"Database = {_database};";

            await Task.Factory.StartNew(() =>
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (var cmd =
                        new SqlCommand(
                            "SELECT t.name,sc.name AS [schema] from sys.tables t INNER JOIN sys.schemas sc ON sc.[schema_id] = t.[schema_id]",
                            con))
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                TablePanel.Dispatcher.Invoke(() =>
                                {
                                    var newButton = new Button()
                                    {
                                        Name = $"{dr["name"]}_button",
                                        Content = (!string.IsNullOrEmpty(dr["schema"].ToString())
                                                      ? $"[{dr["schema"]}]."
                                                      : "") + $@"[{dr["name"]}]",
                                        Height = 50
                                    };
                                    newButton.Click += TableButtonOnClick;
                                    TablePanel.Children.Add(newButton);
                                });

                            }
                        }
                    }

                    con.Close();

                }
            });
        }

        public async void TableButtonOnClick(object sender, RoutedEventArgs e)
        {
            ColumnPanel.Children.Clear();
            var btn = (Button)sender;
            _table = btn.Content.ToString();
            //btn.selected = true;
            var connectionString = ConnectionString + $"Database = {_database};";
            await Task.Factory.StartNew(() =>
            {
                var foreignKeys = new List<ForeignKeyModel>();
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var table = _table.Split('.');
                    var query = "SELECT KCU.COLUMN_NAME[referencing_column_name]" +
                                ",C2.TABLE_NAME[referenced_table_name] " +
                                ",KCU2.COLUMN_NAME[referenced_column_name] " +
                                "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS C " +
                                "INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU " +
                                "ON C.CONSTRAINT_SCHEMA = KCU.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME " +
                                "INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC " +
                                "ON C.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = RC.CONSTRAINT_NAME " +
                                "INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS C2 " +
                                "ON RC.UNIQUE_CONSTRAINT_SCHEMA = C2.CONSTRAINT_SCHEMA AND RC.UNIQUE_CONSTRAINT_NAME = C2.CONSTRAINT_NAME " +
                                "INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2 " +
                                "ON C2.CONSTRAINT_SCHEMA = KCU2.CONSTRAINT_SCHEMA AND C2.CONSTRAINT_NAME = KCU2.CONSTRAINT_NAME " +
                                "AND KCU.ORDINAL_POSITION = KCU2.ORDINAL_POSITION " +
                                $"WHERE C.CONSTRAINT_TYPE = 'FOREIGN KEY' AND C.TABLE_NAME = '{table[1].TrimStart('[').TrimEnd(']')}' AND KCU.CONSTRAINT_SCHEMA = '{table[0].TrimStart('[').TrimEnd(']')}'" +
                                "ORDER BY C.CONSTRAINT_NAME";
                    using (var cmd = new SqlCommand(query, con))
                    using (var dr = cmd.ExecuteReader())
                        while (dr.Read())
                            foreignKeys.Add(new ForeignKeyModel
                            {
                                ReferencingColumnName = dr[0].ToString(),
                                ConnectionString = connectionString,
                                ReferencedTableName = dr[1].ToString(),
                                ReferencedColumnName = dr[2].ToString()
                            });
                    con.Close();
                }

                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var query = "SELECT c.name 'Column Name'," +
                                "t.Name 'Data type'," +
                                "c.max_length 'Max Length'," +
                                "c.precision ," +
                                "c.scale ," +
                                "c.is_nullable," +
                                "ISNULL(i.is_primary_key, 0) 'Primary Key'" +
                                " FROM " +
                                "sys.columns c " +
                                "INNER JOIN sys.types t ON c.user_type_id = t.user_type_id " +
                                "LEFT OUTER JOIN sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id " +
                                "LEFT OUTER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id " +
                                $"WHERE c.object_id = OBJECT_ID('{_table}')";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                dynamic newColumnControl;
                                var dataType = dr["Data type"].ToString();
                                var primaryKey = Convert.ToBoolean(dr["Primary Key"]);
                                if (primaryKey)
                                    continue;
                                ColumnPanel.Dispatcher.Invoke(() =>
                                {
                                    switch (dataType.ToLower())
                                    {
                                        case "int":
                                            newColumnControl = new IntColumnControl();
                                            break;
                                        case "nvarchar":
                                            newColumnControl = new StringColumnControl();
                                            break;
                                        case "bit":
                                            newColumnControl = new BoolColumnControl();
                                            break;
                                        case "datetime":
                                        case "datetime2":
                                            newColumnControl = new DateTimeColumnControl();
                                            break;
                                        default:
                                            newColumnControl = new StringColumnControl();
                                            break;
                                    }
                                    newColumnControl.Name = $"{dr[0]}_button";
                                    newColumnControl.ColumnName = dr["Column Name"].ToString();
                                    newColumnControl.ColumnType = dataType;
                                    newColumnControl.Referenced = foreignKeys.FirstOrDefault(i =>
                                        i.ReferencingColumnName == newColumnControl.ColumnName);
                                    ColumnPanel.Children.Add(newColumnControl);
                                });
                            }
                        }
                    }
                    con.Close();
                }
            });
            BottomGrid.IsEnabled = true;
        }

        private void SqlButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = @"Save SQL File",
                DefaultExt = "sql",
                Filter = @"SQL files (*.sql)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                AddExtension = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                CreateSqlFile(saveFileDialog.FileName);
            }
        }

        private async void CreateSqlFile(string path)
        {
            try
            {
                BottomGrid.IsEnabled = false;
                var numberOfRecords = RecordCount.Value ?? 0;
                await Task.Factory.StartNew(() =>
                {
                    var sb = new StringBuilder($"USE[{_database}]{Environment.NewLine}GO{Environment.NewLine}");
                    var controls = new List<Control>();
                    ColumnPanel.Dispatcher.Invoke(() => { controls = ColumnPanel.Children.Cast<Control>().ToList(); });

                    for (var i = 0; i < numberOfRecords; i++)
                    {
                        var columnString = "";
                        var valueString = "";
                        ColumnPanel.Dispatcher.Invoke(() =>
                        {
                            foreach (dynamic control in controls)
                            {
                                columnString += $"[{control.ColumnName}],";
                                control.Initialize();
                                if (control is StringColumnControl)
                                    valueString += $"'{control.Value}',";
                                else
                                    valueString += $"{control.Value},";
                            }
                        });

                        columnString = columnString.Remove(columnString.Length - 1);
                        valueString = valueString.Remove(valueString.Length - 1);
                        var query =
                            $"INSERT INTO {_table}({columnString})VALUES({valueString}){Environment.NewLine}GO{Environment.NewLine}";
                        sb.Append(query);
                        var i1 = i;
                        ProgressBar.Dispatcher.Invoke(() => { ProgressBar.Value = (int)(i1 / (float)numberOfRecords * 100); });
                    }

                    ProgressBar.Dispatcher.Invoke(() => { ProgressBar.Value = 100; });
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        var directoryName = Path.GetDirectoryName(path);
                        if (directoryName != null && !Directory.Exists(directoryName))
                            Directory.CreateDirectory(directoryName);
                        File.WriteAllText(path, sb.ToString());
                    }

                    sb.Clear();
                });
                MessageBox.Show(@"Done");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                BottomGrid.IsEnabled = true;
                ProgressBar.Value = 0;
            }
        }

        private async void RunSqlCommands()
        {
            try
            {
                BottomGrid.IsEnabled = false;
                var numberOfRecords = RecordCount.Value ?? 0;
                await Task.Factory.StartNew(() =>
                {
                    var controls = new List<Control>();
                    ColumnPanel.Dispatcher.Invoke(() => { controls = ColumnPanel.Children.Cast<Control>().ToList(); });

                    var dt = new DataTable();
                    ColumnPanel.Dispatcher.Invoke(() =>
                    {
                        foreach (dynamic control in controls)
                            dt.Columns.Add(control.ColumnName);
                    });
                    for (var i = 0; i < numberOfRecords; i++)
                    {
                        var row = dt.NewRow();
                        ColumnPanel.Dispatcher.Invoke(() =>
                        {
                            foreach (dynamic control in controls)
                            {
                                control.Initialize();
                                row[control.ColumnName] = control.Value;
                            }
                            dt.Rows.Add(row);
                        });
                        var i1 = i;
                        ProgressBar.Dispatcher.Invoke(() => { ProgressBar.Value = (int)(i1 / (float)numberOfRecords * 50); });
                    }
                    var connectionString = ConnectionString + $"Database = {_database};";
                    using (var sqlBulk = new SqlBulkCopy(connectionString))
                    {
                        sqlBulk.NotifyAfter = 10;
                        var i1 = 0;
                        foreach (DataColumn dataColumn in dt.Columns)
                        {
                            var columnName = dataColumn.ColumnName;
                            sqlBulk.ColumnMappings.Add(new SqlBulkCopyColumnMapping(columnName, columnName));

                        }
                        sqlBulk.SqlRowsCopied += (sender, eventArgs) =>
                        {
                            i1++;
                            ProgressBar.Dispatcher.Invoke(() => { ProgressBar.Value = 50 + (int)(i1 / (float)numberOfRecords * 50 * sqlBulk.NotifyAfter); });
                        };
                        sqlBulk.DestinationTableName = _table;
                        sqlBulk.WriteToServer(dt);
                    }

                    ProgressBar.Dispatcher.Invoke(() => { ProgressBar.Value = 100; });
                });
                MessageBox.Show(@"Done");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                BottomGrid.IsEnabled = true;
                ProgressBar.Value = 0;
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            RunSqlCommands();
        }
    }

    public class ForeignKeyModel{
        public string ReferencingColumnName { get; set; }
        public string ConnectionString { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }

    }
}
