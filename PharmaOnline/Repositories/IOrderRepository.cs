using PharmaOnline.Models;

namespace PharmaOnline.Repositories;

public interface IOrderRepository
{
    Task<int> PlaceOrderAsync(Order order, List<OrderItem> items);
    Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId);
    Task<Order?> GetOrderDetailAsync(int orderId);
    Task UpdateOrderStatusAsync(int orderId, string status);
}
