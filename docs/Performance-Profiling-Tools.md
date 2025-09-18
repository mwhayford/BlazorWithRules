# 🔍 Performance Profiling Tools for BlazorWithRules

This guide outlines comprehensive performance profiling tools and strategies for the BlazorWithRules loan application system.

## 🎯 Performance Profiling Strategy

### **Key Areas to Monitor**

- **Application Performance** - Response times, throughput, errors
- **Database Performance** - Query execution, connection pooling
- **Memory Usage** - Memory leaks, GC pressure, heap analysis
- **Blazor Server Performance** - SignalR connections, component rendering
- **Business Logic** - Loan calculation performance, validation times
- **Infrastructure** - Docker container metrics, Redis caching

---

## 🚀 Application Performance Monitoring (APM)

### **1. Application Insights (Recommended for Production)**

**Setup:**

```csharp
// Add package
dotnet add package Microsoft.ApplicationInsights.AspNetCore

// Program.cs
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]);

// Custom telemetry for loan applications
builder.Services.AddSingleton<ITelemetryInitializer, LoanApplicationTelemetryInitializer>();
```

**Configuration:**

```json
// appsettings.json
{
    "ApplicationInsights": {
        "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://...",
        "EnableAdaptiveSampling": true,
        "EnablePerformanceCounterCollectionModule": true,
        "EnableDependencyTrackingTelemetryModule": true
    }
}
```

**Custom Telemetry:**

```csharp
public class LoanApplicationTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is RequestTelemetry requestTelemetry)
        {
            requestTelemetry.Properties["LoanApplicationVersion"] = "3.0";
            requestTelemetry.Properties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        }
    }
}

// In your service
public class LoanApplicationService
{
    private readonly TelemetryClient _telemetryClient;

    public async Task<LoanApplicationDto> CreateAsync(CreateLoanApplicationDto dto)
    {
        using var operation = _telemetryClient.StartOperation<DependencyTelemetry>("LoanApplication.Create");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Business logic
            var result = await CreateLoanApplicationAsync(dto);

            _telemetryClient.TrackMetric("LoanApplication.CreateDuration", stopwatch.ElapsedMilliseconds);
            _telemetryClient.TrackEvent("LoanApplication.Created", new Dictionary<string, string>
            {
                ["LoanAmount"] = dto.RequestedAmount.ToString(),
                ["LoanTerm"] = dto.TermInMonths.ToString()
            });

            return result;
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex);
            throw;
        }
    }
}
```

**Benefits:**

- ✅ **Real-time monitoring** of application performance
- ✅ **Dependency tracking** (SQL Server, Redis, Google OAuth)
- ✅ **Exception tracking** with stack traces
- ✅ **Custom metrics** for loan application workflow
- ✅ **Live dashboards** and alerting

### **2. MiniProfiler (Development & Testing)**

**Setup:**

```csharp
// Add package
dotnet add package MiniProfiler.AspNetCore.Mvc
dotnet add package MiniProfiler.EntityFrameworkCore

// Program.cs
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
    options.PopupRenderPosition = RenderPosition.BottomLeft;
    options.ColorScheme = ColorScheme.Auto;
    options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();
});

// Add EF Core profiling
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.AddInterceptors(new MiniProfilerCommandInterceptor());
    }
});

// Middleware
app.UseMiniProfiler();
```

**Custom Profiling:**

```csharp
public async Task<LoanTermsCalculation> CalculateLoanTermsAsync(decimal amount, int term)
{
    using (MiniProfiler.Current.Step("Loan Terms Calculation"))
    {
        using (MiniProfiler.Current.Step("Credit Score Lookup"))
        {
            // Credit score logic
        }

        using (MiniProfiler.Current.Step("Interest Rate Calculation"))
        {
            // Rate calculation logic
        }

        using (MiniProfiler.Current.Step("Payment Calculation"))
        {
            // Payment calculation logic
        }
    }
}
```

**Benefits:**

- ✅ **Real-time profiling** in browser
- ✅ **SQL query analysis** with execution times
- ✅ **N+1 query detection**
- ✅ **Custom timing** for business logic steps

---

## 🗄️ Database Performance Profiling

