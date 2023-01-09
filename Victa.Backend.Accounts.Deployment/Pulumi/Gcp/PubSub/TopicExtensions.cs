namespace Pulumi.Gcp.PubSub;

public static class TopicExtensions
{
    public static Subscription Subscribe(this Topic topic, string name, Action<SubscriptionArgs> configure, CustomResourceOptions? options = null)
    {
        var args = new SubscriptionArgs { Topic = topic.Name, };
        configure?.Invoke(args);

        return new Subscription(name, args, options);
    }
}
