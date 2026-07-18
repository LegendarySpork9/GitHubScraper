# GitHub Scraper - Infrastructure Document

## Overview

GitHub Scraper is a console application that gathers issue, commit, pull request, workflow run, and release data from GitHub repositories via the GitHub REST API and stores the results in a SQL Server database. It also generates daily issue aggregate statistics and logs execution summaries per repository.

- **Author:** Hunter Industries / Toby Hunter
- **Version:** 1.1.1
- **Repository:** https://github.com/LegendarySpork9/GitHubScraper

## Technology Stack

| Component | Technology | Version |
|---|---|---|
| Framework | .NET | 8.0 |
| Language | C# | Latest |
| Application Type | Console Application | - |
| Logging | log4net | 3.1.0 |
| HTTP Client | RestSharp | 112.1.0 |
| JSON Serialisation | Newtonsoft.Json | 13.0.3 |
| Database Client | Microsoft.Data.SqlClient | 6.1.1 |
| Database Client (Legacy) | System.Data.SqlClient | 4.9.0 |
| Testing | MSTest | 3.6.4 |
| Test SDK | Microsoft.NET.Test.Sdk | 17.12.0 |
| Mocking | Moq | 4.20.72 |
| Code Coverage | Microsoft.Testing.Extensions.CodeCoverage | 17.12.6 |
| Test Reporting | Microsoft.Testing.Extensions.TrxReport | 1.4.3 |

## Solution Structure

```
GitHubScraper/
+-- GitHubScraper/                      # Main console application
|   +-- Abstractions/                   # Interface definitions
|   +-- Content/                        # Static assets (favicon)
|   +-- Converters/                     # API query builders and label mapping
|   +-- Functions/                      # Issue aggregate and filtering logic
|   +-- Implementations/               # Interface implementations (wrappers)
|   +-- Models/                         # Data models
|   |   +-- Related/                    # Nested response models
|   +-- Services/                       # Business logic services
+-- GitHubScraper.Tests/                # Unit test project
|   +-- Converters/                     # Converter tests
|   +-- Functions/                      # Function tests
|   +-- Services/                       # Service tests
+-- .github/workflows/                  # CI/CD pipeline definitions
```

## Application Architecture

### Application Type

The application is a **.NET 8.0 console application** that runs as a one-shot process. It iterates through configured repositories, scrapes data from the GitHub API since the last run, stores results in SQL Server, and exits.

### Dependency Injection

External dependencies are wrapped behind interfaces to support testability. Services are instantiated manually in `ApplicationService.Run()`.

| Abstraction | Implementation | Purpose |
|---|---|---|
| `ILoggerService` | `LoggerServiceWrapper` | Application logging via log4net |
| `IFileSystem` | `FileSystemWrapper` | SQL file reading |
| `IClock` | `SystemClockProvider` | UTC time and default date (01/01/1900) operations |
| `IGitHubClient` | `GitHubClientWrapper` | GitHub REST API communication via RestSharp |
| `IGitHubOptions` | `GitHubOptionsProvider` | GitHub owner and bearer token access |
| `IDatabase` | `DatabaseWrapper` | SQL Server query execution via SqlClient |
| `IDatabaseOptions` | `DatabaseOptionsProvider` | Connection string and SQL files path access |

### Services

| Service | Responsibility |
|---|---|
| `ApplicationService` | Configuration validation and top-level orchestration |
| `GitHubService` | GitHub API data retrieval with filtering, normalisation, and date conversion |
| `DatabaseService` | SQL Server read/write operations using externalised SQL files |
| `LoggerService` | Internal log4net adapter |

### Functions

| Function | Responsibility |
|---|---|
| `DatabaseFunction` | Issue aggregate creation (daily created/solved counts) and issue deduplication filtering |

### Converters

| Converter | Responsibility |
|---|---|
| `GitHubConverter` | Maps API endpoints to query parameters; classifies issue labels to types |
| `StandardValues` | Log level constants (Debug, Error, Info, Warn) |

