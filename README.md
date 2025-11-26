#### DATA QUERY API APP



This project converts natural language questions (NLQ) into SQLite SQL queries using Google Gemini + Semantic Kernel, executes the SQL, and returns structured results.





##### Project Goal



Allow users to type a natural English question (e.g., “Show top 10 players by rating”) and automatically:

Convert NL → SQL

2\. Run the SQL on a local SQLite database

3\. Return results + execution time





##### Tech Stack



1. .NET 8 Web API

2\. Angular (frontend)

3\. Google Gemini 2.5 Flash (via Semantic Kernel)

4\. SQLite (Soccer database)





##### Setup Instructions



1\. Configure SQLite Database:-



In appsettings.json:

"ConnectionStrings": {

&nbsp; "SoccerDbConnection": "Data Source=YOUR\_DB\_PATH/soccer.db"

}



2\. Add Gemini API Key:-



In appsettings.json:

"Gemini": {

&nbsp; "ApiKey": "YOUR\_API\_KEY"

}





##### API Endpoint



https://localhost:7203/api/Query/query





##### Program Flow (How It Works)



User enters a natural language question → Angular UI sends it to API.

2\. API calls Gemini via Semantic Kernel with a strict prompt containing the DB schema.

3\. Gemini returns a pure SQL query.

4\. The SQL is executed using SqliteService.

5\. Results are formatted into:

&nbsp;   -Generated SQL

&nbsp;   -Rows

&nbsp;   -Execution time

&nbsp;   -Angular UI displays the response.

