using data_query_api_app.Models;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace data_query_api_app.Services
{
    public class SqliteService
    {
        private readonly string ConnectionString;
        public SqliteService(IConfiguration config) 
        {
            ConnectionString = config["ConnectionStrings:SoccerDbConnection"];
        }

        public async Task<QueryResult> ExecuteQuery(string sqlQuery, CancellationToken cancellationToken = default)
        {
            var result = new QueryResult();
            var stopwatch = Stopwatch.StartNew();

            using (var connection = new SqliteConnection(ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqliteCommand(sqlQuery, connection))
                {
                    command.CommandTimeout = 5;
                    // 3. Execution: Execute the query and read results
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        // Get column names
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            result.Columns.Add(reader.GetName(i));
                        }

                        // Read rows
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                // Map column name to value
                                // We use GetValue() and check for DBNull
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }
                            result.Rows.Add(row);
                        }
                    }
                }
            }
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            return result;
        }
    }
}
