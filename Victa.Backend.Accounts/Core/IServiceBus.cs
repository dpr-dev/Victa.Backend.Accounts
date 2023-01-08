namespace Victa.Backend.Accounts.Core;

public interface IServiceBus
{
    Task Publish<T>(T @event) where T : class;
}
