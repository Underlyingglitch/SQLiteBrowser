using System.Collections.ObjectModel;

namespace SQLiteBrowser;

public partial class DatabaseBrowserPage : ContentPage
{
    private string dbPath;
    private ObservableCollection<string> tableNames;
    private ListView tablesView;

    public DatabaseBrowserPage(string _filePath)
    {
        this.tableNames = new ObservableCollection<string>();
        this.dbPath = _filePath;
        
        this.tablesView = new ListView {
            ItemsSource = this.tableNames,
            ItemTemplate = new DataTemplate(() =>
            {
                var textCell = new TextCell();
                textCell.SetBinding(TextCell.TextProperty, ".");
                return textCell;
            })
        };
        this.tablesView.ItemTapped += OnTableSelected;

        Title = "Tables";

        Content = new StackLayout
        {
            Padding = new Thickness(10),
            Children =
            {
                this.tablesView,
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.LoadDatabase();
    }

    public void LoadDatabase()
    {
        this.tableNames.Clear();

        var tableNamesFromDb = SQLiteWrapper.GetTableNames(this.dbPath);
        foreach (var tableName in tableNamesFromDb)
        {
            this.tableNames.Add(tableName);
        }
    }

    private async void OnTableSelected(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is string tableName)
        {
            var tablePage = new TableDetailPage(this.dbPath, tableName);
            await Navigation.PushAsync(tablePage);
        }
    }

}
