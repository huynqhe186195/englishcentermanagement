using System.Threading.Tasks;

namespace EnglishCenter.Web.Services;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string url);
    Task<byte[]?> GetFileAsync(string url);
    Task<T?> PostAsync<TRequest, T>(string url, TRequest body);
    Task<T?> PostMultipartAsync<T>(string url, IFormFile file);
    Task<bool> PostAsync(string url, object body);
    Task<T?> PutAsync<TRequest, T>(string url, TRequest body);
    Task<bool> PutAsync(string url, object body);
    Task<bool> DeleteAsync(string url);
}
