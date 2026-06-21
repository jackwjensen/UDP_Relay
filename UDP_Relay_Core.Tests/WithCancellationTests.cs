using UDP_Relay_Core;
using Xunit;

namespace UDP_Relay_Core.Tests;

/// <summary>
/// Tests for <see cref="AsyncExtensions.WithCancellation{T}"/>, which adds
/// cancellation support to tasks that have no native cancellation (e.g. UdpClient.ReceiveAsync).
/// </summary>
public class WithCancellationTests
{
    [Fact]
    public async Task Returns_result_when_task_completes_before_cancellation()
    {
        var task = Task.Run(async () => { await Task.Delay(20); return 42; });

        int result = await task.WithCancellation(CancellationToken.None);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Returns_result_for_already_completed_task()
    {
        int result = await Task.FromResult(7).WithCancellation(CancellationToken.None);

        Assert.Equal(7, result);
    }

    [Fact]
    public async Task Throws_when_token_is_already_cancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var neverCompletes = new TaskCompletionSource<int>().Task;

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => neverCompletes.WithCancellation(cts.Token));
    }

    [Fact]
    public async Task Throws_when_cancelled_while_awaiting()
    {
        using var cts = new CancellationTokenSource();
        var neverCompletes = new TaskCompletionSource<int>().Task;

        cts.CancelAfter(50);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => neverCompletes.WithCancellation(cts.Token));
    }
}
