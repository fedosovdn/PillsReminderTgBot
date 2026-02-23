namespace PillsReminderTgBot.WebApi.Services;

public static class TimeProviderExtensions
{
    public static Task DelayAsync(this TimeProvider timeProvider, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        ITimer? timer = null;
        timer = timeProvider.CreateTimer(
            _ =>
            {
                timer?.Dispose();
                tcs.TrySetResult();
            },
            state: null,
            dueTime: delay,
            period: Timeout.InfiniteTimeSpan);

        if (cancellationToken.CanBeCanceled)
        {
            var registration = cancellationToken.Register(() =>
            {
                timer?.Dispose();
                tcs.TrySetCanceled(cancellationToken);
            });

            tcs.Task.ContinueWith(
                _ => registration.Dispose(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        return tcs.Task;
    }
}
