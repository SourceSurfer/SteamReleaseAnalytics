using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace SteamReleaseAnalytics.Api.Swagger
{
    /// <summary>
    /// Помечает в Swagger схемой Bearer только действия с [Authorize] и добавляет им ответы 401 / 403.
    /// </summary>
    internal sealed class AuthorizeOperationFilter : IOperationFilter
    {
        public const string SchemeName = "Bearer";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var requiresAuthorization = context.MethodInfo.GetCustomAttributes(true)
                .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? [])
                .OfType<AuthorizeAttribute>()
                .Any();

            if (!requiresAuthorization)
                return;

            operation.Responses ??= [];
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Нет токена или токен недействителен" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Недостаточно прав" });

            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(SchemeName, context.Document)] = []
                }
            ];
        }
    }
}
