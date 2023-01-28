using System.Text;

using IdentityServer4.Models;
using IdentityServer4.Stores;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.ObjectPool;

using MongoDB.Driver;

namespace Victa.Backend.Accounts.Infrastructure.IdentityServer.Adapters.Stores.Operational;

public class PersistedGrantStore : IPersistedGrantStore
{
    private readonly IMemoryCache _cache;
    private readonly IMongoCollection<PersistedGrant> _collection;
    private readonly ILogger<PersistedGrantStore> _logger;

    public PersistedGrantStore(
        IMemoryCache cache,
        IMongoCollection<PersistedGrant> collection,
        ILogger<PersistedGrantStore> logger)
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

    public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
    {
        List<PersistedGrant>? items = await _cache.GetOrCreateAsync(filter.CreateCacheKey(), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);

            List<PersistedGrant> items =
                await _collection.Find(Builders<PersistedGrant>.Filter.And(CreateFilters(filter))).ToListAsync();

            return items;
        });

        return items ?? new List<PersistedGrant>();
    }

    public Task<PersistedGrant?> GetAsync(string key)
    {
        return _cache.GetOrCreateAsync($"persistedgrants@{key}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);

            PersistedGrant item =
                await _collection.Find(new FilterDefinitionBuilder<PersistedGrant>().Eq(x => x.Key, key))
                    .FirstOrDefaultAsync();

            return item;
        });
    }

    public Task RemoveAllAsync(PersistedGrantFilter filter)
    {
        return _collection.DeleteManyAsync(Builders<PersistedGrant>.Filter.And(CreateFilters(filter)));
    }

    public Task RemoveAsync(string key)
    {
        return _collection.DeleteOneAsync(new FilterDefinitionBuilder<PersistedGrant>().Eq(x => x.Key, key));
    }

    public Task StoreAsync(PersistedGrant grant)
    {
        return _collection.UpdateOneAsync(
            Builders<PersistedGrant>.Filter.Eq(x => x.Key, grant.Key),
            Builders<PersistedGrant>.Update
                .Set(x => x.SubjectId, grant.SubjectId)
                .Set(x => x.SessionId, grant.SessionId)
                .Set(x => x.CreationTime, grant.CreationTime)
                .Set(x => x.Expiration, grant.Expiration)
                .Set(x => x.ConsumedTime, grant.ConsumedTime)
                .Set(x => x.Data, grant.Data)
                .SetOnInsert(x => x.ClientId, grant.ClientId)
                .SetOnInsert(x => x.Description, grant.Description)
                .SetOnInsert(x => x.Type, grant.Type),
            new UpdateOptions
            {
                IsUpsert = true
            });
    }


    private static IEnumerable<FilterDefinition<PersistedGrant>> CreateFilters(PersistedGrantFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.ClientId))
        {
            yield return Builders<PersistedGrant>.Filter.Eq(x => x.ClientId, filter.ClientId);
        }


        if (!string.IsNullOrEmpty(filter.SessionId))
        {
            yield return Builders<PersistedGrant>.Filter.Eq(x => x.SessionId, filter.SessionId);
        }


        if (!string.IsNullOrEmpty(filter.SubjectId))
        {
            yield return Builders<PersistedGrant>.Filter.Eq(x => x.SubjectId, filter.SubjectId);
        }


        if (!string.IsNullOrEmpty(filter.Type))
        {
            yield return Builders<PersistedGrant>.Filter.Eq(x => x.Type, filter.Type);
        }

        yield break;
    }
}


internal static class PersistedGrantFilterExtensions
{
    private static readonly ObjectPool<StringBuilder> _stringBuildersPool =
        new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());


    public static string CreateCacheKey(this PersistedGrantFilter filter)
    {
        StringBuilder sb = _stringBuildersPool.Get();
        try
        {
            _ = sb.Append(nameof(filter.Type))
                .Append(nameof(filter.SessionId))
                .Append(nameof(filter.ClientId))
                .Append(nameof(filter.SubjectId))
                .Append(filter.Type)
                .Append(filter.SessionId)
                .Append(filter.ClientId)
                .Append(filter.SubjectId);

            return sb.ToString();
        }
        finally
        {
            _stringBuildersPool.Return(sb);
        }
    }
}
