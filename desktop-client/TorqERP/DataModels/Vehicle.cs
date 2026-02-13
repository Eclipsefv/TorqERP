using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorqERP.DataModels
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Plate { get; set; } = string.Empty;

        public string? Brand { get; set; }

        public string? Model { get; set; }

        public int? Year { get; set; }

        [Required]
        public int CustomerId { get; set; }

        //To reference Customer Id so I can navigate ez through vehicle
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
