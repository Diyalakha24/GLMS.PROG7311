using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using GLMS.API.Repositories;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Code Attribution
// Title: Create web APIs with ASP.NET Core
// Author: Microsoft
// Date: 2026
// Availability: https://learn.microsoft.com/en-us/aspnet/core/web-api/

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require valid JWT
    public class ContractsController : ControllerBase
    {
        private readonly IContractRepository _repo;
        private readonly ContractValidationService _validator;

        public ContractsController(IContractRepository repo, ContractValidationService validator)
        {
            _repo = repo;
            _validator = validator;
        }

        // GET /api/contracts?status=Active&startDate=2025-01-01&endDate=2025-12-31
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var contracts = await _repo.GetAllAsync(status, startDate, endDate);
            return Ok(contracts);
        }

        // GET /api/contracts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contract = await _repo.GetByIdAsync(id);
            if (contract == null) return NotFound(new { message = $"Contract {id} not found" });
            return Ok(contract);
        }

        // POST /api/contracts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Contract contract)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var created = await _repo.CreateAsync(contract);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/contracts/5 — FIX: MVC Edit form calls PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Contract contract)
        {
            if (id != contract.Id)
                return BadRequest(new { message = "ID mismatch" });
            var updated = await _repo.UpdateAsync(contract);
            if (!updated) return NotFound(new { message = $"Contract {id} not found" });
            return NoContent();
        }

        // PATCH /api/contracts/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new { message = $"Contract {id} not found" });

            if (!_validator.IsValidStatusTransition(existing.Status, dto.Status))
            {
                return BadRequest(new
                {
                    message = $"Cannot transition from '{existing.Status}' to '{dto.Status}'"
                });
            }

            await _repo.UpdateStatusAsync(id, dto.Status);
            return NoContent();
        }

        // DELETE /api/contracts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound(new { message = $"Contract {id} not found" });
            return NoContent();
        }
    }
}