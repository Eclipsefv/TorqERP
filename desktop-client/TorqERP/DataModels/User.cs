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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        ADMIN,
        MANAGER,
        USER
    }

    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]

        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string? Password { get; set; }

        public UserRole Role { get; set; } = UserRole.USER;

        public DateTime CreationDate { get; set; }

        public DateTime LastLogin { get; set; }
    }
}