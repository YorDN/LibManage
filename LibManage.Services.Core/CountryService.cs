using LibManage.Data.Models.DTOs;
using LibManage.Services.Core.Contracts;
using System.Net.Http.Json;

namespace LibManage.Services.Core
{
    public class CountryService(HttpClient http) : ICountryService
    {
        public async Task<List<CountryApiModel>> GetCountriesAsync()
        {
            try
            {
                var response = await http.GetFromJsonAsync<List<CountryApiModel>>(
                    "https://restcountries.com/v3.1/all?fields=name,cca2,flags");

                return response?.OrderBy(c => c.name.common).ToList() ?? new();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error: {ex.Message}");
                return new List<CountryApiModel>();
            }

        }
    }
}
