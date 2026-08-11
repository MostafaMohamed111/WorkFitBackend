using System.Threading.Channels;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Infrastructure.Options;

namespace WorkFit.Engine.Infrastructure.CVParsing;

public sealed class CVProcessingChannel : IDisposable
{
    private readonly Channel<ProcessCVJobMessage> _channel;
    public CVProcessingChannel(IOptions<CVParsingOptions> options)
    {
        var capacity = Math.Max(1, options.Value.ChannelCapacity);
        _channel = Channel.CreateBounded<ProcessCVJobMessage>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }
    public ValueTask EnqueueAsync(ProcessCVJobMessage msg, CancellationToken ct = default) => _channel.Writer.WriteAsync(msg, ct);
    public IAsyncEnumerable<ProcessCVJobMessage> ReadAllAsync(CancellationToken ct = default) => _channel.Reader.ReadAllAsync(ct);
    public void Dispose() => _channel.Writer.TryComplete();
}

public sealed record ProcessCVJobMessage(Guid JobId);
