namespace AddressData.WebApi.Controllers;

using Core;
using Core.Models.ApiResponse;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : Controller
{
    [Route(Constants.ErrorControllerRoute)]
    public ErrorModelApiResponse Error()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context!.Error;

        var code = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        Response.StatusCode = code;

        return new ErrorModelApiResponse(exception);
    }
}
