using SharpTypes.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class SharpTypesExtensions
    {
        public static IApplicationBuilder UseSharpTypes(this IApplicationBuilder app)
        {
            app.UseMiddleware<SharpTypesMiddleware>();

            return app;
        }
    }
}
