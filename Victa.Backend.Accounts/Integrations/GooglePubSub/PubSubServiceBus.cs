
using System.Collections.Concurrent;
using System.Text.Json;

using Google.Apis.Auth.OAuth2;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;

using Grpc.Auth;

using Microsoft.Extensions.Options;

using Victa.Backend.Accounts.Contracts.Events.Accounts;
using Victa.Backend.Accounts.Core;

using PubsubMessage = Google.Cloud.PubSub.V1.PubsubMessage;

namespace Victa.Backend.Accounts.Integrations.GooglePubSub;

internal class PubSubServiceBus : IServiceBus
{
    private readonly IReadOnlyDictionary<Type, string> _eventTypesTopicMappings;
    private readonly ILogger _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly PubSubOptions _options;
    private readonly ConcurrentDictionary<string, PublisherClient> _clients;
    private readonly Lazy<GoogleCredential> _credential;

    public PubSubServiceBus(
        IWebHostEnvironment environment,
        IOptions<PubSubOptions> options,
        ILogger<PubSubServiceBus> logger,
        Lazy<GoogleCredential> credential)
    {
        if (environment is null)
        {
            throw new ArgumentNullException(nameof(environment));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _options = options.Value;
        _clients = new ConcurrentDictionary<string, PublisherClient>();
        _logger = logger;
        _environment = environment;
        _credential = credential;
        _eventTypesTopicMappings = new Dictionary<Type, string>
        {
            { typeof(AccountsUserCreated), "accounts.user.created" },
            { typeof(AccountsUserUpdated), "accounts.user.updated" },
        };
    }

    public async Task Publish<T>(T @event) where T : class
    {
        if (!_eventTypesTopicMappings.TryGetValue(typeof(T), out string? topicName))
        {
            throw new InvalidOperationException("Event of type (Type='') is not registered");
        }

        var message = new PubsubMessage
        {
            Data = ByteString.FromBase64(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event))))
        };

        _ = await CreateForTopic(topicName).PublishAsync(message);
    }

    private PublisherClient CreateForTopic(string name)
    {
        if (CanUse())
        {
            return _clients.GetOrAdd($"{name}.{_options.ResourcePrefix}", key =>
            {
                try
                {
                    var builder = new PublisherClientBuilder
                    {
                        TopicName = new TopicName(_options.ProjectId, key),
                        ChannelCredentials = _credential.Value.ToChannelCredentials(),
                    };

                    return builder.Build();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Unable to create PublisherClient");

                    throw;
                }
            });
        }

        _logger.LogInformation(
            "[{method}] Looks like your app running in 'Development' Environment. Create fake client. " +
            "If you want use production publisher client set environment to 'Production' or 'Stage'",
            nameof(CreateForTopic));

        return new NoopSender();
    }

    private bool CanUse()
    {
        return _environment.IsDevelopment() != true;
    }
}
