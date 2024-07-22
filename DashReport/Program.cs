using Microsoft.Data.Sqlite;
using System.Text;

if (args.Length < 2)
{
    Console.WriteLine("Usage: --connection <connection_string> --query <query_file>");
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
    else if (args[i] == "--query")
    {
        queryFilePath = args[++i];
    }
}

if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(queryFilePath))
{
    Console.WriteLine("Both --connection and --query arguments are required.");
    return;
}

string query = File.ReadAllText(queryFilePath);

using var connection = new SqliteConnection(connectionString);
connection.Open();

using var command = new SqliteCommand(query, connection);
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

Console.WriteLine("HTML report generated as output.html");
