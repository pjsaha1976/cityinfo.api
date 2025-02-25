using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/cities")]
    [ApiVersion(1)]
    [ApiVersion(2)]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _citiesDataStore;
        private readonly IMapper _mapper;
        private const int maxCitiesPageSize = 20;

        public CitiesController(ICityInfoRepository citiesDataStore, IMapper mapper)
        {
            _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// get cities
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searchQuery"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities(string? name, string? searchQuery, int pageNumber=1, int pageSize = 10)
        {
            if (pageSize > maxCitiesPageSize)
            {
                pageSize = maxCitiesPageSize;
            }
            var (cityEntities,paginationMetadata) = await _citiesDataStore.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

            Response.Headers.Append("x-pagination", JsonSerializer.Serialize(paginationMetadata));
            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));
        }


        /// <summary>
        /// Get a city by id
        /// </summary>
        /// <param name="id">the id of the city</param>
        /// <param name="includePointOfInterest">Whether or not to include points of interest</param>
        /// <returns>A city with or without points of interest</returns>
        /// <response code="200">Returns the requested city</response>
        /// <response code="400">Returns error - invalid input</response>
        /// 

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCity(int id, bool includePointOfInterest = false)
        {
            // find city
            var cityToReturn = await _citiesDataStore.GetCityAsync(id, includePointOfInterest);

            if (cityToReturn == null)
            {
                return NotFound();
            }
           
            if (includePointOfInterest) return Ok(_mapper.Map<CityDto>(cityToReturn));
            
            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(cityToReturn));

        }
    }
}
