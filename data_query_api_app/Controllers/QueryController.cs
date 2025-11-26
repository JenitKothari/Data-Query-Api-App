using data_query_api_app.Models;
using data_query_api_app.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Primitives;

namespace data_query_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly SqliteService _sqliteService;
        private readonly NlqToSqlConverterService _nqlConverterService;
        public QueryController(SqliteService service, NlqToSqlConverterService nlqConverterService) { 
            _sqliteService = service;
            _nqlConverterService = nlqConverterService;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllResults()
        //{
        //    var queryResut = await _sqliteService.ExecuteQuery("select * from Player_Attributes LIMIT 100");
        //    return Ok(queryResut);
        //}

        [HttpPost("query")]
        public async Task<IActionResult> PostQuery([FromBody] string question)
        {
            string generatedSql = null;
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));

            try
            {
                generatedSql = await _nqlConverterService.ConvertNLToSql(question, cts.Token);
                Console.WriteLine($"Generated SQL: {generatedSql}");
                if (string.IsNullOrEmpty(generatedSql))
                {
                    return Ok(new
                    {
                        Question = question,
                        Result = "INVALID QUESTION, PLEASE ASK VALID QUESTION."
                    }
                        );
                }

                QueryResult results = await _sqliteService.ExecuteQuery(generatedSql,cts.Token);

                var response = new
                {
                    Question = question,
                    GeneratedSql = generatedSql,
                    Results = results.Rows,
                    ExecutionTime = results.ExecutionTime
                };
                return Ok(response);
            }
            catch (SqliteException ex)
            {
                return StatusCode(500, new
                {
                    Error = "Database Query Error: The generated SQL was invalid or caused an execution failure.",
                    Details = $"SQLite Message: {ex.Message}",
                    GeneratedSql = generatedSql
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(500, new
                {
                    Error = "Query Timeout Error.",
                    Details = "The database query exceeded the maximum allowed execution time.",
                    GeneratedSql = generatedSql
                });
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new
                {
                    Error = "An unexpected error occurred during AI processing.",
                    Details = ex.Message,
                    GeneratedSql = generatedSql
                });
            }
            finally
            {
                cts.Dispose();
            }
        }
    }
}
