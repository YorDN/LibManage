
using LibManage.Data.Models.DTOs;

namespace LibManage.Services.Core.Contracts
{
    public interface ICountryService
    {
        public Task<List<CountryApiModel>> GetCountriesAsync();
    }
}
