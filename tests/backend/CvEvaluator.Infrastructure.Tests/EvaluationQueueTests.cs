using CvEvaluator.Infrastructure.Background;

namespace CvEvaluator.Infrastructure.Tests;

public class EvaluationQueueTests
{
    [Fact]
    public async Task EnqueueAsync_WritesIdToChannel()
    {
        var queue = new EvaluationQueue();
        var id = Guid.NewGuid();

        await queue.EnqueueAsync(id);

        Assert.True(queue.Reader.TryRead(out var result));
        Assert.Equal(id, result);
    }

    [Fact]
    public async Task EnqueueAsync_MultipleIds_AllReadableInOrder()
    {
        var queue = new EvaluationQueue();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        await queue.EnqueueAsync(id1);
        await queue.EnqueueAsync(id2);
        await queue.EnqueueAsync(id3);

        Assert.True(queue.Reader.TryRead(out var r1));
        Assert.True(queue.Reader.TryRead(out var r2));
        Assert.True(queue.Reader.TryRead(out var r3));
        Assert.Equal(id1, r1);
        Assert.Equal(id2, r2);
        Assert.Equal(id3, r3);
    }

    [Fact]
    public void Reader_ReturnsChannelReader()
    {
        var queue = new EvaluationQueue();

        Assert.NotNull(queue.Reader);
    }
}
