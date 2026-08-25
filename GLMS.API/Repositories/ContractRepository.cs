using GLMS.API.Data;
using GLMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Repositories
{
    // Repository pattern: isolates DB queries from the API controllers
    public class ContractRepository : IContractRepository
    {
        private readonly AppDbContext _context;

        public ContractRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync(string? status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);
            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            return await query.ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Contract> CreateAsync(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        // FIX: Added UpdateAsync — required for the new PUT /api/contracts/{id} endpoint
        public async Task<bool> UpdateAsync(Contract contract)
        {
            var existing = await _context.Contracts.FindAsync(contract.Id);
            if (existing == null) return false;

            existing.ClientId = contract.ClientId;
            existing.StartDate = contract.StartDate;
            existing.EndDate = contract.EndDate;
            existing.Status = contract.Status;
            existing.ServiceLevel = contract.ServiceLevel;

            // Only update path if a new file was provided
            if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                existing.SignedAgreementPath = contract.SignedAgreementPath;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return false;
            contract.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return false;
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}