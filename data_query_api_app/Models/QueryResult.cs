namespace data_query_api_app.Models
{
    public class QueryResult
    {
        public List<string> Columns { get; set; } = new List<string>();

        public List<Dictionary<string,object>> Rows { get; set; } = new List<Dictionary<string, object>>();

        public TimeSpan ExecutionTime {  get; set; }
    }
}
