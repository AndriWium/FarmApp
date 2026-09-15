# Decisions

One line per irreversible-ish decision, dated.

- 2026-09-15: Swagger bearer-auth padlock (doc 13 Phase 0a) is wired via Swashbuckle's own `AddSwaggerGen`/`UseSwagger` document generation instead of the previously-used `Microsoft.AspNetCore.OpenApi` `AddOpenApi`/`MapOpenApi`. Reason: .NET 10 ships `Microsoft.OpenApi` 2.x, which removed the concrete `OpenApiSecurityScheme`/`OpenApiReference` types `AddOpenApi`'s document-transformer API expected; Swashbuckle.AspNetCore 10.2.3 already targets the new model with a stable, documented `AddSecurityDefinition`/`AddSecurityRequirement` API (the latter now takes an `OpenApiSecuritySchemeReference` via a `Func<OpenApiDocument, OpenApiSecurityRequirement>` delegate). Swagger UI now serves from `/swagger/v1/swagger.json` instead of `/openapi/v1.json`. No spec doc was wrong — doc 13 just said "standard ASP.NET Core JWT+Swashbuckle setup," which this now literally is.
