using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Tests.TestHelpers
{
    internal class TestTempDataProvider : ITempDataProvider
    {
        private Dictionary<string, object?> _data = new();
        public IDictionary<string, object?> LoadTempData(HttpContext context) => _data;
        public void SaveTempData(HttpContext context, IDictionary<string, object?> values) => _data = new(values);
    }
}
