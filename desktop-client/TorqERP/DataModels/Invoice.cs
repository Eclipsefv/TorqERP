using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TorqERP.DataModels
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;

        public InvoiceStatus Status { get; set; } = InvoiceStatus.DRAFT;

        public decimal Subtotal { get; set; } = 0.0m;

        public decimal TaxTotal { get; set; } = 0.0m;

        public decimal Total { get; set; } = 0.0m;

        [Required]
        public int CustomerId { get; set; }

        public int? WorkOrderId { get; set; }

        public List<InvoiceLine> Lines { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }
    }

    public class InvoiceLine
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public float Quantity { get; set; } = 1f;

        public decimal UnitPrice { get; set; } = 0.0m;

        public decimal TaxRate { get; set; } = 21.0m;

        public decimal Total { get; set; } = 0.0m;

        public int InvoiceId { get; set; }

        public int? ProductId { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InvoiceStatus
    {
        DRAFT,
        ISSUED,
        PAID,
        CANCELLED
    }
}
