using PharmaOnline.Models;

namespace PharmaOnline.Repositories;

public interface ICartRepository
{
    Task<IEnumerable<CartItem>> GetCartByUserAsync(string userId);
    Task<CartItem?> GetCartItemAsync(int id);
    Task AddToCartAsync(CartItem item);
    Task UpdateQuantityAsync(int id, int quantity);
    Task RemoveFromCartAsync(int id);
    Task ClearCartAsync(string userId);
    Task<int> GetCartCountAsync(string userId);
}
