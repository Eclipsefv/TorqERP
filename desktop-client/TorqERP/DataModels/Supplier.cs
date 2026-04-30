using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TorqERP.DataModels
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        public string Cif { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Notes { get; set; }
        public bool Active { get; set; } = true;

        public List<DeliveryNote> DeliveryNotes { get; set; } = new();

        [JsonPropertyName("_count")]
        public SupplierCount? Count { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class SupplierCount
    {
        public int DeliveryNotes { get; set; }
    }

    public class DeliveryNote
    {
        public int Id { get; set; }

        public string InternalNumber { get; set; } = string.Empty;

        [Required]
        public string SupplierNoteNumber { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public List<DeliveryNoteLine> Lines { get; set; } = new();

        [JsonPropertyName("_count")]
        public DeliveryNoteCount? Count { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Subtotal { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal TaxTotal { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class DeliveryNoteCount
    {
        public int Lines { get; set; }
    }

    public class DeliveryNoteLine
    {
        public int Id { get; set; }

        public float Quantity { get; set; } = 1f;

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal UnitCost { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal TaxRate { get; set; } = 21.0m;

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Discount { get; set; } = 0.0m;

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal LineTotal { get; set; }

        public int DeliveryNoteId { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}