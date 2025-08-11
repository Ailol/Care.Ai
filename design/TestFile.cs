namespace Design.Tests;

public interface ITestInterface
{
    Task<TestResult> ExecuteTestAsync(TestParameters parameters);
}

public record TestParameters(
    string TestName,
    Dictionary<string, object> Configuration,
    TimeSpan Timeout);

public record TestResult(
    bool Success,
    string Message,
    Dictionary<string, object> Metrics,
    DateTime ExecutedAt);