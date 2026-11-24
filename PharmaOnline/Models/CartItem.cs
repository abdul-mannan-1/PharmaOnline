namespace PharmaOnline.Models;

public class CartItem
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public string? MedicineName { get; set; }
    public decimal? MedicinePrice { get; set; }
    public string? MedicineImageUrl { get; set; }
    public DateTime? MedicineExpiryDate { get; set; }
}
