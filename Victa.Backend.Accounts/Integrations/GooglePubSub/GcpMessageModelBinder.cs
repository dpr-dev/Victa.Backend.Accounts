using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Victa.Backend.Accounts.Integrations.GooglePubSub;

public sealed class GcpMessageModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        Message? message = await bindingContext.HttpContext.Request.ReadFromJsonAsync<Message>();
        if (message is
            { MessageData.Data: var encodedBody }
            && string.IsNullOrEmpty(encodedBody) == false)
        {
            object? value = JsonSerializer.Deserialize(
                Convert.FromBase64String(encodedBody),
                bindingContext.ModelMetadata.ModelType.GenericTypeArguments[0]);

            object? objectInstance = Activator.CreateInstance(bindingContext.ModelMetadata.ModelType, new object[]
            {
                    value,
                    message.MessageData.MessageId,
                    message.Subscription,
                    message.MessageData.Attributes ?? new Dictionary<string, string>(),
                    message.MessageData.PublishTime
            });

            if (objectInstance is { })
            {
                bindingContext.Result = ModelBindingResult.Success(objectInstance);
                return;
            }
        }

        bindingContext.Result = ModelBindingResult.Failed();
    }



    private sealed class Message
    {
        [JsonPropertyName("message")]
        public MessageData MessageData { get; set; } = null!;


        [JsonPropertyName("subscription")]
        public string Subscription { get; set; } = null!;
    }

    private sealed class MessageData
    {
        [JsonPropertyName("data")]
        public string Data { get; set; } = null!;

        [JsonPropertyName("message_id")]
        public string MessageId { get; set; } = null!;

        [JsonPropertyName("attributes")]
        public Dictionary<string, string>? Attributes { get; set; }

        [JsonPropertyName("publish_time")]
        public string PublishTime { get; set; } = null!;
    }
}