### **1. Entity Framework Profiling**

**Detailed Logging:**

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
        options.ConfigureWarnings(warnings =>
            warnings.Log(RelationalEventId.MultipleCollectionIncludeWarning));
    }
});
```

**Query Performance Logging:**

```csharp
public class PerformanceLoggingInterceptor : DbCommandInterceptor
{
    private readonly ILogger<PerformanceLoggingInterceptor> _logger;

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Duration.TotalMilliseconds > 1000) // Log slow queries
        {
            _logger.LogWarning("Slow query detected: {Duration}ms - {CommandText}",
                eventData.Duration.TotalMilliseconds, command.CommandText);
        }

        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }
}
```

### **2. SQL Server Query Store**

**Enable Query Store:**

```sql
-- Enable query store
ALTER DATABASE BlazorWithRules SET QUERY_STORE = ON
(
    OPERATION_MODE = READ_WRITE,
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    INTERVAL_LENGTH_MINUTES = 60,
    MAX_STORAGE_SIZE_MB = 1000,
    QUERY_CAPTURE_MODE = AUTO,
    SIZE_BASED_CLEANUP_MODE = AUTO
);
```

**Query Performance Analysis:**

```sql
-- Top 10 slowest queries
SELECT TOP 10
    qt.query_sql_text,
    rs.avg_duration / 1000.0 AS avg_duration_ms,
    rs.avg_cpu_time / 1000.0 AS avg_cpu_time_ms,
    rs.execution_count,
    rs.avg_logical_io_reads
FROM sys.query_store_query_text qt
INNER JOIN sys.query_store_query q ON qt.query_text_id = q.query_text_id
INNER JOIN sys.query_store_plan p ON q.query_id = p.query_id
INNER JOIN sys.query_store_runtime_stats rs ON p.plan_id = rs.plan_id
WHERE rs.last_execution_time > DATEADD(hour, -24, GETUTCDATE())
ORDER BY rs.avg_duration DESC;

-- Queries with high resource consumption
SELECT
    qt.query_sql_text,
    rs.execution_count,
    rs.avg_duration / 1000.0 AS avg_duration_ms,
    rs.avg_cpu_time / 1000.0 AS avg_cpu_time_ms,
    (rs.avg_duration * rs.execution_count) / 1000.0 AS total_duration_ms
FROM sys.query_store_query_text qt
INNER JOIN sys.query_store_query q ON qt.query_text_id = q.query_text_id
INNER JOIN sys.query_store_plan p ON q.query_id = p.query_id
INNER JOIN sys.query_store_runtime_stats rs ON p.plan_id = rs.plan_id
WHERE qt.query_sql_text LIKE '%LoanApplication%'
ORDER BY total_duration_ms DESC;
```

---

## 🧠 Memory Profiling

### **1. dotMemory (JetBrains)**

**Installation:**

```bash
# Download from JetBrains website
# Or use command line tools
dotnet tool install -g JetBrains.dotMemory.Console
```

**Profiling Commands:**

```bash
# Start profiling
dotmemory start --trigger-timer=30s --save-to=memory-snapshot.dmw --target-executable="dotnet" --target-arguments="run --project src/BlazorApp.Web"

# Get snapshot during execution
dotmemory get-snapshot --save-to=runtime-snapshot.dmw

# Force garbage collection and get snapshot
dotmemory force-gc --get-snapshot --save-to=gc-snapshot.dmw
```

**Memory Monitoring in Code:**

```csharp
public class MemoryMonitoringService : BackgroundService
{
    private readonly ILogger<MemoryMonitoringService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var gc0 = GC.CollectionCount(0);
            var gc1 = GC.CollectionCount(1);
            var gc2 = GC.CollectionCount(2);
            var totalMemory = GC.GetTotalMemory(false);

