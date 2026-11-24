using Dapper;
using Microsoft.Data.SqlClient;
using PharmaOnline.Models;

namespace PharmaOnline.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found.");
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    public async Task<int> PlaceOrderAsync(Order order, List<OrderItem> items)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        try
        {
            var orderId = await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO Orders (UserId, TotalAmount, Status, OrderDate, DeliveryAddress)
                  VALUES (@UserId, @TotalAmount, @Status, @OrderDate, @DeliveryAddress);
                  SELECT SCOPE_IDENTITY();",
                order, tx);

            foreach (var item in items)
            {
                item.OrderId = orderId;
                await conn.ExecuteAsync(
                    @"INSERT INTO OrderItems (OrderId, MedicineId, Quantity, UnitPrice)
                      VALUES (@OrderId, @MedicineId, @Quantity, @UnitPrice);
                      UPDATE Medicines SET Stock = Stock - @Quantity WHERE Id = @MedicineId;",
                    item, tx);
            }

            await tx.CommitAsync();
            return orderId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Order>(
            @"SELECT o.*, u.FullName AS UserFullName
              FROM Orders o
              LEFT JOIN AspNetUsers u ON o.UserId = u.Id
              WHERE o.UserId = @UserId
              ORDER BY o.OrderDate DESC",
            new { UserId = userId });
    }

    public async Task<Order?> GetOrderDetailAsync(int orderId)
    {
        using var conn = CreateConnection();

        var order = await conn.QueryFirstOrDefaultAsync<Order>(
            "SELECT * FROM Orders WHERE Id = @Id", new { Id = orderId });

        if (order != null)
        {
            var items = await conn.QueryAsync<OrderItem>(
                @"SELECT oi.*, m.Name AS MedicineName
                  FROM OrderItems oi
                  INNER JOIN Medicines m ON oi.MedicineId = m.Id
                  WHERE oi.OrderId = @OrderId",
                new { OrderId = orderId });
            order.Items = items.ToList();
        }

        return order;
    }

    public async Task UpdateOrderStatusAsync(int orderId, string status)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Orders SET Status = @Status WHERE Id = @Id",
            new { Status = status, Id = orderId });
    }
}
