using CvEvaluator.Application.Interfaces;
using System.Threading.Channels;

namespace CvEvaluator.Infrastructure.Background;

public class EvaluationQueue : IEvaluationQueue
{
    private readonly Channel<Guid> _channel;

    public EvaluationQueue()
    {
        _channel = Channel.CreateUnbounded<Guid>();
    }

    public ChannelReader<Guid> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(Guid evaluationId)
        => _channel.Writer.WriteAsync(evaluationId);
}