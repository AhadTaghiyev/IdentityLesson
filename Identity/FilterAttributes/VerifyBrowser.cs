using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Identity.FilterAttributes
{
    public class VerifyBrowser : Attribute, IActionFilter
    {

        public void OnActionExecuting(ActionExecutingContext context)
        {
           
            string value =context.HttpContext.Request.Headers.UserAgent;

            if (value.ToLower().Contains("chrome"))
            {
                context.HttpContext.Response.StatusCode = 404;
                context.HttpContext.Response.WriteAsync("Qaqa giremmesen");
            }
        }


        public void OnActionExecuted(ActionExecutedContext context)
        {
           
        }
    }
}

