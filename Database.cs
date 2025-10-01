using Npgsql;
using Dapper;

class Note
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string[] Tags { get; set; }
    public DateTime CreatedAt { get; set; }
}

class Database
{
    private string ConnectionString;

    public Database()
    {
        // 環境変数で接続文字列を取得
        var localConn = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? localConn;

        if (string.IsNullOrEmpty(ConnectionString))
            throw new Exception("DB接続文字列が設定されていません");
    }

    public void AddNote(string title, string content, string[] tags)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();
        conn.Execute(
            "INSERT INTO Notes (Title, Content, Tags) VALUES (@Title, @Content, @Tags)",
            new { Title = title, Content = content, Tags = tags }
        );
    }

    public IEnumerable<Note> SearchNotes(string keyword)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();
        return conn.Query<Note>(
            "SELECT * FROM Notes WHERE Content ILIKE @kw OR Title ILIKE @kw",
            new { kw = $"%{keyword}%" }
        );
    }
}
