using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorqERP.DataModels
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Sku { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ProductType Type { get; set; } = ProductType.ITEM;

        public decimal BuyPrice { get; set; } = 0.0m;

        public decimal SellPrice { get; set; } = 0.0m;

        public decimal TaxRate { get; set; } = 21.0m;
        public float? Stock { get; set; } = 0f;

        public float? MinStock { get; set; } = 0f;

        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; }
    }

    public enum ProductType
    {
        ITEM,
        SERVICE
    }
}
