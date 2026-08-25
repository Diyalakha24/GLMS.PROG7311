using GLMS.API.Data;
using GLMS.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace GLMS.Web.Tests
{
    [TestFixture]
    public class UnitTests
    {
        private decimal ConvertUsdToZar(decimal usdAmount, decimal rate)
        {
            return Math.Round(usdAmount * rate, 2);
        }

        private bool IsValidPdfFile(IFormFile? file)
        {
            if (file == null)
            {
                return false;
            }

            return Path.GetExtension(file.FileName)
                .Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private bool CanCreateServiceRequest(Contract? contract)
        {
            if (contract == null)
            {
                return false;
            }

            return contract.Status == "Active";
        }

        private Mock<IFormFile> CreateMockFile(string fileName)
        {
            var mockFile = new Mock<IFormFile>();

            mockFile
                .Setup(file => file.FileName)
                .Returns(fileName);

            mockFile
                .Setup(file => file.Length)
                .Returns(1024);

            return mockFile;
        }

        [Test]
        public void CurrencyConversion_CorrectResult_WithKnownRate()
        {
            var result = ConvertUsdToZar(100m, 18.5m);

            Assert.That(result, Is.EqualTo(1850.00m));
        }

        [Test]
        public void CurrencyConversion_ZeroAmount_ReturnsZero()
        {
            var result = ConvertUsdToZar(0m, 18.5m);

            Assert.That(result, Is.EqualTo(0.00m));
        }

        [Test]
        public void CurrencyConversion_RoundsToTwoDecimalPlaces()
        {
            var result = ConvertUsdToZar(10m, 18.333m);

            Assert.That(result, Is.EqualTo(183.33m));
        }

        [Test]
        public void CurrencyConversion_LargeAmount_IsCorrect()
        {
            var result = ConvertUsdToZar(1000m, 18.5m);

            Assert.That(result, Is.EqualTo(18500.00m));
        }

        [Test]
        public void CurrencyConversion_NegativeRate_StillCalculates()
        {
            var result = ConvertUsdToZar(100m, 0m);

            Assert.That(result, Is.EqualTo(0.00m));
        }

        [Test]
        public void FileValidation_PdfFile_IsAllowed()
        {
            var file = CreateMockFile("contract.pdf").Object;

            Assert.That(IsValidPdfFile(file), Is.True);
        }

        [Test]
        public void FileValidation_ExeFile_IsRejected()
        {
            var file = CreateMockFile("malicious.exe").Object;

            Assert.That(IsValidPdfFile(file), Is.False);
        }

        [Test]
        public void FileValidation_NullFile_IsRejected()
        {
            Assert.That(IsValidPdfFile(null), Is.False);
        }

        [Test]
        public void FileValidation_DocxFile_IsRejected()
        {
            var file = CreateMockFile("document.docx").Object;

            Assert.That(IsValidPdfFile(file), Is.False);
        }

        [Test]
        public void FileValidation_PdfUppercase_IsAllowed()
        {
            var file = CreateMockFile("CONTRACT.PDF").Object;

            Assert.That(IsValidPdfFile(file), Is.True);
        }

        [Test]
        public void ServiceRequest_ActiveContract_IsAllowed()
        {
            var contract = new Contract
            {
                Status = "Active"
            };

            Assert.That(CanCreateServiceRequest(contract), Is.True);
        }

        [Test]
        public void ServiceRequest_ExpiredContract_IsBlocked()
        {
            var contract = new Contract
            {
                Status = "Expired"
            };

            Assert.That(CanCreateServiceRequest(contract), Is.False);
        }

        [Test]
        public void ServiceRequest_DraftContract_IsBlocked()
        {
            var contract = new Contract
            {
                Status = "Draft"
            };

            Assert.That(CanCreateServiceRequest(contract), Is.False);
        }

        [Test]
        public void ServiceRequest_OnHoldContract_IsBlocked()
        {
            var contract = new Contract
            {
                Status = "On Hold"
            };

            Assert.That(CanCreateServiceRequest(contract), Is.False);
        }

        [Test]
        public void ServiceRequest_NullContract_IsBlocked()
        {
            Assert.That(CanCreateServiceRequest(null), Is.False);
        }
    }

    [TestFixture]
    public class ApiIntegrationTests
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private string _token = string.Empty;

        [SetUp]
        public void Setup()
        {
            
            var databaseName = $"GLMS_Test_DB_{Guid.NewGuid()}";

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");

                    builder.ConfigureServices(services =>
                    {
                        /*
                         * Remove the original SQL Server DbContext
                         * registration from the API.
                         */
                        var dbContextOptionsDescriptor =
                            services.SingleOrDefault(service =>
                                service.ServiceType ==
                                typeof(DbContextOptions<AppDbContext>));

                        if (dbContextOptionsDescriptor != null)
                        {
                            services.Remove(dbContextOptionsDescriptor);
                        }

                        /*
                         * Register an in-memory database for integration tests.
                         *
                         * The same databaseName is reused for every request
                         * made during the current test.
                         */
                        services.AddDbContext<AppDbContext>(options =>
                        {
                            options.UseInMemoryDatabase(databaseName);
                        });
                    });
                });

            _client = _factory.CreateClient();

            /*
             * Log in and retrieve a valid JWT token.
             */
            _token = GetJwtTokenSync();

            Assert.That(
                _token,
                Is.Not.Null.And.Not.Empty,
                "The test login did not return a JWT token.");

            /*
             * Add the JWT token to all API requests made by this client.
             */
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private string GetJwtTokenSync()
        {
            using var tempClient = _factory.CreateClient();

            var loginData = new
            {
                username = "admin",
                password = "password123"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginData),
                Encoding.UTF8,
                "application/json");

            var response = tempClient
                .PostAsync("/api/auth/login", content)
                .GetAwaiter()
                .GetResult();

            var body = response.Content
                .ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK),
                $"Login failed. Status: {response.StatusCode}. Body: {body}");

            dynamic? result =
                JsonConvert.DeserializeObject<dynamic>(body);

            return result?.token?.ToString() ?? string.Empty;
        }

        private async Task<int> CreateTestClientAsync(
            string name = "Test Client")
        {
            var clientData = new
            {
                name,
                contactDetails = "test@glms.com",
                region = "ZA"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(clientData),
                Encoding.UTF8,
                "application/json");

            var response =
                await _client.PostAsync("/api/clients", content);

            var body =
                await response.Content.ReadAsStringAsync();

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Created),
                $"CreateTestClient failed. " +
                $"Status: {response.StatusCode}. Body: {body}");

            dynamic? created =
                JsonConvert.DeserializeObject<dynamic>(body);

            Assert.That(
                created,
                Is.Not.Null,
                "The client creation response was empty.");

            return (int)created!.id;
        }

        private async Task<int> CreateTestContractAsync(
            int clientId,
            string status = "Active")
        {
            var contractData = new
            {
                clientId,
                startDate = "2026-01-01T00:00:00Z",
                endDate = "2027-01-01T00:00:00Z",
                status,
                serviceLevel = "Standard"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(contractData),
                Encoding.UTF8,
                "application/json");

            var response =
                await _client.PostAsync("/api/contracts", content);

            var body =
                await response.Content.ReadAsStringAsync();

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Created),
                $"CreateTestContract failed. " +
                $"Status: {response.StatusCode}. Body: {body}");

            dynamic? created =
                JsonConvert.DeserializeObject<dynamic>(body);

            Assert.That(
                created,
                Is.Not.Null,
                "The contract creation response was empty.");

            return (int)created!.id;
        }

        
        // Basic endpoint tests

        [Test]
        public async Task GetClients_ReturnsOk()
        {
            var response =
                await _client.GetAsync("/api/clients");

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetContracts_ReturnsOk()
        {
            var response =
                await _client.GetAsync("/api/contracts");

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetServiceRequests_ReturnsOk()
        {
            var response =
                await _client.GetAsync("/api/servicerequests");

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetContracts_ReturnsJson_NotNull()
        {
            var response =
                await _client.GetAsync("/api/contracts");

            var body =
                await response.Content.ReadAsStringAsync();

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));

            Assert.That(
                body,
                Is.Not.Null.And.Not.Empty);
        }

        // Authentication tests

        [Test]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            using var freshClient = _factory.CreateClient();

            var loginData = new
            {
                username = "wrong",
                password = "wrong"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginData),
                Encoding.UTF8,
                "application/json");

            var response =
                await freshClient.PostAsync(
                    "/api/auth/login",
                    content);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public void Login_ValidCredentials_ReturnsToken()
        {
            Assert.That(
                _token,
                Is.Not.Null.And.Not.Empty);
        }

        // Client create-then-read test
       

        [Test]
        public async Task PostClient_ThenGet_ReturnsCreatedClient()
        {
            var clientId =
                await CreateTestClientAsync("Logistics Co");

            var getResponse =
                await _client.GetAsync(
                    $"/api/clients/{clientId}");

            var getBody =
                await getResponse.Content.ReadAsStringAsync();

            Assert.That(
                getResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.OK),
                $"GET client failed. " +
                $"Status: {getResponse.StatusCode}. " +
                $"Body: {getBody}");

            dynamic? fetched =
                JsonConvert.DeserializeObject<dynamic>(getBody);

            Assert.That(
                fetched,
                Is.Not.Null);

            Assert.That(
                (string)fetched!.name,
                Is.EqualTo("Logistics Co"));

            Assert.That(
                (string)fetched.region,
                Is.EqualTo("ZA"));
        }


        [Test]
        public async Task PostContract_ThenGet_ReturnsCreatedContract()
        {
            var clientId =
                await CreateTestClientAsync("Contract Test Co");

            var contractId =
                await CreateTestContractAsync(
                    clientId,
                    "Active");

            var getResponse =
                await _client.GetAsync(
                    $"/api/contracts/{contractId}");

            var getBody =
                await getResponse.Content.ReadAsStringAsync();

            Assert.That(
                getResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.OK),
                $"GET contract failed. " +
                $"Status: {getResponse.StatusCode}. " +
                $"Body: {getBody}");

            dynamic? fetched =
                JsonConvert.DeserializeObject<dynamic>(getBody);

            Assert.That(
                fetched,
                Is.Not.Null);

            Assert.That(
                (int)fetched!.id,
                Is.EqualTo(contractId));

            Assert.That(
                (string)fetched.status,
                Is.EqualTo("Active"));

            Assert.That(
                (string)fetched.serviceLevel,
                Is.EqualTo("Standard"));
        }


        [Test]
        public async Task PatchContractStatus_Draft_To_Active_Succeeds()
        {
            var clientId =
                await CreateTestClientAsync("Status Test Co");

            var contractId =
                await CreateTestContractAsync(
                    clientId,
                    "Draft");

            var statusData = new
            {
                status = "Active"
            };

            var patchContent = new StringContent(
                JsonConvert.SerializeObject(statusData),
                Encoding.UTF8,
                "application/json");

            var patchResponse =
                await _client.PatchAsync(
                    $"/api/contracts/{contractId}/status",
                    patchContent);

            var patchBody =
                await patchResponse.Content.ReadAsStringAsync();

            Assert.That(
                patchResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.NoContent),
                $"PATCH contract status failed. " +
                $"Status: {patchResponse.StatusCode}. " +
                $"Body: {patchBody}");

            var getResponse =
                await _client.GetAsync(
                    $"/api/contracts/{contractId}");

            var getBody =
                await getResponse.Content.ReadAsStringAsync();

            Assert.That(
                getResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.OK),
                $"GET updated contract failed. " +
                $"Status: {getResponse.StatusCode}. " +
                $"Body: {getBody}");

            dynamic? updated =
                JsonConvert.DeserializeObject<dynamic>(getBody);

            Assert.That(
                updated,
                Is.Not.Null);

            Assert.That(
                (string)updated!.status,
                Is.EqualTo("Active"));
        }

        [Test]
        public async Task PatchContractStatus_InvalidTransition_ReturnsBadRequest()
        {
            var clientId =
                await CreateTestClientAsync("Invalid Trans Co");

            var contractId =
                await CreateTestContractAsync(
                    clientId,
                    "Expired");

            var statusData = new
            {
                status = "Active"
            };

            var patchContent = new StringContent(
                JsonConvert.SerializeObject(statusData),
                Encoding.UTF8,
                "application/json");

            var patchResponse =
                await _client.PatchAsync(
                    $"/api/contracts/{contractId}/status",
                    patchContent);

            var patchBody =
                await patchResponse.Content.ReadAsStringAsync();

            Assert.That(
                patchResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.BadRequest),
                $"The invalid status transition should return " +
                $"BadRequest. Actual status: " +
                $"{patchResponse.StatusCode}. Body: {patchBody}");
        }


        [Test]
        public async Task GetContract_NonExistentId_ReturnsNotFound()
        {
            var response =
                await _client.GetAsync(
                    "/api/contracts/99999");

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}