using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// Code Attribution
// Title: Make HTTP requests using IHttpClientFactory in ASP.NET Core
// Author: Microsoft
// Date: 2026
// Availability: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests

namespace GLMS.Web.Controllers
{
    // MVC Controller — NO database access. All data comes from the API via HttpClient.
    public class ContractsController : Controller
    {
        private readonly ApiService _api;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public ContractsController(ApiService api, IWebHostEnvironment env, IConfiguration config)
        {
            _api = api;
            _env = env;
            _config = config;
        }

        public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate)
        {
            var url = "api/contracts";
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
            if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

            var contracts = await _api.GetAsync<List<Contract>>(url) ?? new List<Contract>();
            ViewBag.Status = status;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View(contracts);
        }

        public async Task<IActionResult> Create()
        {
            // FIX: asp-items requires SelectList, not List<Client>
            var clients = await _api.GetAsync<List<Client>>("api/clients") ?? new List<Client>();
            ViewBag.Clients = new SelectList(clients, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, IFormFile? pdfFile)
        {
            // Handle file upload locally (file server simulation)
            if (pdfFile != null)
            {
                if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                {
                    ModelState.AddModelError("", "Only PDF files are allowed.");
                    var clientsForError = await _api.GetAsync<List<Client>>("api/clients") ?? new();
                    ViewBag.Clients = new SelectList(clientsForError, "Id", "Name");
                    return View(contract);
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + ".pdf";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await pdfFile.CopyToAsync(stream);

                contract.SignedAgreementPath = uniqueFileName;
            }

            var response = await _api.PostAsync("api/contracts", contract);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Failed to create contract. Please try again.");
            var clientsOnError = await _api.GetAsync<List<Client>>("api/clients") ?? new();
            ViewBag.Clients = new SelectList(clientsOnError, "Id", "Name");
            return View(contract);
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _api.GetAsync<Contract>($"api/contracts/{id}");
            if (contract == null) return NotFound();
            return View(contract);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _api.GetAsync<Contract>($"api/contracts/{id}");
            if (contract == null) return NotFound();

            // FIX: asp-items requires SelectList, not List<Client>
            var clients = await _api.GetAsync<List<Client>>("api/clients") ?? new List<Client>();
            ViewBag.Clients = new SelectList(clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.Id) return BadRequest();
            var response = await _api.PutAsync($"api/contracts/{id}", contract);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Failed to update contract.");
            var clients = await _api.GetAsync<List<Client>>("api/clients") ?? new List<Client>();
            ViewBag.Clients = new SelectList(clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var response = await _api.PatchAsync($"api/contracts/{id}/status", new { status = newStatus });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Status update failed: {error}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteAsync($"api/contracts/{id}");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound();
            return PhysicalFile(filePath, "application/pdf", fileName);
        }
    }
}