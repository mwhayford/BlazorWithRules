using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorApp.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly ConcurrentDictionary<string, byte> _cacheKeys;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _cacheKeys = new ConcurrentDictionary<string, byte>();
    }

    public Task<T?> GetAsync<T>(string key)
        where T : class
    {
        try
        {
            var result = _memoryCache.Get<T>(key);
            _logger.LogDebug("Cache {Operation}: {Key} - {Status}", "GET", key, result != null ? "HIT" : "MISS");
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal,
            };

            options.RegisterPostEvictionCallback(
                (evictedKey, evictedValue, reason, state) =>
                {
                    _cacheKeys.TryRemove(evictedKey.ToString()!, out _);
                    _logger.LogDebug("Cache key {Key} evicted. Reason: {Reason}", evictedKey, reason);
                }
            );

            _memoryCache.Set(key, value, options);
            _cacheKeys.TryAdd(key, 0);

            _logger.LogDebug("Cache SET: {Key} with expiration {Expiration}", key, expiration ?? DefaultExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
            _logger.LogDebug("Cache REMOVE: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var regex = new Regex(pattern, RegexOptions.Compiled);
            var keysToRemove = _cacheKeys.Keys.Where(key => regex.IsMatch(key)).ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            }

            _logger.LogDebug("Cache REMOVE BY PATTERN: {Pattern} - {Count} keys removed", pattern, keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache keys by pattern {Pattern}", pattern);
        }

        return Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiration = null)
        where T : class
    {
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var value = await getItem();
        if (value != null)
        {
            await SetAsync(key, value, expiration);
        }

        return value;
    }

    public async Task<TValue> GetOrSetValueAsync<TValue>(
        string key,
        Func<Task<TValue>> getItem,
        TimeSpan? expiration = null
    )
        where TValue : struct
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var cachedValue) && cachedValue is TValue value)
            {
                _logger.LogDebug("Cache HIT: {Key}", key);
                return value;
            }

            _logger.LogDebug("Cache MISS: {Key}", key);
            var newValue = await getItem();

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal,
            };

            options.RegisterPostEvictionCallback(
                (evictedKey, evictedVal, reason, state) =>
                {
                    _cacheKeys.TryRemove(evictedKey.ToString()!, out _);
                    _logger.LogDebug("Cache key {Key} evicted. Reason: {Reason}", evictedKey, reason);
                }
            );

            _memoryCache.Set(key, newValue, options);
            _cacheKeys.TryAdd(key, 0);

            _logger.LogDebug("Cache SET: {Key} with expiration {Expiration}", key, expiration ?? DefaultExpiration);

            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or setting cache key {Key}", key);
            return await getItem();
        }
    }

    public bool TryGetValue<T>(string key, out T? value)
        where T : class
    {
        try
        {
            return _memoryCache.TryGetValue(key, out value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to get cache key {Key}", key);
            value = null;
            return false;
        }
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
        where T : class
    {
        SetAsync(key, value, expiration).GetAwaiter().GetResult();
    }

    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }
}
