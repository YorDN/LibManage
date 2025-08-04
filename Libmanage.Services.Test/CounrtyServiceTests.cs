using LibManage.Data.Models.DTOs;
using LibManage.Services.Core;

using Moq;
using Moq.Protected;

using System.Net;
using System.Text.Json;

namespace Libmanage.Services.Test
{
    [TestFixture]
    public class CounrtyServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private CountryService _countryService;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _countryService = new CountryService(_httpClient);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public async Task GetCountriesAsync_ReturnsSortedCountries_WhenApiCallSucceeds()
        {
            var testCountries = new List<CountryApiModel>
            {
                new() { name = new() { common = "Zambia" }, cca2 = "ZM", flags = new() { png = "flag1.png" } },
                new() { name = new() { common = "Albania" }, cca2 = "AL", flags = new() { png = "flag2.png" } }
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(testCountries))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString() == "https://restcountries.com/v3.1/all?fields=name,cca2,flags"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var result = await _countryService.GetCountriesAsync();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Albania", result[0].name.common);
            Assert.AreEqual("Zambia", result[1].name.common);
        }

        [Test]
        public async Task GetCountriesAsync_ReturnsEmptyList_WhenApiReturnsEmpty()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var result = await _countryService.GetCountriesAsync();

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetCountriesAsync_ReturnsEmptyList_WhenApiCallFails()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API call failed"));

            var result = await _countryService.GetCountriesAsync();

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetCountriesAsync_ReturnsEmptyList_WhenApiReturnsInvalidJson()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("invalid json")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString() == "https://restcountries.com/v3.1/all?fields=name,cca2,flags"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var result = await _countryService.GetCountriesAsync();

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetCountriesAsync_ReturnsEmptyList_WhenApiReturnsNotFound()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var result = await _countryService.GetCountriesAsync();

            Assert.IsEmpty(result);
        }
    }
}