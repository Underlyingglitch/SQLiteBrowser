namespace SQLiteBrowser;

public partial class EditRowPage : ContentPage
{
    private readonly string dbPath;
    private readonly string tableName;
    private Dictionary<string, object> row;
    private bool isNew;
    private Dictionary<string, string> originalContent;

    public EditRowPage(string _dbPath, string _tableName, Dictionary<string, object> _row, bool _isNew = false)
    {
        this.dbPath = _dbPath;
        this.tableName = _tableName;
        this.row = _row;
        this.isNew = _isNew;
        this.originalContent = new Dictionary<string, string>();

        Title = _isNew ? "New Row" : "Edit Row";

        var collectionView = new StackLayout();
        foreach (var column in this.row)
        {
            var label = new Label { Text = column.Key, FontAttributes = FontAttributes.Bold };
            collectionView.Children.Add(label);
            var entry = new Entry { Placeholder = column.Key };
            if (!_isNew)
            {
                string text = column.Value?.ToString();
                if (text != null && text.Length > 5000)
                {
                    DisplayAlert("Warning", $"The content of '{column.Key}' exceeds 5000 characters and has been truncated.", "OK");
                    this.originalContent[column.Key] = text; // Store the original content
                    text = text.Substring(0, 5000);
                }
                entry.Text = text;
            }

            collectionView.Children.Add(entry);
        }
        collectionView.Children.Add(new Button { Text = "Save", Command = new Command(OnSaveClicked), Margin = new Thickness(0, 10, 0, 0) });
        if (!_isNew) collectionView.Children.Add(new Button { Text = "Delete", Command = new Command(OnDeleteClicked), Margin = new Thickness(0, 10, 0, 0), BackgroundColor = Color.FromRgb(255, 0, 0) });
        collectionView.Children.Add(new Button { Text = "Cancel", Command = new Command(OnCancelClicked), Margin = new Thickness(0, 10, 0, 0), BackgroundColor = Color.FromRgb(150, 150, 150) });

        Content = collectionView;
    }

    private async void OnSaveClicked(object obj)
    {
        if (this.isNew) InsertRecord();
        else UpdateRecord();
    }

    public async void InsertRecord()
    {
        try
        {
            Dictionary<string, object> newRow = new Dictionary<string, object>();
            foreach (var child in (Content as StackLayout).Children)
            {
                if (child is Entry entry)
                {
                    newRow[entry.Placeholder] = string.IsNullOrEmpty(entry.Text) ? null : entry.Text;
                }
            }

            SQLiteWrapper.Insert(this.dbPath, this.tableName, newRow);

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public async void UpdateRecord()
    {
        try
        {
            Dictionary<string, object> newRow = new Dictionary<string, object>();
            foreach (var child in (Content as StackLayout).Children)
            {
                if (child is Entry entry)
                {
                    if (this.originalContent.ContainsKey(entry.Placeholder) && entry.Text == this.originalContent[entry.Placeholder].Substring(0, 5000))
                    {
                        newRow[entry.Placeholder] = this.originalContent[entry.Placeholder]; // Use the original content if not modified
                    }
                    else
                    {
                        newRow[entry.Placeholder] = string.IsNullOrEmpty(entry.Text) ? null : entry.Text;
                    }
                }
            }
            SQLiteWrapper.Update(this.dbPath, this.tableName, newRow, this.row);

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnDeleteClicked(object obj)
    {
        if (!await DisplayAlert("Cancel", "Are you sure you want to delete?", "Yes", "No")) return;
        try
        {
            SQLiteWrapper.Delete(this.dbPath, this.tableName, this.row);

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCancelClicked(object obj)
    {
        await Navigation.PopAsync();
    }
}
