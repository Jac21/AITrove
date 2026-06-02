# Sample Invocation

## Input

```csharp
public sealed class ReportService
{
    public string BuildReport(string url)
    {
        var data = FetchDataAsync(url).Result;
        return data.ToUpperInvariant();
    }

    public async Task<string> FetchDataAsync(string url)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string[]> FetchAllAsync(IEnumerable<string> urls)
    {
        var tasks = urls.Select(FetchDataAsync);
        return await Task.WhenAll(tasks);
    }
}
```

## Phase 1 Output

`TaskPitfallReviewer` returns only compressed context:

```markdown
## TaskReviewHandoff

### Scope
- reviewed_area: ReportService Task usage and fan-out patterns.
- assumptions: Code is service-side C# and may execute under request load.
- omitted: Detailed await capture and exception-flow review deferred to phase 2.

### Risk Map
| id | severity | pattern | symbol_or_location | confidence | why_it_matters |
| --- | --- | --- | --- | --- | --- |
| T1 | high | sync-over-async | BuildReport | high | `.Result` can deadlock or block thread-pool threads under load. |
| T2 | medium | missing cancellation | FetchDataAsync, FetchAllAsync | high | Callers cannot cancel HTTP work or fan-out operations. |
| T3 | medium | unbounded fan-out | FetchAllAsync | medium | `Task.WhenAll` over arbitrary input can exhaust sockets or remote dependencies. |
| T4 | medium | task lifetime and disposal | FetchDataAsync | medium | Per-call `HttpClient` creation can cause socket exhaustion and weak lifetime management. |

### Priority Symbols
- symbol: BuildReport
  reason: Contains sync-over-async boundary.
- symbol: FetchDataAsync
  reason: Central async I/O method used by fan-out.
- symbol: FetchAllAsync
  reason: Performs unconstrained concurrent task creation.

### Phase 2 Questions
- question: Can BuildReport be made async end-to-end instead of blocking on `.Result`?
  linked_risk_ids: T1
- question: Should FetchDataAsync accept and pass a CancellationToken through GetAsync and ReadAsStringAsync?
  linked_risk_ids: T2
- question: Does FetchAllAsync need bounded concurrency or batching?
  linked_risk_ids: T3

### Compression Notes
- code_quoted_lines: 0
- dropped_context: General Task education and full code excerpts excluded.
```

## Phase 2 Invocation Shape

`AsyncAwaitReviewer` receives:

- original code
- compressed `TaskReviewHandoff`
- caller scope

It does not receive the full phase 1 analysis transcript.

## Phase 2 Final Review Shape

```markdown
## Summary

The most important issue is the sync-over-async boundary in `BuildReport`, followed by missing cancellation and unbounded fan-out.

## Findings

| severity | location | finding | phase 1 context |
| --- | --- | --- | --- |
| high | BuildReport | Blocks on `FetchDataAsync(url).Result`, which can deadlock or starve threads. Convert the call chain to async and return `Task<string>`. | T1 |
| medium | FetchDataAsync | Does not accept or propagate `CancellationToken`; HTTP calls can outlive caller intent. Add a token parameter and pass it to async I/O. | T2 |
| medium | FetchAllAsync | Runs all URL fetches concurrently without a limit. Add bounded concurrency for large inputs. | T3 |

## Recommended Fixes

1. Change `BuildReport` to `BuildReportAsync` and `await FetchDataAsync`.
2. Thread `CancellationToken` through all async methods.
3. Reuse `HttpClient` through dependency injection.
4. Bound fan-out in `FetchAllAsync`.

## Residual Risk

The sample does not show calling code, dependency injection setup, or expected input sizes, so concurrency limits and API shape should be validated against production usage.
```