            _logger.LogInformation("Memory Stats - Total: {TotalMemory} bytes, GC0: {GC0}, GC1: {GC1}, GC2: {GC2}",
                totalMemory, gc0, gc1, gc2);

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

### **2. PerfView (Microsoft)**

**Download and Setup:**

```bash
# Download from Microsoft GitHub
# https://github.com/Microsoft/perfview/releases

# Capture ETW events
PerfView.exe /GCOnly collect memory-trace.etl

# Run your application with profiling
dotnet run --project src/BlazorApp.Web

# Stop collection
PerfView.exe stop
```

**Benefits:**

- ✅ **Memory leak detection**
- ✅ **Object allocation analysis**
- ✅ **GC pressure monitoring**
- ✅ **ETW event tracing**

---

## ⚡ Blazor Server Performance

### **1. SignalR Connection Monitoring**

**Custom Hub with Metrics:**

```csharp
public class PerformanceHub : Hub
{
    private readonly ILogger<PerformanceHub> _logger;
    private readonly TelemetryClient _telemetryClient;

    public override async Task OnConnectedAsync()
    {
        _telemetryClient.TrackEvent("SignalR.Connected");
        _logger.LogInformation("SignalR connection established: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _telemetryClient.TrackEvent("SignalR.Disconnected", new Dictionary<string, string>
        {
            ["Exception"] = exception?.Message ?? "Normal"
        });
        await base.OnDisconnectedAsync(exception);
    }
}
```

### **2. Component Rendering Performance**

**Render Timing:**

```csharp
@implements IDisposable
@inject IJSRuntime JSRuntime

@code {
    private DateTime _renderStart;
    private DateTime _renderEnd;

    protected override void OnInitialized()
    {
        _renderStart = DateTime.UtcNow;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _renderEnd = DateTime.UtcNow;
            var renderTime = (_renderEnd - _renderStart).TotalMilliseconds;

            await JSRuntime.InvokeVoidAsync("console.log", $"Component render time: {renderTime}ms");

            // Track in Application Insights
            TelemetryClient.TrackMetric("Component.RenderTime", renderTime, new Dictionary<string, string>
            {
                ["ComponentName"] = nameof(LoanApplication)
            });
        }
    }
}
```

---

## 🐳 Docker Container Performance

### **1. Container Metrics**

**Docker Stats:**

```bash
# Real-time container stats
docker stats blazorwithrules-app

# Export to file
docker stats --no-stream blazorwithrules-app > container-stats.txt

# Memory and CPU usage
docker exec blazorwithrules-app cat /proc/meminfo
docker exec blazorwithrules-app cat /proc/cpuinfo
```

**Container Resource Monitoring:**

```yaml
# docker-compose.yml
services:
    app:
        # ... other config
        deploy:
            resources:
                limits:
                    memory: 2G
                    cpus: "1.0"
                reservations:
                    memory: 1G
                    cpus: "0.5"
        healthcheck:
            test: ["CMD", "curl", "-f", "http://localhost:80/health"]
            interval: 30s
            timeout: 10s
            retries: 3
            start_period: 40s
```

### **2. Application Metrics in Container**

**Health Check Endpoint:**

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow
        });

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
});
```

---

## 📊 Business Logic Performance

### **1. Loan Calculation Performance**

**Benchmarking:**

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class LoanCalculationBenchmarks
{
    private readonly LoanApplicationService _service;

    [Benchmark]
    public async Task<LoanTermsCalculation> CalculateLoanTerms_SmallAmount()
    {
        return await _service.CalculateLoanTermsAsync(10000, 36);
    }

    [Benchmark]
    public async Task<LoanTermsCalculation> CalculateLoanTerms_LargeAmount()
    {
        return await _service.CalculateLoanTermsAsync(500000, 360);
    }

    [Benchmark]
    public async Task<LoanTermsCalculation> CalculateLoanTerms_WithCreditScore()
    {
        return await _service.CalculateLoanTermsAsync(100000, 180, 750);
    }
}
```

**Run Benchmarks:**

```bash
# Add package
dotnet add package BenchmarkDotNet

# Run benchmarks
dotnet run -c Release --project tests/BlazorApp.Benchmarks
```

### **2. Validation Performance**

**FluentValidation Profiling:**

