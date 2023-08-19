using Authentication.Api.CustomMiddleware;

namespace Authentication.Api.Extensions
{
    public static class ApplicationExtensions
	{
		public static void UseExceptionHandling(this WebApplication app)
		{
			app.UseMiddleware<ExceptionMiddleware>();
		}
	}
}

