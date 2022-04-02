﻿using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace DFACore.Repository
{
    public class UnionBankApiKeysRepository : IDisposable
    {
        private readonly Data.ApplicationDbContext _applicationDbContext;
        private readonly IMemoryCache _memoryCache;

        private bool isDisposed;

        public UnionBankApiKeysRepository(Data.ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;

            var memoryCacheOptions = new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(35)
            };

            _memoryCache = new MemoryCache(memoryCacheOptions);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    _applicationDbContext.Dispose();
                }

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Models.UnionBankApiKeys GetKeys(Models.Enums.UnionBankApiKeysEnviroment enviroment)
        {
            var keys = _memoryCache.Get<Models.UnionBankApiKeys>(enviroment);

            if (keys is null)
            {
                keys = _applicationDbContext.UnionBankApiKeys.FirstOrDefault(keys => keys.Environment == enviroment);

                var memoryCacheItemOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.UtcNow.AddMinutes(30)
                };

                _memoryCache.Set(enviroment, keys, memoryCacheItemOptions);
            }

            return keys;
        }
    }
}
