using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorqERP.DataModels
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Nif { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Adress { get; set; }

        public string? Phonenumber { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
