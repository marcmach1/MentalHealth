using MentalHealthSupport.Models;
using MentalHealthSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthSupport.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportRequestsApiController(IMentalHealthRepository repository) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<SupportRequest>> GetAll()
        => Ok(repository.GetSupportRequests());

    [HttpPost]
    public ActionResult<SupportRequest> Create([FromBody] SupportRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = repository.AddSupportRequest(request);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}
