using System.Data.SQLite;

namespace SQLiteBrowser;

public partial class EditRowPage : ContentPage
{
    private readonly string connectionString;
    private readonly string tableName;
    private Dictionary<string, object> row;
    private bool isNew;

    public EditRowPage(string _connectionString, string _tableName, Dictionary<string, object> _row, bool _isNew = false)
    {
        this.connectionString = _connectionString;
        this.tableName = _tableName;
        this.row = _row;
        this.isNew = _isNew;

        Title = _isNew ? "New Row" : "Edit Row";

        var collectionView = new StackLayout();
        foreach (var column in this.row)
        {
            var label = new Label { Text = column.Key, FontAttributes = FontAttributes.Bold };
            collectionView.Children.Add(label);
            var entry = new Entry { Placeholder = column.Key };
            if (!_isNew) entry.Text = column.Value.ToString();
            collectionView.Children.Add(entry);
        }
        collectionView.Children.Add(new Button { Text = "Save", Command = new Command(OnSaveClicked), Padding = 10 });
        if (!_isNew) collectionView.Children.Add(new Button { Text = "Delete", Command = new Command(OnDeleteClicked), BackgroundColor = Color.FromRgb(255, 0, 0), Padding = 10 });
        collectionView.Children.Add(new Button { Text = "Cancel", Command = new Command(OnCancelClicked), BackgroundColor = Color.FromRgb(150, 150, 150), Padding = 10 });

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
            // Save the row to the database
            using var connection = new SQLiteConnection(this.connectionString);
            connection.Open();

            string query = $"INSERT INTO {this.tableName}  ";
            string columns = "(";
            string values = "VALUES (";
            foreach (var column in newRow)
            {
                columns += $"{column.Key}, ";
                values += column.Value is null ? "null, " : $"'{column.Value}', ";
            }
            columns = columns.Substring(0, columns.Length - 2); // Remove the last comma
            values = values.Substring(0, values.Length - 2); // Remove the last comma
            columns += ")";
            values += ")";
            query += columns + " " + values;

            using var command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();

            connection.Close();

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
                    newRow[entry.Placeholder] = string.IsNullOrEmpty(entry.Text) ? null : entry.Text;
                }
            }
            // Save the row to the database
            using var connection = new SQLiteConnection(this.connectionString);
            connection.Open();

            string query = $"UPDATE {this.tableName} SET ";
            foreach (var column in newRow)
            {
                query += $"{column.Key} = ";
                query += column.Value is null ? "null, " : $"'{column.Value}', ";
            }
            query = query.Substring(0, query.Length - 2); // Remove the last comma
                                                          // Use all the keys to find the row to update
            query += " WHERE ";
            foreach (var column in this.row)
            {
                query += $"{column.Key} = ";
                query += column.Value is null ? "null" : $"'{column.Value}'";
                query += " AND ";
            }
            query = query.Substring(0, query.Length - 5); // Remove the last AND

            using var command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();

            connection.Close();

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
            using var connection = new SQLiteConnection(this.connectionString);
            connection.Open();

            string query = $"DELETE FROM {this.tableName} WHERE ";
            foreach (var column in this.row)
            {
                query += $"{column.Key} = '{column.Value}' AND ";
            }
            query = query.Substring(0, query.Length - 5); // Remove the last AND

            using var command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();

            connection.Close();

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
