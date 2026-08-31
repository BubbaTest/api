//using Alexa.DAL;
using Alexa.Filters;
//using MaxMind.GeoIP2;
//using MaxMind.GeoIP2.Model;
using Microsoft.AspNetCore.Mvc;
//using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("api/geo")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class GeoLocationAPIController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GeoLocationAPIController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [NoCache]
        [HttpGet("InformacionIP/{ip}")]
        public async Task<IActionResult> InformacionIP([FromRoute] string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                return BadRequest(new { mensaje = "La dirección IP es requerida." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                string url = $"https://api.ipquery.io/{ip}";

                // Al estar los DTOs decorados con JsonPropertyName, la deserialización es directa y exacta
                var data = await client.GetFromJsonAsync<IpQueryResponse>(url);

                if (data == null)
                {
                    return NotFound(new { mensaje = "No se obtuvo respuesta del servicio de IP." });
                }

                return Ok(data);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { mensaje = "Error al comunicarse con el proveedor de IP externo.", detalle = ex.Message });
            }
        }

        //[HttpGet(template:"[action]/{ipAddress}/{origen}")]
        //public IActionResult GeoCountry(string ipAddress, string origen)
        //{
        //    if (origen =="country")
        //    {
        //        using (var reader = new DatabaseReader(file: @"C:\Misthos\wwwroot\db\geo.mmdb"))
        //        //using (var reader = new DatabaseReader(file: @"C:\\Users\\ldavila\\source\\Repositorio\\Alexa\\WebApiUsuarios\\wwwroot\\db\\geo.mmdb"))
        //        {
        //            //var s = reader.Asn(ipAddress);
        //            var response = reader.Country(ipAddress);
        //            var geoLocation = new CountryGeo();
        //            geoLocation.countryName = response.Country.Name;
        //            geoLocation.countryIsoCode = response.Country.IsoCode;
        //            geoLocation.IsInEuropeanUnion = response.Country.IsInEuropeanUnion;
        //            return StatusCode(StatusCodes.Status200OK, geoLocation);
        //        }
        //    }
        //    else
        //    {
        //        using (var reader = new DatabaseReader(file: @"C:\Misthos\wwwroot\db\GeoCity.mmdb"))
        //        //using (var reader = new DatabaseReader(file: @"C:\Users\ldavila\source\Repositorio\Alexa\WebApiUsuarios\wwwroot\db\GeoCity.mmdb"))
        //        {
        //            var response = reader.City(ipAddress);
        //            var geoLocation = new CityGeo();
        //            geoLocation.cityName = response.Country.Name;
        //            geoLocation.cityIsoCode = response.Country.IsoCode;
        //            geoLocation.cityLatitude = response.Location.Latitude;
        //            geoLocation.cityLongitude = response.Location.Longitude;
        //            geoLocation.cityTimeZone = response.Location.TimeZone;
        //            return StatusCode(StatusCodes.Status200OK, geoLocation);
        //        }
        //    }
            
                
        //}

        public class IpQueryResponse
        {
            [JsonPropertyName("ip")]
            public string Ip { get; set; } = string.Empty;

            [JsonPropertyName("isp")]
            public IspData Isp { get; set; } = new();

            [JsonPropertyName("location")]
            public LocationData Location { get; set; } = new();

            [JsonPropertyName("risk")]
            public RiskData Risk { get; set; } = new();
        }

        public class IspData
        {
            [JsonPropertyName("asn")]
            public string Asn { get; set; } = string.Empty;

            [JsonPropertyName("org")]
            public string Org { get; set; } = string.Empty;

            [JsonPropertyName("isp")]
            public string IspName { get; set; } = string.Empty; // Le cambié el nombre para evitar colisión con la clase
        }

        public class LocationData
        {
            [JsonPropertyName("country")]
            public string Country { get; set; } = string.Empty;

            [JsonPropertyName("country_code")]
            public string CountryCode { get; set; } = string.Empty;

            [JsonPropertyName("city")]
            public string City { get; set; } = string.Empty;

            [JsonPropertyName("state")]
            public string State { get; set; } = string.Empty;

            [JsonPropertyName("zipcode")]
            public string Zipcode { get; set; } = string.Empty;

            [JsonPropertyName("latitude")]
            public double Latitude { get; set; } // Cambiado a double por el formato numérico del JSON

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; } // Cambiado a double por el formato numérico del JSON

            [JsonPropertyName("timezone")]
            public string Timezone { get; set; } = string.Empty;

            [JsonPropertyName("localtime")]
            public string Localtime { get; set; } = string.Empty;
        }

        public class RiskData
        {
            [JsonPropertyName("is_mobile")]
            public bool IsMobile { get; set; }

            [JsonPropertyName("is_vpn")]
            public bool IsVpn { get; set; }

            [JsonPropertyName("is_tor")]
            public bool IsTor { get; set; }

            [JsonPropertyName("is_proxy")]
            public bool IsProxy { get; set; }

            [JsonPropertyName("is_datacenter")]
            public bool IsDatacenter { get; set; }

            [JsonPropertyName("risk_score")]
            public int RiskScore { get; set; }
        }

    }
}
