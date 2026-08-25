using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    // MVC Client Controller — uses ApiService, zero direct DB access
    public class ClientsController : Controller
    {
        private readonly ApiService _api;

        public ClientsController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var clients = await _api.GetAsync<List<Client>>("api/clients") ?? new List<Client>();
            return View(clients);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            var response = await _api.PostAsync("api/clients", client);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Failed to create client.");
            return View(client);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = await _api.GetAsync<Client>($"api/clients/{id}");
            if (client == null) return NotFound();
            return View(client);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _api.GetAsync<Client>($"api/clients/{id}");
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return BadRequest();
            // FIX: Use PutAsync instead of PatchAsync — API has PUT /api/clients/{id}
            var response = await _api.PutAsync($"api/clients/{id}", client);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Failed to update client.");
            return View(client);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteAsync($"api/clients/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}