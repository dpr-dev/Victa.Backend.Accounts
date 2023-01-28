using Google.Cloud.PubSub.V1;
using Google.Protobuf;

namespace Victa.Backend.Accounts.Infrastructure.Integrations.Google.PubSub;

public sealed class NoopSender : PublisherClient
{
    public override Task<string> PublishAsync(PubsubMessage message)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public override Task<string> PublishAsync(string orderingKey, string message, System.Text.Encoding encoding = null)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public override Task<string> PublishAsync(string message, System.Text.Encoding encoding = null)
    {
        return base.PublishAsync(message, encoding);
    }

    public override Task<string> PublishAsync(string orderingKey, IMessage message)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public override Task<string> PublishAsync(IMessage message)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public override Task<string> PublishAsync(string orderingKey, ByteString message)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public override Task<string> PublishAsync(ByteString message)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public override Task<string> PublishAsync(string orderingKey, byte[] message)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public override Task<string> PublishAsync(byte[] message)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public override void ResumePublish(string orderingKey)
    {
    }

    public override Task ShutdownAsync(CancellationToken hardStopToken)
    {
        return Task.CompletedTask;
    }

    public override Task ShutdownAsync(TimeSpan timeout)
    {
        return Task.CompletedTask;
    }
}
