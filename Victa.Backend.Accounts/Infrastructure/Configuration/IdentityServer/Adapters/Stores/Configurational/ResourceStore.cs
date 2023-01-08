using System.Runtime.CompilerServices;

using IdentityServer4.Models;
using IdentityServer4.Stores;

using Microsoft.Extensions.Caching.Memory;

using MongoDB.Driver;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Stores.Configurational;

public class ResourceStore : IResourceStore
{
    private readonly IMemoryCache _cache;
    private readonly IMongoCollection<Resource> _collection;
    private readonly ILogger<ResourceStore> _logger;

    public ResourceStore(
        IMemoryCache cache,
        IMongoCollection<Resource> collection,
        ILogger<ResourceStore> logger)
    {
        if (cache is null)
        {
            throw new ArgumentNullException(nameof(cache));
        }

        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        _cache = cache;
        _collection = collection;
        _logger = logger;
    }

    public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> names)
    {
        List<ApiResource> result = await _cache.GetOrCreateAsync(CreateKey(names), entry =>
        {
            _ = entry.SetSlidingExpiration(TimeSpan.FromHours(1));

            return _collection.OfType<ApiResource>().Find(new FilterDefinitionBuilder<ApiResource>().In(x => x.Name, names))
                .ToListAsync();
        });

        if (result.Any())
        {
            _logger.LogDebug("Found {apis} API resource in database", result.Select(x => x.Name));
        }
        else
        {
            _logger.LogDebug("Did not find {apis} API resource in database", names);
        }

        return result;
    }

    public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        List<ApiResource> result = await _cache.GetOrCreateAsync(CreateKey(scopeNames), entry =>
        {
            _ = entry.SetSlidingExpiration(TimeSpan.FromHours(1));

            return _collection.OfType<ApiResource>()
                .Find(new FilterDefinitionBuilder<ApiResource>().AnyIn(x => x.Scopes, scopeNames))
                .ToListAsync();
        });

        _logger.LogDebug("Found {apis} API resources in database", result.Select(x => x.Name));

        return result;
    }

    public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
    {
        List<ApiScope> result = await _cache.GetOrCreateAsync(CreateKey(scopeNames), entry =>
        {
            _ = entry.SetSlidingExpiration(TimeSpan.FromHours(1));

            return _collection.OfType<ApiScope>()
                .Find(new FilterDefinitionBuilder<ApiScope>().In(x => x.Name, scopeNames))
                .ToListAsync();
        });

        _logger.LogDebug("Found {scopes} scopes in database", result.Select(x => x.Name));

        return result;
    }

    public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        List<IdentityResource> result = await _cache.GetOrCreateAsync(CreateKey(scopeNames), entry =>
        {
            _ = entry.SetSlidingExpiration(TimeSpan.FromHours(1));

            return _collection.OfType<IdentityResource>()
                .Find(new FilterDefinitionBuilder<IdentityResource>().In(x => x.Name, scopeNames))
                .ToListAsync();
        });

        _logger.LogDebug("Found {scopes} identity scopes in database", result.Select(x => x.Name));

        return result;
    }

    public async Task<Resources> GetAllResourcesAsync()
    {
        List<Resource> items = await _cache.GetOrCreateAsync($"{nameof(ResourceStore)}:{nameof(GetAllResourcesAsync)}", entry =>
        {
            return _collection.Find(FilterDefinition<Resource>.Empty).ToListAsync();
        });

        return new Resources(
            items.OfType<IdentityResource>(),
            items.OfType<ApiResource>(),
            items.OfType<ApiScope>());
    }


    private string CreateKey(IEnumerable<string> parts, [CallerMemberName] string? methodName = null)
    {
        return $"{methodName}@{string.Join(", ", parts)}";
    }
}
