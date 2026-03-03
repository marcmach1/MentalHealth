using MentalHealthSupport.Models;
using MentalHealthSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthSupport.Controllers;

/// <summary>
/// API para suporte emocional assistido por IA.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AiSupportApiController(IAiSupportService aiSupportService) : ControllerBase
{
    /// <summary>
    /// Processa a pergunta do usuário e retorna uma resposta de apoio emocional.
    /// </summary>
    /// <param name="request">Requisição contendo a pergunta do usuário.</param>
    /// <returns>Resposta de apoio emocional da IA.</returns>
    /// <response code="200">Resposta de apoio gerada com sucesso (ou fallback se integração não estiver configurada).</response>
    /// <response code="400">Requisição inválida (pergunta vazia).</response>
    [HttpPost]
    public async Task<ActionResult<AiSupportResponse>> Post([FromBody] AiSupportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { message = "A pergunta não pode estar vazia." });
        }

        var response = await aiSupportService.GetSupportResponseAsync(request.Question);
        return Ok(response);
    }
}

