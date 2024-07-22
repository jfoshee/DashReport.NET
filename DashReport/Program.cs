using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

if (args.Length < 4)
{
    Console.WriteLine("Usage: --connection <connection_string> --query-file <query_file>");
    return;
}

string connectionString = string.Empty;
string queryFilePath = string.Empty;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--connection")
    {
        connectionString = args[++i];
    }
    else if (args[i] == "--query-file")
    {
        queryFilePath = args[++i];
    }
}

if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(queryFilePath))
{
    Console.WriteLine("Both --connection and --query-file arguments are required.");
    return;
}

string query = File.ReadAllText(queryFilePath);

IDbConnection connection;
IDbCommand command;

if (connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("Filename=", StringComparison.OrdinalIgnoreCase))
{
    connection = new SqliteConnection(connectionString);
    command = new SqliteCommand(query, (SqliteConnection)connection);
}
else if (connectionString.StartsWith("Server=", StringComparison.OrdinalIgnoreCase) ||
         connectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
         connectionString.Contains("Integrated Security=", StringComparison.OrdinalIgnoreCase))
{
    connection = new SqlConnection(connectionString);
    command = new SqlCommand(query, (SqlConnection)connection);
}
else
{
    Console.WriteLine("Unsupported database type in the connection string.");
    return;
}

connection.Open();

using (connection)
using (command)
{
    using var reader = command.ExecuteReader();
    var htmlBuilder = new StringBuilder();

    htmlBuilder.AppendLine("<table>");
    htmlBuilder.AppendLine("  <thead>");
    htmlBuilder.AppendLine("    <tr>");

    for (int i = 0; i < reader.FieldCount; i++)
    {
        htmlBuilder.AppendLine($"      <th>{reader.GetName(i)}</th>");
    }

    htmlBuilder.AppendLine("    </tr>");
    htmlBuilder.AppendLine("  </thead>");
    htmlBuilder.AppendLine("  <tbody>");

    while (reader.Read())
    {
        htmlBuilder.AppendLine("    <tr>");
        for (int i = 0; i < reader.FieldCount; i++)
        {
            htmlBuilder.AppendLine($"      <td>{reader.GetValue(i)}</td>");
        }
        htmlBuilder.AppendLine("    </tr>");
    }

    htmlBuilder.AppendLine("  </tbody>");
    htmlBuilder.AppendLine("</table>");

    File.WriteAllText("output.html", htmlBuilder.ToString());
    Console.WriteLine("HTML report generated successfully.");
}
