namespace GLMS.Web.Services
{

    // Code Attribution
    // Title: Strategy Pattern
    // Author: Refactoring Guru
    // Date: 2026
    // Availability: https://refactoring.guru/design-patterns/strategy

    public interface IContractObserver
    {
        void Update(string contractStatus, int contractId);
    }

    public class ManagerNotification : IContractObserver
    {
        public void Update(string contractStatus, int contractId)
        {
            Console.WriteLine($"[Manager] Contract {contractId} is now {contractStatus}");
        }
    }

    public class ComplianceSystem : IContractObserver
    {
        public void Update(string contractStatus, int contractId)
        {
            Console.WriteLine($"[Compliance] Contract {contractId} flagged: {contractStatus}");
        }
    }
}