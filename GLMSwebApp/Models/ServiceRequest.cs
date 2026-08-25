namespace GLMS.Web.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    public int ContractId { get; set; }
    public Contract? Contract { get; set; } // Which contract this request belongs to

    // These come from the related Contract, not stored directly in this table
    [NotMapped]
    public int ClientId { get; set; }

    [NotMapped]
    public string ContractType { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty; // What service is being requested

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostUSD { get; set; } // Original cost in USD

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostZAR { get; set; } // Converted amount in ZAR

    [Required]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Completed, Rejected
}