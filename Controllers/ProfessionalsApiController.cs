using MentalHealthSupport.Models;
using MentalHealthSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthSupport.Controllers;

/// <summary>
/// API para gerenciar profissionais de saúde mental.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProfessionalsApiController(IMentalHealthRepository repository) : ControllerBase
{
    /// <summary>
    /// Lista todos os profissionais de saúde mental disponíveis.
    /// </summary>
    /// <returns>Lista de profissionais.</returns>
    /// <response code="200">Lista de profissionais retornada com sucesso.</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Professional>>> GetAll()
    {
        var professionals = await repository.GetProfessionalsAsync();
        return Ok(professionals);
    }

    /// <summary>
    /// Obtém os detalhes de um profissional específico.
    /// </summary>
    /// <param name="id">ID do profissional.</param>
    /// <returns>Dados do profissional.</returns>
    /// <response code="200">Profissional encontrado.</response>
    /// <response code="404">Profissional não encontrado.</response>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Professional>> GetById(int id)
    {
        var professional = await repository.GetProfessionalAsync(id);
        return professional is null ? NotFound() : Ok(professional);
    }

    /// <summary>
    /// Adiciona um novo profissional à plataforma.
    /// </summary>
    /// <param name="prof">Dados do novo profissional.</param>
    /// <returns>Profissional criado com ID gerado.</returns>
    /// <response code="201">Profissional criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
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
