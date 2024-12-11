using System.Data.SQLite;
using System.Collections.ObjectModel;

namespace SQLiteBrowser;

public partial class DatabaseBrowserPage : ContentPage
{
    private string connectionString;
    private ObservableCollection<string> tableNames;
    private Grid tableGrid;

    public DatabaseBrowserPage(string _filePath)
    {
        this.tableNames = new ObservableCollection<string>();
        this.connectionString = $"Data Source={_filePath}";

        this.tableGrid = new Grid();

        this.LoadDatabase();

        Title = "Tables";

        Content = new StackLayout
        {
            Padding = new Thickness(10),
            Children =
            {
                this.tableGrid,
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.LoadDatabase();
    }

    private void SetTables()
    {
        this.tableGrid.Children.Clear();

        foreach (var tableName in this.tableNames)
        {
            this.tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var label = new Label { Text = tableName, FontAttributes = FontAttributes.Bold };
            this.tableGrid.Children.Add(label);

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.CommandParameter = tableName;
            tapGestureRecognizer.Tapped += OnTableSelected;
            label.GestureRecognizers.Add(tapGestureRecognizer);
        }
    }

    public void LoadDatabase()
    {
        this.tableNames.Clear();

        using var connection = new SQLiteConnection(this.connectionString);
        connection.Open();

        string query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

        using var command = new SQLiteCommand(query, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            string tableName = reader.GetString(0);
            this.tableNames.Add(tableName);
        }

        SetTables();
    }

    private async void OnTableSelected(object? _sender, TappedEventArgs _e)
    {
        if (_e.Parameter is string tableName)
        {
            var tablePage = new TableDetailPage(this.connectionString, tableName);
            await Navigation.PushAsync(tablePage);
        }
    }
}
