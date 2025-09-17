namespace BlazorApp.Core.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiration = null) where T : class;
    Task<TValue> GetOrSetValueAsync<TValue>(string key, Func<Task<TValue>> getItem, TimeSpan? expiration = null) where TValue : struct;
    bool TryGetValue<T>(string key, out T? value) where T : class;
    void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    void Remove(string key);
}