## Application Pipeline

### Execution Flow

For each configured repository:

1. **Get Last Run Date** — Query the database for when this repository was last scraped
2. **Get Existing Issues** — Query the database for issues already stored
3. **Scrape GitHub Data:**
   - Fetch issues updated since last run
   - Fetch all branches
   - For each branch, fetch commits since last run (deduplicated by SHA)
   - Fetch pull requests updated since last run
   - For each configured workflow, fetch workflow runs since last run
   - Fetch releases since last run
4. **Store Data** — Insert issues, commits, pull requests, workflow runs, and releases into SQL Server
5. **Create Aggregates** — Calculate daily issue created/solved counts, filling date gaps with zeroes
6. **Log Run** — Record execution summary (counts of each data type scraped)

### Data Normalisation

The `GitHubService` performs the following normalisation before database insertion:

| Field | Normalisation |
|---|---|
| Issue/PR Type | Derived from labels: `bug` → `Bug`, `enhancement` → `New Feature`, `documentation` → `Documentation` |
| State | Title-cased (e.g., `open` → `Open`) |
| Workflow Status/Conclusion | Underscores replaced with spaces, title-cased |
| Workflow Event | Underscores replaced with spaces, title-cased |
| All Dates | Converted to UTC |
| Pull Requests | Filtered out of issue results (GitHub API returns PRs in issue endpoints) |
| Commits | Deduplicated by SHA across branches, sorted by committer date |

## GitHub API Integration

- **Base URL:** `https://api.github.com`
- **Client:** RestSharp HTTP library
- **Authentication:** Bearer token (GitHub PAT)
- **Pagination:** All endpoints paginated with `per_page=100`

### Endpoints

| Data | Endpoint | Query Parameters |
|---|---|---|
| Issues | `/repos/{owner}/{repo}/issues` | `?state=all&sort=updated&per_page=100` |
| Branches | `/repos/{owner}/{repo}/branches` | `?per_page=100` |
| Commits | `/repos/{owner}/{repo}/commits` | `?per_page=100` (with `sha` and `since` parameters) |
| Pull Requests | `/repos/{owner}/{repo}/pulls` | `?state=all&sort=updated&direction=desc&per_page=100` |
| Workflow Runs | `/repos/{owner}/{repo}/actions/workflows/{workflow}/runs` | `?per_page=100` (with `created` date filter) |
| Releases | `/repos/{owner}/{repo}/releases` | `?per_page=100` |

## Data Models

### Primary Models

| Model | Key Properties |
|---|---|
| `IssueModel` | Id, Number, Title, Assignee, Type, State, Created_At, Closed_At, Labels |
| `CommitModel` | Repository, Sha, Commit (Author, Committer, Message) |
| `PullRequestModel` | Id, Number, Title, Assignee, Type, State, Created_At, Updated_At, Closed_At, Merged_At, Labels |
| `WorkflowModel` | Name, WorkflowRuns |
| `WorkflowRunModel` | Id, Run_Number, Actor, Name, Display_Title, Event, Status, Conclusion, Created_At, Updated_At |
| `ReleaseModel` | Id, Name, Author, Body, NumberOfAssets, Draft, Created_At, Updated_At, Published_At, Assets |
| `BranchModel` | Name |
| `IssueAggregateModel` | Date, Created, Solved |

### Related Models

| Model | Key Properties |
|---|---|
| `UserModel` | Login, Name, Date |
| `LabelModel` | Name |
| `RelatedCommitModel` | Author, Committer, Message |
| `AssetModel` | Id |

## Data Persistence

### SQL Server

All data is persisted to a SQL Server database. SQL statements are externalised as `.sql` files loaded from a configurable directory at runtime.

### SQL Operations

