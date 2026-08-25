using GLMS.API.Models;
using GLMS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _repo;

        public ClientsController(IClientRepository repo)
        {
            _repo = repo;
        }

        // GET /api/clients
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var clients = await _repo.GetAllAsync();
            return Ok(clients);
        }

        // GET /api/clients/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var client = await _repo.GetByIdAsync(id);
            if (client == null) return NotFound(new { message = $"Client {id} not found" });
            return Ok(client);
        }

        // POST /api/clients
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Client client)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var created = await _repo.CreateAsync(client);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Client client)
        {
            if (id != client.Id)
                return BadRequest(new { message = "ID mismatch" });
            var updated = await _repo.UpdateAsync(client);
            if (!updated) return NotFound(new { message = $"Client {id} not found" });
            return NoContent();
        }

        // PATCH /api/clients/5  — FIX: MVC calls PATCH for Edit, so we need this endpoint
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(int id, [FromBody] Client client)
        {
            client.Id = id; // Ensure the ID is set from the route
            var updated = await _repo.UpdateAsync(client);
            if (!updated) return NotFound(new { message = $"Client {id} not found" });
            return NoContent();
        }

        // DELETE /api/clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound(new { message = $"Client {id} not found" });
            return NoContent();
        }
    }
}