using Microsoft.Maui.Devices.Sensors;
using System;
using System.Threading.Tasks;

namespace WelfareMonitorApp.Services
{
    public class LocationService
    {
        public async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                // Attempt to get the last known location
                var location = await Geolocation.Default.GetLastKnownLocationAsync();

                if (location != null)
                {
                    return location;
                }

                // If no last known location, request a new location
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(30));
                location = await Geolocation.Default.GetLocationAsync(request);

                return location;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                Console.WriteLine($"Feature not supported: {fnsEx.Message}");
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                Console.WriteLine($"Permission issue: {pEx.Message}");
            }
            catch (Exception ex)
            {
                // Unable to get location
                Console.WriteLine($"Unable to get location: {ex.Message}");
            }

            return null;
        }
    }
}
