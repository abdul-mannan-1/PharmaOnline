using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaOnline.Models;
using PharmaOnline.Repositories;
using System.Security.Claims;

namespace PharmaOnline.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cart;

        public CartController(ICartRepository cart)
        {
            _cart = cart;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetItems()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var items = await _cart.GetCartByUserAsync(userId);
            return Json(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int medicineId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var item = new CartItem
            {
                MedicineId = medicineId,
                UserId = userId,
                Quantity = quantity,
                AddedAt = DateTime.UtcNow
            };
            await _cart.AddToCartAsync(item);
            var count = await _cart.GetCartCountAsync(userId);
            return Json(new { success = true, cartCount = count });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            if (quantity < 1)
            {
                await _cart.RemoveFromCartAsync(id);
            }
            else
            {
                await _cart.UpdateQuantityAsync(id, quantity);
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            await _cart.RemoveFromCartAsync(id);
            return Json(new { success = true });
        }

        public async Task<IActionResult> GetCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var count = await _cart.GetCartCountAsync(userId);
            return Json(new { count });
        }
    }
}
