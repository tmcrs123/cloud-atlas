using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace cloud_atlas.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly CosmosDbContext CosmosDbContext;

        public WeatherForecastController(CosmosDbContext cosmosDb)
        {
            CosmosDbContext = cosmosDb;
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotos()
        {
            try
            {

            await CosmosDbContext.Database.EnsureCreatedAsync();

            CosmosDbContext.Add(new MarkerPhotos
            {
                Id = new Guid(),
                MapId = new Guid(),
                MarkerId = new Guid(),
                Photos = new List<Photo>
                {
                    new Photo
                    {
                        Id = Guid.NewGuid(),
                        Legend = "Sunset over the mountains",
                        URL = "https://example.com/photos/sunset.jpg"
                    },
                    new Photo
                    {
                        Id = Guid.NewGuid(),
                        Legend = "Cloudy sky above the lake",
                        URL = "https://example.com/photos/cloudy-lake.jpg"
                    },
                    new Photo
                    {
                        Id = Guid.NewGuid(),
                        Legend = "Rainy day in the city",
                        URL = "https://example.com/photos/rainy-city.jpg"
                    }
                }
            });

                await CosmosDbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}