| Operation | SQL File | Purpose |
|---|---|---|
| Get Last Run Date | `GetLastRunDate.sql` | Retrieve the last scrape timestamp for a repository |
| Get Issues | `GetIssues.sql` | Retrieve existing issues for deduplication |
| Output Issue | `OutputIssue.sql` | Insert a scraped issue |
| Output Commit | `OutputCommit.sql` | Insert a scraped commit |
| Output Pull Request | `OutputPullRequest.sql` | Insert a scraped pull request |
| Output Workflow Run | `OutputWorkflowRun.sql` | Insert a scraped workflow run |
| Output Release | `OutputRelease.sql` | Insert a scraped release |
| Log Issue Aggregate | `LogIssueAggregate.sql` | Upsert daily issue created/solved counts |
| Log Run | `LogRun.sql` | Record execution summary with counts per data type |

### Issue Aggregate Logic

The `DatabaseFunction.CreateAggregates` method generates daily statistics:

- Counts issues created per day (from `Created_At`)
- Counts issues solved per day (from `Closed_At`)
- Fills date gaps between the earliest issue and today with zero-count records
- Excludes issues that already exist unchanged in the database

## Configuration

### App.config Structure

```xml
<appSettings>
  <add key="Owner" value="<GitHub repository owner>" />
  <add key="Repositories" value="<comma-separated repository names>" />
  <add key="Workflows" value="<comma-separated workflow filenames>" />
  <add key="BearerToken" value="<GitHub Personal Access Token>" />
  <add key="SQLConnectionString" value="<SQL Server connection string>" />
  <add key="SQLFiles" value="<path to directory containing .sql files>" />
</appSettings>
```

| Setting | Required | Purpose |
|---|---|---|
| `Owner` | Yes | GitHub repository owner or organisation |
| `Repositories` | Yes | Comma-separated list of repository names to scrape |
| `Workflows` | No | Comma-separated list of workflow filenames to query (e.g., `Commit.yml,Pull Request.yml`) |
| `BearerToken` | Yes | GitHub Personal Access Token for API authentication |
| `SQLConnectionString` | Yes | SQL Server connection string |
| `SQLFiles` | Yes | Path to directory containing externalised SQL files |

## Logging

- **Framework:** log4net 3.1.0
- **Configuration:** Embedded in App.config

### Appenders

| Appender | Type | File | Purpose |
|---|---|---|---|
| ConsoleAppender | Console | - | Display INFO-WARN messages to console |
| LogAppender | RollingFile | `Logs\Scraper.log` | Application operation logs (INFO+) |

### Log File Settings

- **Max File Size:** 10 MB
- **Backup Count:** 10 rolling files
- **Format:** `{ISO8601 Timestamp} {LEVEL} - {Message}`
- **Lock Model:** MinimalLock (concurrent access safe)

## CI/CD

### GitHub Actions Workflows

All workflows run on `windows-latest` using .NET 10.0.x SDK.

| Workflow | Trigger | Steps |
|---|---|---|
| **CI on Commit** (`Commit.yml`) | Push to any branch | Checkout, Restore, Build (Release) |
| **CI on Pull Request** (`Pull Request.yml`) | PR to any branch | Checkout, Restore, Build (Release), Run Tests |
| **Check for Linked Issue** (`PR Linked Issue.yml`) | PR opened/edited/reopened/synchronised | Verifies PR has linked GitHub issues via description, comments, or Development section |

### Build Configuration

- **SDK:** .NET 10.0.x
- **Configuration:** Release
- **Test Runner:** `dotnet test` (MSTest with method-level parallelisation)

## Hosting Requirements

### Runtime Prerequisites

- .NET 8.0 Runtime
- Windows or Linux (no OS-specific dependencies)
- Network access to SQL Server

### Network Requirements

- Outbound HTTPS to `api.github.com` for API requests
- Outbound connection to SQL Server (configurable via connection string)

### File System Requirements

- Read access to the SQL files directory
- Read/write access to the `Logs/` directory
