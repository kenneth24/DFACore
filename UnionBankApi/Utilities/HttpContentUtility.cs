using System.Net.Http;
using System.Text;

namespace UnionBankApi.Utilities
{
    public static class HttpContentUtility
    {
        public static StringContent CreateJsonContent(string content)
        {
            return new StringContent(content, Encoding.UTF8, "application/json");
        }
    }
}
