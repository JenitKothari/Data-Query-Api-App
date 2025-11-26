namespace data_query_api_app.Services
{
    using Microsoft.SemanticKernel;
    using Microsoft.Extensions.Configuration;
    using Microsoft.SemanticKernel.Connectors.Google;

    public class NlqToSqlConverterService
    {
        private readonly Kernel _kernel;
        private readonly string _systemPrompt;
        private const string GeminiModel = "gemini-2.5-flash";

        public NlqToSqlConverterService(IConfiguration configuration)
        {
            // 1. Read the Gemini API Key
            var apiKey = configuration.GetSection("Gemini")["ApiKey"]
                         ?? throw new InvalidOperationException("Gemini:ApiKey not found in configuration.");

            // 2. Build the Semantic Kernel instance using the Gemini Connector
            var builder = Kernel.CreateBuilder();

            // Use the dedicated extension method for Gemini
            builder.AddGoogleAIGeminiChatCompletion(
                modelId: GeminiModel,
                apiKey: apiKey);

            _kernel = builder.Build();

            // 3. Define the System Prompt
            _systemPrompt = CreateSystemPrompt();
        }

        private string CreateSystemPrompt()
        {
            // Define the SCHEMA
            string schema = @"
SQLite Database Schema:
Table: Match (match_api_id, date, home_team_api_id, away_team_api_id, home_team_goal, away_team_goal)
Table: Team (team_api_id, team_long_name, team_short_name)
Table: Player (player_api_id, player_name, birthday)
Table: Player_Attributes (player_api_id, date, overall_rating, potential, sprint_speed)";

            // Define the RULES
            string rules = @"
RULES:
1. Only generate SELECT queries.
2. Use proper JOIN statements when needed.
3. Return ONLY the SQL query, no explanations, markdown, or text.
4. Use SQLite syntax (e.g., use strftime('%Y', date) for year extraction, use MAX() or MIN() for ordering).
5. When filtering by team name, use the 'Team' table's 'team_long_name' column.
";

            return "You are a database query generator. Your task is to convert a natural language question into a single, syntactically correct SQLite SQL query based on the following schema and rules:\n"
                   + schema
                   + rules;
        }


        public async Task<string> ConvertNLToSql(string naturalQuestion, CancellationToken cancellationToken = default)
        {
            // Concatenate the system prompt with the user's specific question
            string prompt = $"{_systemPrompt}\n\nQuestion: {naturalQuestion}\n\nSQL Query:";

            // Invoke the model
            var result = await _kernel.InvokePromptAsync(prompt,cancellationToken: cancellationToken);

            // Clean up the result (LLMs sometimes add newlines or markdown)
            return result.GetValue<string>()?.Trim().Replace("```sql", "").Replace("```", "").Trim() ?? string.Empty;
        }
    }
}
