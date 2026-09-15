using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using api_gestion_productos.Models;
using api_gestion_productos.Services;
using Microsoft.Extensions.Configuration;

namespace api_gestion_productos.Tests;

public class PaginationTests
{
    [Theory]
    [InlineData(0, 20, 1, 20)]
    [InlineData(-3, 20, 1, 20)]
    [InlineData(2, 0, 2, 20)]
    [InlineData(1, 500, 1, 100)]
    [InlineData(3, 10, 3, 10)]
    public void Normalize_ClampsInvalidValues(int page, int size, int expPage, int expSize)
    {
        var (p, s) = Pagination.Normalize(page, size);
        Assert.Equal(expPage, p);
        Assert.Equal(expSize, s);
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(95, 20, 5)]
    [InlineData(100, 20, 5)]
    [InlineData(101, 20, 6)]
    public void TotalPages_Ceils(int total, int size, int expPages)
    {
        var r = new PagedResult<ProductResponseDto> { total = total, page = 1, pageSize = size };
        Assert.Equal(expPages, r.totalPages);
    }
}

public class TokenServiceTests
{
    private static TokenService BuildService(Dictionary<string, string?> values)
        => new(new ConfigurationBuilder().AddInMemoryCollection(values!).Build());

    [Fact]
    public void GenerateToken_Throws_WhenKeyMissingOrShort()
    {
        var svcNoKey = BuildService(new()
        {
            ["Jwt:Issuer"] = "iss", ["Jwt:Audience"] = "aud", ["Jwt:ExpiryMinutes"] = "60",
        });
        Assert.Throws<InvalidOperationException>(() => svcNoKey.GenerateToken(1, "A B", "a@b.c", "USER"));

        var svcShort = BuildService(new()
        {
            ["Jwt:Key"] = "corta", ["Jwt:Issuer"] = "iss", ["Jwt:Audience"] = "aud",
        });
        Assert.Throws<InvalidOperationException>(() => svcShort.GenerateToken(1, "A B", "a@b.c", "USER"));
    }

    [Fact]
    public void GenerateToken_ContainsExpectedClaimsAndExpiry()
    {
        var svc = BuildService(new()
        {
            ["Jwt:Key"] = new string('k', 64),
            ["Jwt:Issuer"] = "test-iss",
            ["Jwt:Audience"] = "test-aud",
            ["Jwt:ExpiryMinutes"] = "60",
        });

        var jwt = svc.GenerateToken(42, "Ada Lovelace", "ada@x.com", "USER");
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(jwt));

        var token = handler.ReadJwtToken(jwt);
        Assert.Equal("test-iss", token.Issuer);
        Assert.Contains(token.Claims, c => (c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid") && c.Value == "42");
        Assert.Contains(token.Claims, c => (c.Type == ClaimTypes.Email || c.Type == "email") && c.Value == "ada@x.com");
        Assert.Contains(token.Claims, c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == "USER");
        Assert.True(token.ValidTo > DateTime.UtcNow.AddMinutes(50));
    }

    [Fact]
    public void RefreshToken_IsUniqueAndOpaque()
    {
        var svc = BuildService(new() { ["Jwt:Key"] = new string('k', 64) });
        var a = svc.GenerateRefreshToken();
        var b = svc.GenerateRefreshToken();
        Assert.NotEqual(a, b);
        Assert.DoesNotContain("==", a + b);
    }

    [Fact]
    public void HashRefreshToken_IsDeterministicAndHidesPlaintext()
    {
        var svc = BuildService(new() { ["Jwt:Key"] = new string('k', 64) });
        var h1 = svc.HashRefreshToken("abc");
        var h2 = svc.HashRefreshToken("abc");
        var h3 = svc.HashRefreshToken("abd");
        Assert.Equal(h1, h2);
        Assert.NotEqual(h1, h3);
        Assert.Equal(64, h1.Length);
        Assert.DoesNotContain("abc", h1);
    }

    [Theory]
    [InlineData(null, 7)]
    [InlineData(0, 7)]
    [InlineData(-5, 7)]
    [InlineData(14, 14)]
    [InlineData(99, 30)]
    public void RefreshExpiryDays_Clamps(int? configured, int expected)
    {
        var values = new Dictionary<string, string?> { ["Jwt:Key"] = new string('k', 64) };
        if (configured is not null) values["Jwt:RefreshExpiryDays"] = configured.ToString();
        Assert.Equal(expected, BuildService(values).GetRefreshExpiryDays());
    }
}
