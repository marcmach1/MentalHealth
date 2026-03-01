using MentalHealthSupport.Models;
using MentalHealthSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthSupport.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfessionalsApiController(IMentalHealthRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Professional>>> GetAll()
    {
        var professionals = await repository.GetProfessionalsAsync();
        return Ok(professionals);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Professional>> GetById(int id)
    {
        var professional = await repository.GetProfessionalAsync(id);
        return professional is null ? NotFound() : Ok(professional);
    }

    [HttpPost]
    public async Task<ActionResult<Professional>> Create([FromBody] Professional prof)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await repository.AddProfessionalAsync(prof);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
