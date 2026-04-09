namespace SlotFlow.Api.Infrastructure.BackgroundJobs;

public sealed class ExpiryJobOptions
{
    public const string SectionName = "ExpiryJob";
    public int IntervalSeconds { get; init; } = 30;
}