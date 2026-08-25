using System.ComponentModel.DataAnnotations;
using GLMS.Web.Models;

namespace GLMS.Web.Models
{
    // Represents a service contract between a client and the company
    public class Contract
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; } // Navigation property back to the client

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } = "Draft"; // Draft, Active, Expired, Terminated

        [Required]
        public string ServiceLevel { get; set; } = string.Empty; // e.g., "Gold", "Silver", "Bronze"

        public string? SignedAgreementPath { get; set; } // File path to the signed PDF

        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}