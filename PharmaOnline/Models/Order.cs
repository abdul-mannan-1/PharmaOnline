using System.ComponentModel.DataAnnotations;

namespace PharmaOnline.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;

    public List<OrderItem> Items { get; set; } = new();
    public string? UserFullName { get; set; }
}
