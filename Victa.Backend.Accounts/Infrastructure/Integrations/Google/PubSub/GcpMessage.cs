using Microsoft.AspNetCore.Mvc;

namespace Victa.Backend.Accounts.Infrastructure.Integrations.Google.PubSub;

[ModelBinder(typeof(GcpMessageModelBinder))]
public class GcpMessage<TData> where TData : class
{
    public GcpMessage(TData data,
        string messageId, string subscription,
        Dictionary<string, string>? attributes,
        string publishTime)
    {
        Data = data;
        MessageId = messageId;
        Subscription = subscription;
        Attributes = attributes;
        PublishTime = publishTime;
    }

    public TData Data { get; }
    public string MessageId { get; }
    public string Subscription { get; }
    public Dictionary<string, string>? Attributes { get; }
    public string PublishTime { get; }
}
