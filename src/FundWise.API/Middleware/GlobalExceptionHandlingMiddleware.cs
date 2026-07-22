using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using FundWise.Domain.Exceptions;

namespace FundWise.API.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException ve => ((int)HttpStatusCode.BadRequest, "Validation Error", ve.Message, (object?)ve.Errors),
            NotFoundException ne => ((int)HttpStatusCode.NotFound, "Resource Not Found", ne.Message, null),
            UnauthorizedException ue => ((int)HttpStatusCode.Unauthorized, "Unauthorized", ue.Message, null),
            ConflictException ce => ((int)HttpStatusCode.Conflict, "Conflict", ce.Message, null),
            DomainException de => ((int)HttpStatusCode.BadRequest, "Domain Error", de.Message, null),
            _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred on the server.", null)
        };

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        var json = JsonSerializer.Serialize(problemDetails);
        return context.Response.WriteAsync(json);
    }
}
