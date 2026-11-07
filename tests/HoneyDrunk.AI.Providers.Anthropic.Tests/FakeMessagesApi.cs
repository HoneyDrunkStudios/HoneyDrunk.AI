using Anthropic.Models.Messages;
using System.Runtime.CompilerServices;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Records requests and returns canned SDK responses; never touches the network.</summary>
internal sealed class FakeMessagesApi : IAnthropicMessagesApi
{
    public List<MessageCreateParams> Requests { get; } = [];

    public Message? NextMessage { get; set; }

    public IReadOnlyList<RawMessageStreamEvent> NextEvents { get; set; } = [];

    public Task<Message> CreateAsync(MessageCreateParams request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        this.Requests.Add(request);
        return Task.FromResult(this.NextMessage ?? throw new InvalidOperationException("No canned message."));
    }

    public async IAsyncEnumerable<RawMessageStreamEvent> CreateStreamingAsync(MessageCreateParams request, [EnumeratorCancellation] CancellationToken ct)
    {
        this.Requests.Add(request);
        foreach (var streamEvent in this.NextEvents)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return streamEvent;
        }
    }
}
