using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Shopping_Mobile.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho phép Request đi tiếp vào Pipeline (đến Controller)
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Đã xảy ra lỗi không mong muốn tại: {Path}", context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Phân loại mã lỗi HTTP dựa trên loại Exception
            context.Response.StatusCode = exception switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,     // Không tìm thấy dữ liệu
                ArgumentException => StatusCodes.Status400BadRequest,      // Dữ liệu đầu vào sai
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized, // Chưa đăng nhập
                _ => StatusCodes.Status500InternalServerError             // Lỗi hệ thống chưa xác định
            };

            var problemDetails = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "Hệ thống xảy ra lỗi",
                Detail = exception.Message,
                Instance = context.Request.Path 
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}