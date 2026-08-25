using GLMS.API.Models;

namespace GLMS.API.Repositories
{
    public interface IServiceRequestRepository
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task<ServiceRequest> CreateAsync(ServiceRequest request);
        Task<bool> DeleteAsync(int id);
    }
}