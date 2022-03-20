using Microsoft.Extensions.Caching.Memory;
using System;

namespace DFACore.Helpers.Payment
{
    public class PaymentDataCache : IDisposable
    {
        private readonly MemoryCache _memoryCache;

        private bool _isDisposed;

        public PaymentDataCache()
        {
            var memoryCacheOptions = new MemoryCacheOptions
            {

                ExpirationScanFrequency = TimeSpan.FromMinutes(5)
            };

            _memoryCache = new MemoryCache(memoryCacheOptions);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _memoryCache.Dispose();
            }

            _isDisposed = true;

            GC.SuppressFinalize(this);
        }

        public void SetData(string userId, PaymentData data)
        {
            var existingData = _memoryCache.Get<PaymentData>(userId);

            if (existingData is null)
            {
                var memoryCacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.UtcNow.AddMinutes(5)
                };

                _memoryCache.Set(userId, data, memoryCacheEntryOptions);
            }
            else
            {
                existingData.AccessToken = data.AccessToken;
                existingData.OtpRequestId = data.OtpRequestId;
            }
        }

        public PaymentData GetData(string userId)
        {
            return _memoryCache.Get<PaymentData>(userId);
        }
    }
}
