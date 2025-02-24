using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
    [ApiVersion(1)]
    [ApiVersion(2)]
    [ApiController]
    [Authorize(Policy = "MemberMustBeFromParis")]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _citiesDataStore;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService mailService,
            ICityInfoRepository citiesDataStore,
            IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            try
            {
                //var cityName = User.Claims.FirstOrDefault(c => c.Type == "city")?.Value;

                //if (!await _citiesDataStore.CityNameMatchesCityIdAsync(cityName, cityId))
                //    return Forbid();

                var pointsOfInterest = await _citiesDataStore.GetPointsOfInterestForCityAsync(cityId);

                if (pointsOfInterest == null)
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                    return NotFound();
                }

                var result = new List<PointOfInterestDto>();
                foreach (var point in pointsOfInterest)
                {
                    result.Add(
                        new PointOfInterestDto()
                        {
                            Id = point.Id,
                            Name = point.Name,
                            Description = point.Description
                        }
                        );

                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    $"Exception while getting points of interest for city with id {cityId}.",
                    ex);
                return StatusCode(500,
                    "A problem happened while handling your request.");
            }
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(
            int cityId, int pointOfInterestId)
        {
            var pointOfInterest = await _citiesDataStore.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

            if (pointOfInterest == null)
            {
                return NotFound();
            }

            var result = new PointOfInterestDto()
            {
                Id = pointOfInterest.Id,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(
           int cityId,
           PointOfInterestForCreationDto pointOfInterest)
        {
            var cityExists = await _citiesDataStore.CityExistsAsync(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            await _citiesDataStore.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
            await _citiesDataStore.SaveChangesAsync();

            var createdPointOfInterestToReturn = _mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                 new
                 {
                     cityId = cityId,
                     pointOfInterestId = createdPointOfInterestToReturn.Id
                 },
                 createdPointOfInterestToReturn);
        }

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId,
            PointOfInterestForUpdateDto pointOfInterest)
        {

            var cityExists = await _citiesDataStore.CityExistsAsync(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _citiesDataStore.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(pointOfInterest, pointOfInterestEntity);
            await _citiesDataStore.SaveChangesAsync();

            return NoContent();
        }


        [HttpPatch("{pointofinterestid}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(
            int cityId, int pointOfInterestId,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var cityExists = await _citiesDataStore.CityExistsAsync(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = await _citiesDataStore
                .GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStoreDto = _mapper
                .Map<Models.PointOfInterestForUpdateDto>(pointOfInterestFromStore);

            patchDocument.ApplyTo(pointOfInterestFromStoreDto, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!TryValidateModel(pointOfInterestFromStoreDto))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(pointOfInterestFromStoreDto, pointOfInterestFromStore);

            await _citiesDataStore.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{pointOfInterestId}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            if (!await _citiesDataStore.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestFromStore = await _citiesDataStore
                .GetPointOfInterestForCityAsync(cityId,pointOfInterestId);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            _citiesDataStore.DeletePointOfInterest(pointOfInterestFromStore);
            await _citiesDataStore.SaveChangesAsync();

            _mailService.Send("Point of interest deleted.",
                    $"Point of interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted.");

            return NoContent();
        }

    }
}
