using SQLitePCL;

namespace SQLiteBrowser;

internal class SQLiteWrapper
{
    public static List<string> GetTableNames(string _dbPath)
    {
        Batteries.Init();
        sqlite3 db;
        int result = raw.sqlite3_open(_dbPath, out db);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception("Could not open database");
        }

        sqlite3_stmt stmt;
        string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
        result = raw.sqlite3_prepare_v2(db, sql, out stmt);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception($"Failed to prepare SQL statement: {raw.sqlite3_errmsg(db).utf8_to_string()}");
        }

        List<string> tableNames = new List<string>();

        while ((result = raw.sqlite3_step(stmt)) == raw.SQLITE_ROW)
        {
            tableNames.Add(raw.sqlite3_column_text(stmt, 0).utf8_to_string());
        }

        raw.sqlite3_finalize(stmt);
        raw.sqlite3_close(db);

        return tableNames;
    }

    public static List<Dictionary<string, object>> SelectAll(string _dbPath, string _tableName)
    {
        Batteries.Init();
        int result = raw.sqlite3_open(_dbPath, out sqlite3 db);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception("Could not open database");
        }

        sqlite3_stmt stmt;
        string sql = $"SELECT * FROM {_tableName}";
        result = raw.sqlite3_prepare_v2(db, sql, out stmt);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception($"Failed to prepare SQL statement: {raw.sqlite3_errmsg(db).utf8_to_string()}");
        }

        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

        while ((result = raw.sqlite3_step(stmt)) == raw.SQLITE_ROW)
        {
            Dictionary<string, object> row = new Dictionary<string, object>();
            for (int i = 0; i < raw.sqlite3_column_count(stmt); i++)
            {
                string columnName = raw.sqlite3_column_name(stmt, i).utf8_to_string();
                string columnType = raw.sqlite3_column_decltype(stmt, i).utf8_to_string();
                object columnValue = null;
                switch (columnType)
                {
                    case "INTEGER":
                        columnValue = raw.sqlite3_column_int(stmt, i);
                        break;
                    case "TEXT":
                        columnValue = raw.sqlite3_column_text(stmt, i).utf8_to_string();
                        break;
                    case "REAL":
                        columnValue = raw.sqlite3_column_double(stmt, i);
                        break;
                    case "BLOB":
                        columnValue = raw.sqlite3_column_blob(stmt, i).ToArray();
                        break;
                }
                row.Add(columnName, columnValue);
            }
            rows.Add(row);
        }

        raw.sqlite3_finalize(stmt);
        raw.sqlite3_close(db);

        return rows;
    }

    public static void Insert(string _dbPath, string _tableName, Dictionary<string, object> _values)
    {
        Batteries.Init();
        int result = raw.sqlite3_open(_dbPath, out sqlite3 db);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception("Could not open database");
        }
        string sql = GetInsertQuery(_tableName, _values);
        result = raw.sqlite3_exec(db, sql);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception($"Failed to execute SQL statement: {raw.sqlite3_errmsg(db).utf8_to_string()}");
        }
        raw.sqlite3_close(db);
    }

    public static void Update(string _dbPath, string _tableName, Dictionary<string, object> _values, Dictionary<string, object> _oldValues)
    {
        Batteries.Init();
        int result = raw.sqlite3_open(_dbPath, out sqlite3 db);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception("Could not open database");
        }
        string sql = GetUpdateQuery(_tableName, _oldValues, _values);

        result = raw.sqlite3_exec(db, sql);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception($"Failed to execute SQL statement: {raw.sqlite3_errmsg(db).utf8_to_string()}");
        }
        raw.sqlite3_close(db);
    }

    public static void Delete(string _dbPath, string _tableName, Dictionary<string, object> _values)
    {
        Batteries.Init();
        int result = raw.sqlite3_open(_dbPath, out sqlite3 db);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception("Could not open database");
        }
        string sql = GetDeleteQuery(_tableName, _values);
        result = raw.sqlite3_exec(db, sql);
        if (result != raw.SQLITE_OK)
        {
            throw new Exception($"Failed to execute SQL statement: {raw.sqlite3_errmsg(db).utf8_to_string()}");
        }
        raw.sqlite3_close(db);
    }

    public static string GetInsertQuery(string _tableName, Dictionary<string, object> _row)
    {
        string query = $"INSERT INTO {_tableName}  ";
        string columns = "(";
        string values = "VALUES (";
        foreach (var column in _row)
        {
            columns += $"{column.Key}, ";
            values += column.Value is null ? "null, " : $"'{column.Value}', ";
        }
        columns = columns.Substring(0, columns.Length - 2); // Remove the last comma
        values = values.Substring(0, values.Length - 2); // Remove the last comma
        columns += ")";
        values += ")";
        query += columns + " " + values;

        return query;
    }

    public static string GetUpdateQuery(string _tableName, Dictionary<string, object> _oldValues, Dictionary<string, object> _newValues)
    {
        string query = $"UPDATE {_tableName} SET ";
        foreach (var column in _newValues)
        {
            query += $"{column.Key} = ";
            query += column.Value is null ? "null, " : $"'{column.Value}', ";
        }
        query = query.Substring(0, query.Length - 2); // Remove the last comma
                                                      // Use all the keys to find the row to update
        query += " WHERE ";
        foreach (var column in _oldValues)
        {
            query += $"{column.Key} = ";
            query += column.Value is null ? "null" : $"'{column.Value}'";
            query += " AND ";
        }
        query = query.Substring(0, query.Length - 5); // Remove the last AND
        return query;
    }

    public static string GetDeleteQuery(string _tableName, Dictionary<string, object> _values)
    {
        string query = $"DELETE FROM {_tableName} WHERE ";
        foreach (var column in _values)
        {
            query += $"{column.Key} = ";
            query += column.Value is null ? "null" : $"'{column.Value}'";
            query += " AND ";
        }
        query = query.Substring(0, query.Length - 5); // Remove the last AND
        return query;
    }
}
