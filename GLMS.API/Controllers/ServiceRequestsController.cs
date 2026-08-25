using GLMS.API.Data;
using GLMS.API.Models;
using GLMS.API.Repositories;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestRepository _repo;
        private readonly AppDbContext _context;
        private readonly ContractValidationService _validator;
        private readonly CurrencyService _currencyService;

        public ServiceRequestsController(
            IServiceRequestRepository repo,
            AppDbContext context,
            ContractValidationService validator,
            CurrencyService currencyService)
        {
            _repo = repo;
            _context = context;
            _validator = validator;
            _currencyService = currencyService;
        }

        // GET /api/servicerequests
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _repo.GetAllAsync();
            return Ok(requests);
        }

        // GET /api/servicerequests/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _repo.GetByIdAsync(id);
            if (request == null) return NotFound(new { message = $"ServiceRequest {id} not found" });
            return Ok(request);
        }

        // POST /api/servicerequests
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceRequest request)
        {
            // Look up the contract
            var contract = await _context.Contracts.FindAsync(request.ContractId);
            if (contract == null)
                return BadRequest(new { message = "Contract not found." });

            // Validation logic is in the service — NOT in the controller
            if (!_validator.CanCreateServiceRequest(contract))
                return BadRequest(new { message = $"Cannot raise a request on a contract with status '{contract.Status}'." });

            // Currency conversion
            var rate = await _currencyService.GetUsdToZarRateAsync();
            request.CostZAR = Math.Round(request.CostUSD * rate, 2);

            var created = await _repo.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // DELETE /api/servicerequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound(new { message = $"ServiceRequest {id} not found" });
            return NoContent();
        }
    }
}