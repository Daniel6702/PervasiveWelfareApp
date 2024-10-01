using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace WelfareMonitorApp.Services
{
    public class LocationService
    {
        private Page _page;

        public LocationService(Page page)
        {
            _page = page;
            CheckPermissionsAsync().Wait();
        }

        public async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                var location = await Geolocation.Default.GetLastKnownLocationAsync();

                if (location != null)
                {
                    return location;
                }

                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(30));
                location = await Geolocation.Default.GetLocationAsync(request);

                return location;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Console.WriteLine($"Feature not supported: {fnsEx.Message}");
            }
            catch (PermissionException pEx)
            {
                Console.WriteLine($"Permission issue: {pEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to get location: {ex.Message}");
            }

            return null;
        }

        private async Task CheckPermissionsAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                await _page.DisplayAlert("Permission Denied", "Location permission is required to access GPS.", "OK");
            }
        }
    }
}
