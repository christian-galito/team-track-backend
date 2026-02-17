using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;
using TeamTrack.Domain.Common;

namespace TeamTrack.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    Message = "Validation failed.",
                    Errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
                    TraceId = context.TraceIdentifier
                });
            }
            catch (DomainException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    Message = "Domain validation failed.",
                    Errors = new[] { ex.Message },
                    TraceId = context.TraceIdentifier
                });
            }
            catch (DbUpdateException ex)
            {
                string? constraintName = null;

                if (TryGetUniqueConstraintName(ex, out constraintName))
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Message = "A resource conflict occurred.",
                        Errors = new[] { "A resource with the same unique value already exists." },
                        TraceId = context.TraceIdentifier
                    });

                    _logger.LogWarning(ex, "Unique constraint violation. Constraint: {Constraint}, TraceId: {TraceId}", constraintName, context.TraceIdentifier);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Message = "A database error occurred.",
                        TraceId = context.TraceIdentifier
                    });

                    _logger.LogError(ex, "Unexpected DbUpdateException occurred. TraceId: {TraceId}", context.TraceIdentifier);
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(new
                {
                    Message = "An unexpected error occurred.",
                    TraceId = context.TraceIdentifier
                });

                _logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
            }
        }

        private static bool TryGetUniqueConstraintName(DbUpdateException ex, out string? constraintName)
        {
            constraintName = null;

            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                constraintName = pgEx.ConstraintName ?? "Unknown_Postgres_UniqueConstraint";
                return true;
            }

            if (ex.InnerException is DbException dbEx && dbEx.Message.Contains("UNIQUE constraint failed"))
            {
                constraintName = dbEx.Message;
                return true;
            }

            return false;
        }
    }
}
