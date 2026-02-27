using MentalHealthSupport.Models;
using MentalHealthSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthSupport.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfessionalsApiController(IMentalHealthRepository repository) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Professional>> GetAll()
        => Ok(repository.GetProfessionals());

    [HttpGet("{id:int}")]
    public ActionResult<Professional> GetById(int id)
    {
        var professional = repository.GetProfessional(id);
        return professional is null ? NotFound() : Ok(professional);
    }

    [HttpPost]
    public ActionResult<Professional> Create([FromBody] Professional prof)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = repository.AddProfessional(prof);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
