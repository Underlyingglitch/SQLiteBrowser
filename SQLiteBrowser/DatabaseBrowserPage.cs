using System.Collections.ObjectModel;
using System.Diagnostics;

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

        this.tablesView = new ListView
        {
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

        // If the app that includes this package is running in Release mode, show a warning message
        if (!Debugger.IsAttached)
        {
            var warningLabel = new Label
            {
                Text = "Warning: This package is intended for development purposes only. Do not use in production.",
                TextColor = Color.FromRgb(255, 0, 0),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 10, 0, 0)
            };
            this.Content = new StackLayout
            {
                Padding = new Thickness(10),
                Children =
                {
                    warningLabel
                }
            };
            return;
        }

        Content = new StackLayout
        {
            Padding = new Thickness(10),
            Children =
            {
                this.tablesView,
                new Button
                {
                    Text = "Clean all tables (TRUNCATE)",
                    Command = new Command(OnCleanTablesBtnClicked),
                    BackgroundColor = Color.FromRgb(255, 204, 0),
                    TextColor = Color.FromRgb(0, 0, 0),
                    Margin = new Thickness(0, 10, 0, 0)
                },
                new Button
                {
                    Text = "Delete all tables (DROP)",
                    Command = new Command(OnDropTablesBtnClicked),
                    BackgroundColor = Color.FromRgb(255, 0, 0),
                    Margin = new Thickness(0, 10, 0, 0)
                }
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

    private async void OnCleanTablesBtnClicked(object obj)
    {
        if (!await DisplayAlert("Confirm", "Are you sure you want to TRUNCATE?", "Yes", "No")) return;
        foreach (var tableName in this.tableNames)
        {
            try
            {
                SQLiteWrapper.Truncate(this.dbPath, tableName);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnDropTablesBtnClicked(object obj)
    {
        if (!await DisplayAlert("Confirm", "Are you sure you want to DROP?", "Yes", "No")) return;
        foreach (var tableName in this.tableNames)
        {
            try
            {
                SQLiteWrapper.Drop(this.dbPath, tableName);

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

}
