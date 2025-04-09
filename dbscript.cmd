dotnet-ef database drop -f -c IssuesDbContext -p .\IssueService\src\Issues\SachkovTech.Issues.Infrastructure\ -s .\IssueService\src\SachkovTech.Web\

dotnet-ef migrations remove -c IssuesDbContext -p .\IssueService\src\Issues\SachkovTech.Issues.Infrastructure\ -s .\IssueService\src\SachkovTech.Web\

dotnet-ef migrations add Issues_init -c IssuesDbContext -p .\IssueService\src\Issues\SachkovTech.Issues.Infrastructure\ -s .\IssueService\src\SachkovTech.Web\

dotnet-ef database update -c IssuesDbContext -p .\IssueService\src\Issues\SachkovTech.Issues.Infrastructure\ -s .\IssueService\src\SachkovTech.Web\


dotnet-ef database drop -f -c AccountsDbContext -p .\AccountService\src\AccountService.Infrastructure\ -s .\AccountService\src\AccountService.Api\

dotnet-ef migrations remove -c AccountsDbContext -p .\AccountService\src\AccountService.Infrastructure\ -s .\AccountService\src\AccountService.Api\

dotnet-ef migrations add Accounts_init -c AccountsDbContext -p .\AccountService\src\AccountService.Infrastructure\ -s .\AccountService\src\AccountService.Api\

dotnet-ef database update -c AccountsDbContext -p .\AccountService\src\AccountService.Infrastructure\ -s .\AccountService\src\AccountService.Api\


pause