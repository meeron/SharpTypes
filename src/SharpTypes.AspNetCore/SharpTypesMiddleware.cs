using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SharpTypes.AspNetCore
{
    public sealed class SharpTypesMiddleware
    {
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

        private readonly RequestDelegate _next;

        public SharpTypesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var response = context.Response;
            var request = context.Request;

            if (!request.Path.StartsWithSegments("/_types") || request.Method != "GET")
            {
                await _next(context);
                return;
            }

            var requestPath = request.Path.ToString().ToLower();
            if (requestPath.EndsWith("/"))
            {
                requestPath = requestPath.Substring(0, requestPath.Length - 1);
            }

            if (Cache.ContainsKey(requestPath))
            {
                await WriteResponse(response, Cache[requestPath]);
                return;
            }

            var pathSegments = requestPath.Split('/');
            if (pathSegments.Length != 3)
            {
                response.StatusCode = 404;
                return;
            }

            var controllerName = pathSegments[2];

            var controllerType = Assembly.GetEntryAssembly()?
                .GetExportedTypes()
                .Where(t => t.BaseType?.Equals(typeof(ControllerBase)) ?? false)
                .SingleOrDefault(t => t.Name.Equals($"{controllerName}Controller", StringComparison.OrdinalIgnoreCase));

            if (controllerType == null)
            {
                response.StatusCode = 404;
                return;
            }

            var methodTypes = controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<HttpGetAttribute>() != null)
                .Where(m => m.ReturnType != typeof(IActionResult) && m.ReturnType != typeof(ActionResult))
                .Select(m => m.ReturnType)
                .ToArray();

            var producedTypes = controllerType
                .GetMethods()
                .Select(m => m.GetCustomAttribute<ProducesResponseTypeAttribute>())
                .Where(x => x != null && x.StatusCode == 200)
                .Select(x => x.Type)
                .ToArray();

            using (var writer = new StringWriter())
            {
                var typeWriter = new TypeWriter();

                typeWriter.Write(producedTypes, writer);
                typeWriter.Write(methodTypes, writer);

                var content = writer.ToString();

                await WriteResponse(response, content);

                Cache.Add(requestPath, content);
            }
        }

        private static async Task WriteResponse(HttpResponse response, string content)
        {
            response.StatusCode = 200;
            response.ContentType = "text/x.typescript";

            await response.WriteAsync(content, Encoding.UTF8);
        }
    }
}
