using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;

namespace Web.Infrastructure.Http;

public static class ResiliencePolicies
{
    public static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(
                    attempt * 500 + Random.Shared.Next(0, 200)),
                onRetry: (outcome, retryCount, context) =>
                {
                    Console.WriteLine($"[Resilience] Retry #{retryCount} after {outcome.Result?.StatusCode}");
                });

    public static readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> CircuitBreaker =
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => (int)r.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, duration) =>
                {
                    Console.WriteLine($"[CircuitBreaker] OPEN for {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("[CircuitBreaker] CLOSED — service recovered");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("[CircuitBreaker] HALF-OPEN — testing service");
                });
}
