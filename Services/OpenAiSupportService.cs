using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MentalHealthSupport.Models;
using Microsoft.Extensions.Options;

namespace MentalHealthSupport.Services;

/// <summary>
/// Implementação de serviço de suporte com OpenAI.
/// Encapsula a lógica de chamada à API e tratamento de erros.
/// </summary>
public class OpenAiSupportService : IAiSupportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiSupportService> _logger;

    /// <summary>
    /// Inicializa nova instância do serviço de suporte por IA.
    /// </summary>
    public OpenAiSupportService(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiSupportService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Processa uma pergunta do usuário chamando a API de OpenAI.
    /// </summary>
    /// <remarks>
    /// Se a integração não estiver configurada ou houver erro, retorna mensagens genéricas de fallback
    /// que orientam o usuário a procurar apoio profissional.
    /// </remarks>
    public async Task<AiSupportResponse> GetSupportResponseAsync(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
        }

        // Validar configuração
        if (!IsConfigured())
        {
            _logger.LogWarning("OpenAI não está configurado. Retornando fallback.");
            return new AiSupportResponse { Answer = AiSupportMessages.NotConfiguredFallback };
        }

        try
        {
            return await CallOpenAiAsync(question);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de conexão ao chamar serviço de IA da OpenAI.");
            return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao processar resposta JSON da OpenAI.");
            return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao chamar serviço de IA.");
            return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
        }
    }

    /// <summary>
    /// Verifica se a integração OpenAI está adequadamente configurada.
    /// </summary>
    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_options.ApiKey) &&
               !string.IsNullOrWhiteSpace(_options.Endpoint);
    }

    /// <summary>
    /// Chama a API da OpenAI com a pergunta do usuário.
    /// </summary>
    private async Task<AiSupportResponse> CallOpenAiAsync(string question)
    {
        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var endpoint = _options.Endpoint ?? "https://api.openai.com/v1/chat/completions";
        var model = _options.Model ?? "gpt-4o-mini";
        var systemPrompt = _options.SystemPrompt ?? AiSupportMessages.DefaultSystemPrompt;

        var payload = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = question }
            }
        };

        var httpResponse = await client.PostAsJsonAsync(endpoint, payload);
        httpResponse.EnsureSuccessStatusCode();

        var json = await httpResponse.Content.ReadFromJsonAsync<dynamic>();
        string? answer = json?.choices?[0]?.message?.content;

        if (string.IsNullOrWhiteSpace(answer))
        {
            answer = AiSupportMessages.NoResponseFallback;
        }

        return new AiSupportResponse { Answer = answer };
    }
}
