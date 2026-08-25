using GLMS.Web.Models;

namespace GLMS.Web.Services
{

    // Code Attribution
    // Title: Strategy Pattern
    // Author: Refactoring Guru
    // Date: 2026
    // Availability: https://refactoring.guru/design-patterns/strategy

    public interface IContractFactory
    {
        Contract CreateContract(int clientId, DateTime startDate, DateTime endDate, string status);
    }

    public class StandardContractFactory : IContractFactory
    {
        public Contract CreateContract(int clientId, DateTime startDate, DateTime endDate, string status)
        {
            return new Contract { ClientId = clientId, StartDate = startDate, EndDate = endDate, Status = status, ServiceLevel = "Standard" };
        }
    }

    public class PremiumContractFactory : IContractFactory
    {
        public Contract CreateContract(int clientId, DateTime startDate, DateTime endDate, string status)
        {
            return new Contract { ClientId = clientId, StartDate = startDate, EndDate = endDate, Status = status, ServiceLevel = "Premium" };
        }
    }
}