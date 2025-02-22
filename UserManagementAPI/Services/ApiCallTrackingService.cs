using System.Collections.Concurrent;

namespace UserManagementAPI.Services
{
    public class ApiCallTrackingService
    {
        private readonly ConcurrentDictionary<string, int> _apiCallCounts = new ConcurrentDictionary<string, int>();

        public void TrackCall(string path)
        {
            _apiCallCounts.AddOrUpdate(path, 1, (key, count) => count + 1);
        }

        public ConcurrentDictionary<string, int> GetApiCallCounts()
        {
            return _apiCallCounts;
        }
    }
}