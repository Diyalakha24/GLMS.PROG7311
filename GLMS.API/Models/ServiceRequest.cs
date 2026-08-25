using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.API.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostUSD { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostZAR { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";
    }
}