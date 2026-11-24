using System.ComponentModel.DataAnnotations;

namespace PharmaOnline.Models;

public class Medicine
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string GenericName { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime ExpiryDate { get; set; }

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}
