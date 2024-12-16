using System.Collections.ObjectModel;

namespace SQLiteBrowser;

public partial class TableDetailPage : ContentPage
{
    private readonly string dbPath;
    private readonly string tableName;
    private ObservableCollection<Dictionary<string, object>> rows;
    private List<string> columnNames;
    private Grid collectionView;

    private bool hasRows;
    public bool HasRows
    {
        get => hasRows;
        set
        {
            if (hasRows != value)
            {
                hasRows = value;
                OnPropertyChanged();
            }
        }
    }

    public TableDetailPage(string _dbPath, string _tableName)
    {
        this.dbPath = _dbPath;
        this.tableName = _tableName;

        Title = this.tableName;

        this.collectionView = new Grid();

        SetData();

        Content = new StackLayout
        {
            Padding = new Thickness(10),
            Children =
            {
                new ScrollView
                {
                    Orientation = ScrollOrientation.Both, // Enable both horizontal and vertical scrolling
                    Content = collectionView
                },
                new Button
                {
                    Text = "Add New Row",
                    Command = new Command(OnAddNewRowClicked),
                    IsVisible = HasRows,
                    BindingContext = this
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
        collectionView.RowDefinitions.Clear();
        collectionView.ColumnDefinitions.Clear();
        LoadTableData();

        collectionView.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        foreach (var columnName in this.columnNames)
        {
            collectionView.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var label = new Label { Text = columnName, FontAttributes = FontAttributes.Bold, Padding = new Thickness(5) };
            var frame = new Frame
            {
                Content = label,
                BorderColor = Colors.Black,
                Padding = new Thickness(2),
                CornerRadius = 0
            };
            collectionView.SetColumn(frame, this.columnNames.IndexOf(columnName));
            collectionView.SetRow(frame, 0);
            collectionView.Children.Add(frame);
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
                var label = new Label { Text = value, Padding = new Thickness(5) };
                var frame = new Frame
                {
                    Content = label,
                    BorderColor = Colors.Black,
                    Padding = new Thickness(2),
                    CornerRadius = 0
                };

                Grid.SetColumn(frame, j);
                Grid.SetRow(frame, i + 1);
                collectionView.Children.Add(frame);

                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.CommandParameter = this.rows[i];
                tapGestureRecognizer.Tapped += OnRowTapped;
                label.GestureRecognizers.Add(tapGestureRecognizer);
            }
        }
    }

    private async void OnRowTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Dictionary<string, object> row)
        {
            await Navigation.PushAsync(new EditRowPage(this.dbPath, this.tableName, row));
        }
    }

    private async void OnAddNewRowClicked(object obj)
    {
        if (this.columnNames.Count < 1)
        {
            await DisplayAlert("Error", "No known columns. Make sure at least 1 row is present in the database", "OK");
            return;
        }
        var emptyRow = this.columnNames.ToDictionary(col => col, col => (object)null);
        await Navigation.PushAsync(new EditRowPage(this.dbPath, this.tableName, emptyRow, true));
    }

    private void LoadTableData()
    {
        this.rows = new ObservableCollection<Dictionary<string, object>>();

        List<Dictionary<string, object>> result = SQLiteWrapper.SelectAll(this.dbPath, this.tableName);

        this.rows = new ObservableCollection<Dictionary<string, object>>(result);

        if (this.rows.Count > 0)
        {
            this.columnNames = this.rows[0].Keys.ToList();
            this.HasRows = true;
        }
        else
        {
            this.columnNames = new List<string>();
            this.HasRows = false;
        }
    }
}



