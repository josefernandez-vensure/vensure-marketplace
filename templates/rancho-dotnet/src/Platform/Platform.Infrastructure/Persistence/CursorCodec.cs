using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Platform.Application.Abstractions;

namespace Platform.Infrastructure.Persistence;

/// <summary>The only type that turns a cursor into a token and back: encrypted and authenticated, under the query's own purpose.</summary>
internal sealed class CursorCodec(IDataProtectionProvider protection) : ICursorCodec
{
    private const string Purpose = "Platform.PageCursor";

    public string Seal(PageCursor cursor, string purpose) =>
        protection.CreateProtector(Purpose, purpose).Protect(JsonSerializer.Serialize(cursor));

    public PageCursor? Open(string token, string purpose)
    {
        try
        {
            return JsonSerializer.Deserialize<PageCursor>(protection.CreateProtector(Purpose, purpose).Unprotect(token));
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
