using Dapper;
using Microsoft.Data.SqlClient;
using PharmaOnline.Models;

namespace PharmaOnline.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly string _connectionString;

    public UserProfileRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found.");
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    public async Task<ApplicationUser?> GetProfileAsync(string userId)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ApplicationUser>(
            @"SELECT Id, UserName, Email, PhoneNumber, FullName, Address, RegisteredAt
              FROM AspNetUsers WHERE Id = @Id",
            new { Id = userId });
    }

    public async Task UpdateProfileAsync(ApplicationUser user)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE AspNetUsers
              SET FullName = @FullName,
                  PhoneNumber = @PhoneNumber,
                  Address = @Address
              WHERE Id = @Id",
            new { user.FullName, user.PhoneNumber, user.Address, user.Id });
    }
}
