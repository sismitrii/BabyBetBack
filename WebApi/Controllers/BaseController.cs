using BabyBetBack.Utils;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace BabyBetBack.Controllers;

public class BaseController : ControllerBase
{
    private readonly ILog _logger;
    public BaseController()
    {
        _logger = LogManager.GetLogger(typeof(BaseController));
    }

    protected IActionResult ReturnResponse(dynamic model)
    {
        if (model.Status == RequestExecution.Successful)
        {
            return Ok(model);
        }

        return BadRequest(model);
    }

    protected IActionResult HandleError(Exception ex, string customErrorMessage = null)
    {
        _logger.Error(ex.Message, ex);

        BaseResponse<string> response = new BaseResponse<string>
        {
            Status = RequestExecution.Error
        };
#if DEBUG
        response.Errors = new List<string>() { $"Error: {(ex?.InnerException?.Message ?? ex.Message)} --> {ex?.StackTrace}" };
        return BadRequest(response);
#else
             response.Errors = new List<string>() { "An error occurred while processing your request!"};
             return BadRequest(response);
#endif
    }
}