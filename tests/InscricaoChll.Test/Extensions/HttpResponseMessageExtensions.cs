using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InscricaoChll.Test.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> DeserializeObject<T>(this HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }
}