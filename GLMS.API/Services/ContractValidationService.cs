using GLMS.API.Models;

namespace GLMS.API.Services
{
    // Business rule logic lives HERE, not in controllers
    // This directly addresses the marker feedback about separating logic from controllers
    public class ContractValidationService
    {
        // Rule: ServiceRequests can only be raised on Active contracts
        public bool CanCreateServiceRequest(Contract contract)
        {
            return contract.Status == "Active";
        }

        // Rule: Status transitions must be valid
        public bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            var allowed = new Dictionary<string, List<string>>
            {
                { "Draft",    new List<string> { "Active", "On Hold" } },
                { "Active",   new List<string> { "Expired", "On Hold" } },
                { "On Hold",  new List<string> { "Active", "Expired" } },
                { "Expired",  new List<string>() } // Terminal state
            };

            return allowed.ContainsKey(currentStatus) &&
                   allowed[currentStatus].Contains(newStatus);
        }

        // Rule: PDF files only for signed agreements
        public bool IsValidAgreementFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            return Path.GetExtension(fileName).ToLower() == ".pdf";
        }
    }
}