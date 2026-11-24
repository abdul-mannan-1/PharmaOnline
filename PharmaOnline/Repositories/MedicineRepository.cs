using Dapper;
using Microsoft.Data.SqlClient;
using PharmaOnline.Models;

namespace PharmaOnline.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private readonly string _connectionString;

    public MedicineRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found.");
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<Medicine>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Medicine>(
            "SELECT * FROM Medicines ORDER BY Name");
    }

    public async Task<Medicine?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Medicine>(
            "SELECT * FROM Medicines WHERE Id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<Medicine>> SearchAsync(string query)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Medicine>(
            @"SELECT * FROM Medicines
              WHERE Name LIKE @q OR GenericName LIKE @q OR Category LIKE @q
              ORDER BY Name",
            new { q = $"%{query}%" });
    }

    public async Task<IEnumerable<Medicine>> GetByCategoryAsync(string category)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Medicine>(
            "SELECT * FROM Medicines WHERE Category = @Category ORDER BY Name",
            new { Category = category });
    }

    public async Task<int> AddAsync(Medicine medicine)
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Medicines (Name, GenericName, Price, Stock, ExpiryDate, ImageUrl, Category, Description)
              VALUES (@Name, @GenericName, @Price, @Stock, @ExpiryDate, @ImageUrl, @Category, @Description);
              SELECT SCOPE_IDENTITY();",
            medicine);
    }

    public async Task UpdateAsync(Medicine medicine)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE Medicines SET
                Name = @Name,
                GenericName = @GenericName,
                Price = @Price,
                Stock = @Stock,
                ExpiryDate = @ExpiryDate,
                ImageUrl = @ImageUrl,
                Category = @Category,
                Description = @Description
              WHERE Id = @Id",
            medicine);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Medicines WHERE Id = @Id", new { Id = id });
    }
}
