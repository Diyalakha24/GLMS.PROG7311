using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApiService _api;
        private readonly CurrencyService _currencyService;

        public ServiceRequestsController(ApiService api, CurrencyService currencyService)
        {
            _api = api;
            _currencyService = currencyService;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _api.GetAsync<List<ServiceRequest>>("api/servicerequests")
                ?? new List<ServiceRequest>();
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCreateFormDataAsync();
            return View(new ServiceRequest { Status = "Pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateFormDataAsync(request.ContractId);
                return View(request);
            }

            var rate = await _currencyService.GetUsdToZarRateAsync();
            request.CostZAR = Math.Round(request.CostUSD * rate, 2);

            var response = await _api.PostAsync("api/servicerequests", request);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var errorBody = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(errorBody)
                    ? "The service request could not be created."
                    : $"Error: {errorBody}");

            await LoadCreateFormDataAsync(request.ContractId);
            return View(request);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _api.GetAsync<ServiceRequest>($"api/servicerequests/{id}");
            if (request == null) return NotFound();
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteAsync($"api/servicerequests/{id}");
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCreateFormDataAsync(int? selectedContractId = null)
        {
            var clients = await _api.GetAsync<List<Client>>("api/clients") ?? new List<Client>();
            var contracts = await _api.GetAsync<List<Contract>>("api/contracts") ?? new List<Contract>();

            // Only show Active contracts — can't raise a request on Expired/Draft/On Hold
            var activeContracts = contracts.Where(c => c.Status == "Active").ToList();

            // Build a meaningful label: "ClientName — ServiceLevel (Status)"
            var contractItems = activeContracts.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Client?.Name ?? "Client #" + c.ClientId} — {c.ServiceLevel} (Active)",
                // Store clientId as a data attribute via the group name trick
            }).ToList();

            ViewBag.Clients = new SelectList(clients, nameof(Client.Id), nameof(Client.Name));
            ViewBag.Contracts = activeContracts; // Pass full list so JS can filter by clientId
            ViewBag.ContractItems = contractItems;
            ViewBag.SelectedContractId = selectedContractId;
            ViewBag.Rate = await _currencyService.GetUsdToZarRateAsync();
        }
    }
}