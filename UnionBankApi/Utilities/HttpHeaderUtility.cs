using System.Net.Http.Headers;

namespace UnionBankApi.Utilities
{
    public static class HttpHeaderUtility
    {
        public static MediaTypeWithQualityHeaderValue CreateJsonContentHeaderValue()
        {
            return new MediaTypeWithQualityHeaderValue("application/json");
        }

        public static AuthenticationHeaderValue CreateAuthorizationHeaderValue(string accessToken)
        {
            return new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}
