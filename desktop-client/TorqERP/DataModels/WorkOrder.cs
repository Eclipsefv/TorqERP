using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorqERP.DataModels
{
    public class WorkOrder
    {
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        public string? Description { get; set; }

        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.PENDING;

        [Required]
        public int VehicleId { get; set; }

        public List<WorkOrderLine> Lines { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

    public class WorkOrderLine
    {
        public int Id { get; set; }

        public float Quantity { get; set; } = 1f;

        public decimal Price { get; set; } = 0.0m;

        public decimal Discount { get; set; } = 0.0m;

        public int WorkOrderId { get; set; }

        [Required]
        public int ProductId { get; set; }
    }

    public enum WorkOrderStatus
    {
        PENDING,
        IN_PROGRESS,
        COMPLETED,
        CANCELLED
    }
}
