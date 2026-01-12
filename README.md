<h2>Database scaffolding:</h2>

````
dotnet ef dbcontext scaffold "Host=localhost:5433;Database=germa;Username=germa;Password=germa" Npgsql.EntityFrameworkCore.PostgreSQL --no-onconfiguring --output-dir DataModel/Database --force --context DataContext --namespace GERMAG.DataModel.Database --startup-project Server --project Shared
````
<h2>Http Staus Codes:</h2>

| Code         | Definition |
|-------------|-----|
| 429 | To many Requests |
| 512 | Requested location is outside the covered area|
| 513 | Not Enough space is given to place BHE in the given requested area |
| 514 | The selected locations are not connected by a landparcel |
| 515 | Could not connect to Server |
| 516 | To much requests for the short-report. Rate is limited |
| 517 | To much requests for the full-report. Rate is limited |
| 518 | To much requests for the User assignment. Rate is limited |