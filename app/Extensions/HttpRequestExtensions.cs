using Microsoft.AspNetCore.Http;

namespace App.Extensions;

public static class HttpRequestExtensions
{
    public static string GetBaseUrl(this IHttpContextAccessor context)
        => $"{context.HttpContext!.Request.Scheme}://{context.HttpContext!.Request.Host}";
}