```csharp
public class ValidationPerformanceInterceptor : IValidatorInterceptor
{
    private readonly ILogger<ValidationPerformanceInterceptor> _logger;

    public IValidationContext BeforeValidation(IValidationContext context, ValidationResult result)
    {
        context.RootContextData["StartTime"] = DateTime.UtcNow;
        return context;
    }

    public ValidationResult AfterValidation(IValidationContext context, ValidationResult result)
    {
        if (context.RootContextData.TryGetValue("StartTime", out var startTime))
        {
            var duration = DateTime.UtcNow - (DateTime)startTime;
            _logger.LogInformation("Validation completed in {Duration}ms for {InstanceType}",
                duration.TotalMilliseconds, context.InstanceToValidate.GetType().Name);
        }

        return result;
    }
}
```

---

## 🛠️ Performance Monitoring Setup

### **1. Create Performance Monitoring Script**

```powershell
# performance-monitor.ps1
param(
    [int]$DurationMinutes = 30,
    [string]$OutputPath = "performance-logs"
)

Write-Host "Starting performance monitoring for $DurationMinutes minutes..." -ForegroundColor Green

# Create output directory
New-Item -ItemType Directory -Path $OutputPath -Force

# Start monitoring
$jobs = @()

# Docker stats
$jobs += Start-Job -ScriptBlock {
    param($OutputPath, $Duration)
    docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}\t{{.BlockIO}}" > "$OutputPath/docker-stats.log"
} -ArgumentList $OutputPath, $DurationMinutes

# Application logs
$jobs += Start-Job -ScriptBlock {
    param($OutputPath, $Duration)
    docker logs -f blazorwithrules-app > "$OutputPath/app-logs.log" 2>&1
} -ArgumentList $OutputPath, $DurationMinutes

# Database performance
$jobs += Start-Job -ScriptBlock {
    param($OutputPath, $Duration)
    $query = @"
    SELECT
        GETDATE() as timestamp,
        cntr_value as value,
        counter_name
    FROM sys.dm_os_performance_counters
    WHERE counter_name IN ('Batch Requests/sec', 'SQL Compilations/sec', 'Page lookups/sec')
"@

    for ($i = 0; $i -lt ($Duration * 12); $i++) {
        docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -Q "$query" >> "$OutputPath/db-performance.log"
        Start-Sleep 5
    }
} -ArgumentList $OutputPath, $DurationMinutes

Write-Host "Monitoring started. Waiting for $DurationMinutes minutes..."
Start-Sleep ($DurationMinutes * 60)

# Stop all jobs
$jobs | Stop-Job
$jobs | Remove-Job

Write-Host "Performance monitoring completed. Logs saved to $OutputPath" -ForegroundColor Green
```

### **2. Automated Performance Testing**

```csharp
// tests/BlazorApp.PerformanceTests/LoadTests.cs
[TestClass]
public class LoadTests
{
    [TestMethod]
    public async Task LoanApplication_Submit_LoadTest()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        var tasks = new List<Task>();

        // Simulate 100 concurrent loan applications
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(SubmitLoanApplicationAsync(httpClient, i));
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        Console.WriteLine($"100 loan applications completed in {stopwatch.ElapsedMilliseconds}ms");
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 30000, "Load test took too long");
    }

    private async Task SubmitLoanApplicationAsync(HttpClient client, int index)
    {
        var loanApplication = new CreateLoanApplicationDto
        {
            FirstName = $"Test{index}",
            LastName = "User",
            Email = $"test{index}@example.com",
            RequestedAmount = 50000,
            TermInMonths = 60
            // ... other properties
        };

        var json = JsonSerializer.Serialize(loanApplication);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/loan-applications", content);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }
}
```

---

## 📈 Recommended Monitoring Stack

### **Development Environment**

1. **MiniProfiler** - Real-time query profiling
2. **dotMemory** - Memory leak detection
3. **Entity Framework Logging** - Database performance
4. **Custom performance counters** - Business logic timing

### **Production Environment**

1. **Application Insights** - APM and monitoring
2. **SQL Server Query Store** - Database performance
3. **Docker metrics** - Container performance
4. **Custom telemetry** - Business metrics

### **Performance Testing**

1. **BenchmarkDotNet** - Micro-benchmarks
2. **Load testing tools** - Concurrent user simulation
3. **Memory profilers** - Memory leak detection
4. **Database profiling** - Query optimization

---

_Last Updated: January 18, 2025_  
_Version: 1.0_
