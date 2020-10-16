using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
    public class GooglePlaceAutoCompleteResults
    {
        [JsonProperty("predictions")]
        public List<GooglePlaceInfo> Predictions { set; get; }

        [JsonProperty("status")]
        public string Status { set; get; }
    }

    public class GooglePlaceInfo
    {
        [JsonProperty("description")]
        public string Description { set; get; }

        [JsonProperty("matched_substrings")]
        public List<MatchSubstring> MatchedSubstrings { set; get; }

        [JsonProperty("place_id")]
        public string PlaceId { set; get; }

        [JsonProperty("reference")]
        public string Reference { set; get; }

        [JsonProperty("structured_formatting")]
        public StructuredFormat StructuredFormatting { set; get; }

        [JsonProperty("terms")]
        public List<MatchSubstring> Terms { set; get; }

        [JsonProperty("types")]
        public List<string> Types { set; get; }
    }

    public class MatchSubstring
    {
        [JsonProperty("length")]
        public int Length { set; get; }

        [JsonProperty("offset")]
        public int Offset { set; get; }

        [JsonProperty("value")]
        public string Value { set; get; }
    }

    public class StructuredFormat
    {
        [JsonProperty("main_text")]
        public string MainText { set; get; }

        [JsonProperty("main_text_matched_substrings")]
        public List<MatchSubstring> MainTextMatchedSubstrings { set; get; }

        [JsonProperty("secondary_text")]
        public string SecondaryText { set; get; }
    }

    public class GooglePlace
    {
        [JsonProperty("result")]
        public GooglePlaceMeta Result { set; get; }

        [JsonProperty("status")]
        public string Status { set; get; }

        public EbGeoLocation GetCordinates()
        {
            return Result?.Geometry?.Location;
        }
    }

    public class GooglePlaceMeta
    {
        [JsonProperty("geometry")]
        public GooglePlaceGeometry Geometry { set; get; }
    }

    public class GooglePlaceGeometry
    {
        [JsonProperty("location")]
        public EbGeoLocation Location { set; get; }
    }

    public class EbGeoLocation
    {
        [JsonProperty("lat")]
        public double Latitude { set; get; }

        [JsonProperty("lng")]
        public double Longitude { set; get; }

        public string Address { set; get; }

        public EbGeoLocation() { }

        public EbGeoLocation(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
    }

    public class EbMapBinding
    {
        public EbGeoLocation Location { set; get; }

        public Command ResultCommand { set; get; }
    }

}
