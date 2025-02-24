using CityInfo.API.DBContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _cityInfoContext;
        public CityInfoRepository(CityInfoContext cityInfoContext)
        {
            _cityInfoContext = cityInfoContext ?? throw new ArgumentNullException(nameof(cityInfoContext));
        }

        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber=1, int pageSize=10)
        {

            var cities = _cityInfoContext.Cities as IQueryable<City>;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
                cities = cities.Where(c => c.Name == name);
            }
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                cities = cities.Where(c => c.Name.Contains(searchQuery) || (c.Description != null && c.Description.Contains(searchQuery)));
            }

            var totalItemCount = await cities.CountAsync();
            var paginationMetaData = new PaginationMetadata(totalItemCount, pageSize, pageNumber);
      
            var cityCollection = await cities.OrderBy(c => c.Name)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();
            return (cityCollection, paginationMetaData);

        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if (!includePointsOfInterest)
            {
                return await _cityInfoContext.Cities
                .Where(c => c.Id == cityId)
                .FirstOrDefaultAsync();
            }
            return await _cityInfoContext.Cities
                .Include(c => c.PointsOfInterest)
                .Where(c => c.Id == cityId)
                .FirstOrDefaultAsync();

        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            var city = await _cityInfoContext.Cities
                .Where(c => c.Id == cityId).FirstOrDefaultAsync();
            if (city == null) return false;
            return true;

        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await _cityInfoContext.PointOfInterests
                .Where(p => p.CityId == cityId)
                .ToListAsync();
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            return await _cityInfoContext.PointOfInterests
                .Where(p => p.CityId == cityId && p.Id == pointOfInterestId)
                .FirstOrDefaultAsync();
        }

        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            if (city != null)
            {
                city.PointsOfInterest.Add(pointOfInterest);
            }
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {

            _cityInfoContext.PointOfInterests.Remove(pointOfInterest);

        }

        public async Task<bool> CityNameMatchesCityIdAsync(string cityName, int cityId)
        {
            var city = await _cityInfoContext.Cities
                .Where(c => c.Id == cityId && c.Name == cityName)
                .FirstOrDefaultAsync();

            return city != null;

        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _cityInfoContext.SaveChangesAsync() >= 0);
        }
    }
}
