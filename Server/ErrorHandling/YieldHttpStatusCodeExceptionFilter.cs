using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StronglyTypedApi.Server.ErrorHandling
{
    public class YieldHttpStatusCodeExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is YieldHttpStatusCodeException ex)
            {
                var result = new StatusCodeResult((int)ex.StatusCode);
                context.Result = result;
                context.ExceptionHandled = true;
            }
        }
    }
}
