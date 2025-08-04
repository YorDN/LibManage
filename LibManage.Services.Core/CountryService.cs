using LibManage.Data.Models.DTOs;
using LibManage.Services.Core.Contracts;

using System.Text.Json;

namespace LibManage.Services.Core
{
    public class CountryService(HttpClient http) : ICountryService
    {
        public async Task<List<CountryApiModel>> GetCountriesAsync()
        {
            try
            {
                var response = await http.GetAsync(
                    "https://restcountries.com/v3.1/all?fields=name,cca2,flags");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var countries = JsonSerializer.Deserialize<List<CountryApiModel>>(content);

                return countries?.OrderBy(c => c.name.common).ToList() ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                Console.WriteLine($"Error fetching countries: {ex.Message}");
                return new List<CountryApiModel>();
            }
        }

    }
}
