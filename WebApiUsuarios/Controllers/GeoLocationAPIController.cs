using Alexa.DAL;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Model;
using Microsoft.AspNetCore.Mvc;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("api/geo")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class GeoLocationAPIController : ControllerBase
    {       

        [HttpGet(template:"[action]/{ipAddress}/{origen}")]
        public IActionResult GeoCountry(string ipAddress, string origen)
        {
            if (origen =="country")
            {
                using (var reader = new DatabaseReader(file: @"C:\Misthos\wwwroot\db\geo.mmdb"))
                //using (var reader = new DatabaseReader(file: @"C:\\Users\\ldavila\\source\\Repositorio\\Alexa\\WebApiUsuarios\\wwwroot\\db\\geo.mmdb"))
                {
                    //var s = reader.Asn(ipAddress);
                    var response = reader.Country(ipAddress);
                    var geoLocation = new CountryGeo();
                    geoLocation.countryName = response.Country.Name;
                    geoLocation.countryIsoCode = response.Country.IsoCode;
                    geoLocation.IsInEuropeanUnion = response.Country.IsInEuropeanUnion;
                    return StatusCode(StatusCodes.Status200OK, geoLocation);
                }
            }
            else
            {
                using (var reader = new DatabaseReader(file: @"C:\Misthos\wwwroot\db\GeoCity.mmdb"))
                //using (var reader = new DatabaseReader(file: @"C:\Users\ldavila\source\Repositorio\Alexa\WebApiUsuarios\wwwroot\db\GeoCity.mmdb"))
                {
                    var response = reader.City(ipAddress);
                    var geoLocation = new CityGeo();
                    geoLocation.cityName = response.Country.Name;
                    geoLocation.cityIsoCode = response.Country.IsoCode;
                    geoLocation.cityLatitude = response.Location.Latitude;
                    geoLocation.cityLongitude = response.Location.Longitude;
                    geoLocation.cityTimeZone = response.Location.TimeZone;
                    return StatusCode(StatusCodes.Status200OK, geoLocation);
                }
            }
            
                
        }       

    }
}
