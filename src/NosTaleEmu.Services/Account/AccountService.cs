using Microsoft.EntityFrameworkCore;
using NosTaleEmu.Database;
using NosTaleEmu.Database.Entities;
using NosTaleEmu.Dto.Account;

namespace NosTaleEmu.Services.Account;

public sealed class AccountService
{
    private readonly LoginDbContext _context;

    public AccountService(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDto?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        AccountEntity? entity = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username == username, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    /// <summary>
    /// Valida usuario + hash de contraseña (SHA-512 hex) y que la cuenta no
    /// esté baneada. Toda la lógica de "¿puede entrar o no?" vive acá, no en
    /// el LoginSession.
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string username, string passwordHashHex, CancellationToken cancellationToken = default)
    {
        AccountDto? account = await FindByUsernameAsync(username, cancellationToken);

        return account is not null
            && !account.IsBanned
            && string.Equals(account.Password, passwordHashHex, StringComparison.OrdinalIgnoreCase);
    }

    /// <returns>true si se creó la cuenta; false si el usuario ya existía.</returns>
    public async Task<bool> CreateAccountAsync(string username, string passwordHashHex, CancellationToken cancellationToken = default)
    {
        bool exists = await _context.Accounts.AnyAsync(a => a.Username == username, cancellationToken);
        if (exists)
        {
            return false;
        }

        _context.Accounts.Add(new AccountEntity
        {
            Username = username,
            Password = passwordHashHex
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AccountDto ToDto(AccountEntity entity) => new()
    {
        Id = entity.Id,
        Username = entity.Username,
        Password = entity.Password,
        Authority = entity.Authority,
        IsBanned = entity.IsBanned
    };
}
