using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services.GoogleMap
{
    public interface IGoogleMapApiService
    {
        Task<GooglePlaceAutoCompleteResults> GetPlaces(string text);

        Task<GooglePlace> GetPlaceDetails(string placeId);
    }
}
