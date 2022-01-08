using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DFACore.Helpers
{


    public static class SessionExtensions
    {
        public static T GetComplexData<T>(this ISession session, string key)
        {
            var data = session.GetString(key);
            if (data == null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static void SetComplexData(this ISession session, string key, object value)
        {
            var str = JsonConvert.SerializeObject(value);
            session.SetString(key, str);
        }
    }
}
