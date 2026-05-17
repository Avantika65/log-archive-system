using Microsoft.Data.Sqlite;

namespace myCSharpApp;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        var dbPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "logs.db");

        _connectionString =
            $"Data Source={dbPath}";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        @"
            CREATE TABLE IF NOT EXISTS ProcessedFiles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FileName TEXT NOT NULL,
                SourcePath TEXT NOT NULL,
                ArchivePath TEXT NOT NULL,
                Hash TEXT NOT NULL,
                ProcessedAt TEXT NOT NULL
            );
        ";

        command.ExecuteNonQuery();
    }

    public bool FileHashExists(string hash)
    {
        using var connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        @"
            SELECT COUNT(*)
            FROM ProcessedFiles
            WHERE Hash = $hash
        ";

        command.Parameters.AddWithValue(
            "$hash",
            hash);

        var count =
            Convert.ToInt32(command.ExecuteScalar());

        return count > 0;
    }

    public void SaveProcessedFile(
        string fileName,
        string sourcePath,
        string archivePath,
        string hash)
    {
        using var connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        @"
            INSERT INTO ProcessedFiles (
                FileName,
                SourcePath,
                ArchivePath,
                Hash,
                ProcessedAt
            )
            VALUES (
                $fileName,
                $sourcePath,
                $archivePath,
                $hash,
                $processedAt
            )
        ";

        command.Parameters.AddWithValue(
            "$fileName",
            fileName);

        command.Parameters.AddWithValue(
            "$sourcePath",
            sourcePath);

        command.Parameters.AddWithValue(
            "$archivePath",
            archivePath);

        command.Parameters.AddWithValue(
            "$hash",
            hash);

        command.Parameters.AddWithValue(
            "$processedAt",
            DateTime.UtcNow.ToString("O"));

        command.ExecuteNonQuery();
    }
}