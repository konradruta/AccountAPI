
using AccountAPI.Exceptions;

namespace AccountAPI.Middleware
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (WrongLoginException loginException)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(loginException.Message);
            }
            catch (SearchPhraseException searchPhraseException)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(searchPhraseException.Message);
            }
            catch (NotUserFoundException notUserFoundException)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(notUserFoundException.Message);
            }
            catch (TemporaryPasswordException temporaryPasswordException)
            {
                context.Response.StatusCode = 501;
                await context.Response.WriteAsync(temporaryPasswordException.Message);
            }
        }
    }
}
