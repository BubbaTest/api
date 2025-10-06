namespace Alexa.DAL
{
    public class CityGeo
    {
        public string cityName { get; set; }
        public string cityIsoCode { get; set; }
        public Nullable<double> cityLatitude { get; set; }
        public Nullable<double> cityLongitude { get; set; }
        public string cityTimeZone { get; set; }
    }
}
