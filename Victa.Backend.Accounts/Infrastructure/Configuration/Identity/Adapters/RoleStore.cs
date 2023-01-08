using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using MongoDB.Driver;
using MongoDB.Driver.Linq;

using Victa.Backend.Accounts.Domain.Models.RoleAggregate;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.Identity.Adapters;

public class RoleStore : IRoleClaimStore<AccountsRole>, IQueryableRoleStore<AccountsRole>
{
    private bool _disposed;
    private readonly IMongoCollection<AccountsRole> _collection;

    public RoleStore(IMongoCollection<AccountsRole> collection, IdentityErrorDescriber describer)
    {
        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        _collection = collection;

        ErrorDescriber = describer
            ?? new IdentityErrorDescriber();
    }

    public IQueryable<AccountsRole> Roles =>
        _collection.AsQueryable();

    public IdentityErrorDescriber ErrorDescriber { get; set; }

    public async Task<IdentityResult> CreateAsync(AccountsRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        try
        {
            await _collection.InsertOneAsync(role, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (MongoDuplicateKeyException)
        {
            return IdentityResult.Failed(new[]
            {
                new IdentityError {Code = "db@keyduplication"}
            });
        }
        catch (Exception)
        {
            return IdentityResult.Failed(Array.Empty<IdentityError>());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(AccountsRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        string currentConcurrencyStamp = role.ConcurrencyStamp;
        role.ConcurrencyStamp = Guid.NewGuid().ToString();

        ReplaceOneResult result = await _collection.ReplaceOneAsync(x => x.Id.Equals(role.Id) && x.ConcurrencyStamp.Equals(currentConcurrencyStamp), role, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!result.IsAcknowledged || result.ModifiedCount == 0)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(AccountsRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        DeleteResult result = await _collection.DeleteOneAsync(x => x.Id.Equals(role.Id) && x.ConcurrencyStamp.Equals(role.ConcurrencyStamp), cancellationToken).ConfigureAwait(false);
        if (!result.IsAcknowledged || result.DeletedCount == 0)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public Task<string> GetRoleIdAsync(AccountsRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        return Task.FromResult(role.Id);
    }

    public Task<string> GetRoleNameAsync(AccountsRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(AccountsRole role, string roleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        role.Name = roleName;

        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedRoleNameAsync(AccountsRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(AccountsRole role, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        role.NormalizedName = normalizedRoleName;

        return Task.CompletedTask;
    }

    public Task<AccountsRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return _collection.AsQueryable().FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken: cancellationToken);
    }

    public Task<AccountsRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return _collection.AsQueryable().FirstOrDefaultAsync(x => x.NormalizedName == normalizedName, cancellationToken: cancellationToken);
    }

    public async Task<IList<Claim>> GetClaimsAsync(AccountsRole role, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        AccountsRole? dbRole = await _collection.AsQueryable()
            .FirstOrDefaultAsync(x => x.Id.Equals(role.Id), cancellationToken: cancellationToken).ConfigureAwait(false);

        if (dbRole is null)
        {
        }

        return dbRole.Claims.Select(e => new Claim(e.ClaimType, e.ClaimValue)).ToList() ?? new List<Claim>();
    }

    public async Task AddClaimAsync(AccountsRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (claim == null)
        {
            throw new ArgumentNullException(nameof(claim));
        }

        var identityRoleClaim = new IdentityRoleClaim<string>()
        {
            ClaimType = claim.Type,
            ClaimValue = claim.Value
        };

        role.Claims.Add(identityRoleClaim);

        _ = await _collection.UpdateOneAsync(x => x.Id.Equals(role.Id), Builders<AccountsRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public Task RemoveClaimAsync(AccountsRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (claim == null)
        {
            throw new ArgumentNullException(nameof(claim));
        }

        _ = role.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

        return _collection.UpdateOneAsync(x => x.Id.Equals(role.Id), Builders<AccountsRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken);
    }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
