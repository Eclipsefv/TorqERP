using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

        public Customer? Customer { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        public string? Description { get; set; }

        public string Status { get; set; } = "SCHEDULED";

        [NotMapped]
        [JsonIgnore]
        public string DisplayClient =>
            Customer?.Name ?? Vehicle?.Customer?.Name ?? "N/A";

        [NotMapped]
        [JsonIgnore]
        public string DisplayVehicle =>
            Vehicle != null ? $"{Vehicle.Plate} ({Vehicle.Brand} {Vehicle.Model})" : "N/A";

        [NotMapped]
        [JsonIgnore]
        public string DisplayTime => ScheduledAt.ToString("HH:mm");

        [NotMapped]
        [JsonIgnore]
        public string DisplayDate => ScheduledAt.ToString("dd/MM/yyyy");
    }
}















