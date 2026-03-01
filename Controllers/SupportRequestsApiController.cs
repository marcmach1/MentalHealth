using MentalHealthSupport.Models;
using MentalHealthSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthSupport.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportRequestsApiController(IMentalHealthRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupportRequest>>> GetAll()
    {
        var requests = await repository.GetSupportRequestsAsync();
        return Ok(requests);
    }

    [HttpPost]
    public async Task<ActionResult<SupportRequest>> Create([FromBody] SupportRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await repository.AddSupportRequestAsync(request);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}
