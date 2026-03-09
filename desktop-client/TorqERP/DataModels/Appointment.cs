using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TorqERP.DataModels
{
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public int CustomerId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        public string? Description { get; set; }

        public string Status { get; set; } = "SCHEDULED";

        [JsonIgnore]
        public string DisplayClient => Vehicle?.Customer?.Name ?? "N/A";

        [JsonIgnore]
        public string DisplayVehicle => Vehicle != null ? $"{Vehicle.Plate} ({Vehicle.Model})" : "N/A";
    }
}
