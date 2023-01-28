using IdentityServer4.Models;
using IdentityServer4.Stores;

using Microsoft.Extensions.Caching.Memory;

using MongoDB.Driver;

namespace Victa.Backend.Accounts.Infrastructure.IdentityServer.Adapters.Stores.Configurational;

public class ClientStore : IClientStore
{
    private readonly ILogger<ClientStore> _logger;
    private readonly IMongoCollection<Client> _collection;
    private readonly IMemoryCache _cache;

    public ClientStore(
        ILogger<ClientStore> logger,
        IMongoCollection<Client> collection,
        IMemoryCache cache)
    {
        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (cache is null)
        {
            throw new ArgumentNullException(nameof(cache));
        }

        _logger = logger;
        _collection = collection;
        _cache = cache;
    }

    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        _logger.LogInformation(
            "[{method}] Try find client (Client='{client}')",
            nameof(FindClientByIdAsync), clientId);

        Client? client = await _cache.GetOrCreateAsync($"is:client:{clientId}", entry =>
        {
            _logger.LogInformation(
                "[{method}] Client (Client='{client}') does not exists. Try find in the database",
                nameof(FindClientByIdAsync), clientId);

            _ = entry.SetAbsoluteExpiration(TimeSpan.FromHours(12));
            return _collection.Find(new FilterDefinitionBuilder<Client>().Eq(x => x.ClientId, clientId))
                .SingleOrDefaultAsync();
        });

        _logger.LogInformation(
            "[{method}] Client (Client='{client}') was {result}",
            nameof(FindClientByIdAsync), clientId, client is { } ? "found" : "not found");

        return client;
    }
}
