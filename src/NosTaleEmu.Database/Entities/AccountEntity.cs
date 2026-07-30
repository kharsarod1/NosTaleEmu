namespace NosTaleEmu.Database.Entities;

public sealed class AccountEntity
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;

    // SHA-512(password) en hex — nunca texto plano.
    public string Password { get; set; } = string.Empty;

    public byte Authority { get; set; }
    public bool IsBanned { get; set; }
}
