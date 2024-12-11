using System.Data.SQLite;
using System.Collections.ObjectModel;
using SQLiteBrowser;

namespace SQLiteBrowser;

public partial class TableDetailPage : ContentPage
{
    private readonly string connectionString;
    private readonly string tableName;
    private ObservableCollection<Dictionary<string, object>> rows;
    private List<string> columnNames;
    private Grid collectionView;

    public TableDetailPage(string _connectionString, string _tableName)
    {
        this.connectionString = _connectionString;
        this.tableName = _tableName;

        Title = this.tableName;

        this.collectionView = new Grid();

        SetData();

        Content = new StackLayout
        {
            Padding = new Thickness(10),
            Children =
            {
                collectionView,
                new Button
                {
                    Text = "Add New Row",
                    Command = new Command(OnAddNewRowClicked)
                }
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetData();
    }

    private void SetData()
    {
        collectionView.Children.Clear();
        LoadTableData();

        collectionView.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        foreach (var columnName in this.columnNames)
        {
            collectionView.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var label = new Label { Text = columnName, FontAttributes = FontAttributes.Bold };
            collectionView.SetColumn(label, this.columnNames.IndexOf(columnName));
            collectionView.SetRow(label, 0);
            collectionView.Children.Add(label);
        }
        for (int i = 0; i < this.rows.Count; i++)
        {
            collectionView.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int j = 0; j < this.columnNames.Count; j++)
            {
                if (collectionView.ColumnDefinitions.Count < this.columnNames.Count)
                {
                    collectionView.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }

                string columnName = this.columnNames[j];
                string value = this.rows[i][columnName].ToString();
                var label = new Label();
                label.Text = value;

                Grid.SetColumn(label, j);
                Grid.SetRow(label, i + 1);
                collectionView.Children.Add(label);

                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.CommandParameter = this.rows[i];
                tapGestureRecognizer.Tapped += OnRowTapped;
                label.GestureRecognizers.Add(tapGestureRecognizer);
            }
        }
    }

    private void RefreshData()
    {
        collectionView.Children.Clear();
        LoadTableData();
        collectionView.Children.Clear();
    }

    private async void OnRowTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Dictionary<string, object> row)
        {
            await Navigation.PushAsync(new EditRowPage(this.connectionString, this.tableName, row));
        }
    }

    private async void OnAddNewRowClicked(object obj)
    {
        var emptyRow = this.columnNames.ToDictionary(col => col, col => (object)null);
        await Navigation.PushAsync(new EditRowPage(this.connectionString, this.tableName, emptyRow, true));
    }

    private void LoadTableData()
    {
        this.rows = new ObservableCollection<Dictionary<string, object>>();

        using var connection = new SQLiteConnection(this.connectionString);
        connection.Open();
        string query = $"SELECT * FROM {this.tableName}";

        using var command = new SQLiteCommand(query, connection);
        using var reader = command.ExecuteReader();
        // Get raw results
        while (reader.Read())
        {
            Dictionary<string, object> rowData = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.WriteLine($"{reader.GetName(i)}: {reader.GetValue(i)}");
                rowData[reader.GetName(i)] = reader.GetValue(i);
            }
            this.rows.Add(rowData);
        }

        // Get column names
        this.columnNames = new List<string>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            this.columnNames.Add(reader.GetName(i));
        }

        connection.Close();
    }
}
