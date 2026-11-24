using Dapper;
using Microsoft.Data.SqlClient;
using PharmaOnline.Models;

namespace PharmaOnline.Repositories;

public class CartRepository : ICartRepository
{
    private readonly string _connectionString;

    public CartRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found.");
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<CartItem>> GetCartByUserAsync(string userId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<CartItem>(
            @"SELECT c.Id, c.MedicineId, c.UserId, c.Quantity, c.AddedAt,
                     m.Name AS MedicineName, m.Price AS MedicinePrice,
                     m.ImageUrl AS MedicineImageUrl, m.ExpiryDate AS MedicineExpiryDate
              FROM CartItems c
              INNER JOIN Medicines m ON c.MedicineId = m.Id
              WHERE c.UserId = @UserId
              ORDER BY c.AddedAt DESC",
            new { UserId = userId });
    }

    public async Task<CartItem?> GetCartItemAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<CartItem>(
            "SELECT * FROM CartItems WHERE Id = @Id", new { Id = id });
    }

    public async Task AddToCartAsync(CartItem item)
    {
        using var conn = CreateConnection();
        var existing = await conn.QueryFirstOrDefaultAsync<CartItem>(
            "SELECT * FROM CartItems WHERE UserId = @UserId AND MedicineId = @MedicineId",
            new { item.UserId, item.MedicineId });

        if (existing != null)
        {
            await conn.ExecuteAsync(
                "UPDATE CartItems SET Quantity = Quantity + @Qty WHERE Id = @Id",
                new { Qty = item.Quantity, existing.Id });
        }
        else
        {
            await conn.ExecuteAsync(
                @"INSERT INTO CartItems (MedicineId, UserId, Quantity, AddedAt)
                  VALUES (@MedicineId, @UserId, @Quantity, @AddedAt)",
                item);
        }
    }

    public async Task UpdateQuantityAsync(int id, int quantity)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE CartItems SET Quantity = @Quantity WHERE Id = @Id",
            new { Quantity = quantity, Id = id });
    }

    public async Task RemoveFromCartAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("DELETE FROM CartItems WHERE Id = @Id", new { Id = id });
    }

    public async Task ClearCartAsync(string userId)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM CartItems WHERE UserId = @UserId", new { UserId = userId });
    }

    public async Task<int> GetCartCountAsync(string userId)
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT ISNULL(SUM(Quantity), 0) FROM CartItems WHERE UserId = @UserId",
            new { UserId = userId });
    }
}
