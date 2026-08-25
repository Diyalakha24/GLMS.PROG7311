using GLMS.API.Models;

namespace GLMS.API.Repositories
{
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllAsync(string? status, DateTime? startDate, DateTime? endDate);
        Task<Contract?> GetByIdAsync(int id);
        Task<Contract> CreateAsync(Contract contract);
        Task<bool> UpdateAsync(Contract contract);   // FIX: added
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);
    }
}