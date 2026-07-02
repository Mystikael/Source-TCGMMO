using UnityEngine;

namespace SourceTCG.Debugging
{
    public class GpsSimulator : MonoBehaviour
    {
        [SerializeField] double latitude = 37.7749;
        [SerializeField] double longitude = -122.4194;

        public double Latitude => latitude;
        public double Longitude => longitude;

        public void SetPosition(double lat, double lng)
        {
            latitude = lat;
            longitude = lng;
        }

        public void TeleportToPreset(string preset)
        {
            switch (preset)
            {
                case "sf": SetPosition(37.7749, -122.4194); break;
                case "beach": SetPosition(34.0195, -118.4912); break;
                case "yosemite": SetPosition(37.8651, -119.5383); break;
                default: break;
            }
        }
    }
}