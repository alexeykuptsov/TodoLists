using EasyExceptions;
using Microsoft.Net.Http.Headers;
using Serilog;

namespace TodoLists.App.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await this.next.Invoke(context);
        }
        catch (Exception e)
        {
            Log.Error("Unhandled exception: " + ExceptionDumpUtil.Dump(e));
            
            var response = context.Response;
            if (response.HasStarted)
            {
                throw;
            }
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.ContentType = "application/json; charset=utf-8";
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);

            await context.Response.WriteAsync("Unexpected server error.");
        }
    }
}