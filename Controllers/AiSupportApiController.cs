using System.Net.Http.Headers;
using System.Net.Http.Json;
using MentalHealthSupport.Models;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthSupport.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiSupportApiController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AiSupportApiController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AiSupportResponse>> Post([FromBody] AiSupportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest();
        }
        
        // Configuração da OpenAI vinda do appsettings
        var apiKey = configuration["OpenAI:ApiKey"];
        // Endpoint padrão da API de chat completions, pode ser sobrescrito em configuração
        var endpoint = configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
        // Modelo default; pode ser sobrescrito em configuração
        var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(endpoint))
        {
            var fallback = new AiSupportResponse
            {
                Answer =
                    "Obrigado por compartilhar o que você está sentindo.\n\n" +
                    "No momento, a integração com a IA ainda não está configurada neste ambiente, " +
                    "mas aqui vão algumas orientações gerais:\n" +
                    "- Procure alguém de confiança para conversar sobre o que você está passando.\n" +
                    "- Se estiver em sofrimento intenso ou com pensamentos de autoagressão, " +
                    "busque imediatamente um serviço de emergência ou um profissional de saúde mental.\n\n" +
                    "Quando a integração com a IA for configurada, esta resposta será gerada de forma personalizada " +
                    "com base na sua mensagem."
            };

            return Ok(fallback);
        }

        try
        {
            using var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content =
                            "Você é um assistente de apoio emocional em língua portuguesa. " +
                            "Ofereça acolhimento, valide sentimentos e sugira caminhos saudáveis, " +
                            "sempre reforçando que sua resposta não substitui acompanhamento profissional. " +
                            "Se a pessoa mencionar risco à própria vida ou à de terceiros, " +
                            "oriente a procurar imediatamente serviços de emergência da região."
                    },
                    new
                    {
                        role = "user",
                        content = request.Question
                    }
                }
            };

            var httpResponse = await client.PostAsJsonAsync(endpoint, payload);
            httpResponse.EnsureSuccessStatusCode();

            var json = await httpResponse.Content.ReadFromJsonAsync<dynamic>();
            string? answer = json?.choices?[0]?.message?.content;

            if (string.IsNullOrWhiteSpace(answer))
            {
                answer = "A IA não conseguiu gerar uma resposta no momento. Tente novamente em alguns instantes.";
            }

            return Ok(new AiSupportResponse { Answer = answer });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao chamar serviço de IA.");
            return Ok(new AiSupportResponse
            {
                Answer =
                    "Não foi possível falar com a IA agora. Isso pode ser algo temporário do serviço externo.\n\n" +
                    "Se estiver em sofrimento intenso, por favor procure um profissional de saúde mental " +
                    "ou um serviço de emergência na sua região."
            });
        }
    }
}

