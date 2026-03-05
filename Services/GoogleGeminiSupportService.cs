using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MentalHealthSupport.Models;
using Microsoft.Extensions.Options;

namespace MentalHealthSupport.Services;

/// <summary>
/// Implementação de IA usando o Gemini (Google Cloud).
/// A interface é a mesma de <see cref="OpenAiSupportService"/>, permitindo
/// troca de provedor sem alterar o restante da aplicação.
/// </summary>
public class GoogleGeminiSupportService : IAiSupportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GoogleOptions _options;
    private readonly ILogger<GoogleGeminiSupportService> _logger;

    public GoogleGeminiSupportService(
        IHttpClientFactory httpClientFactory,
        IOptions<GoogleOptions> options,
        ILogger<GoogleGeminiSupportService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiSupportResponse> GetSupportResponseAsync(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
        }

        if (!IsConfigured())
        {
            _logger.LogWarning("Gemini (Google) não está configurado. Retornando fallback.");
            return new AiSupportResponse { Answer = AiSupportMessages.NotConfiguredFallback };
        }

        try
        {
            return await CallGeminiAsync(question);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de conexão ao chamar serviço Gemini.");
            return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao processar resposta JSON do Gemini.");
            return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao chamar serviço Gemini.");
            return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
        }
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_options.ApiKey)
               && !string.IsNullOrWhiteSpace(_options.Endpoint);
    }

    private async Task<AiSupportResponse> CallGeminiAsync(string question)
    {
        using var client = _httpClientFactory.CreateClient();

        var apiKey = _options.ApiKey;
        var systemPrompt = _options.SystemPrompt ?? AiSupportMessages.DefaultSystemPrompt;
        string endpoint = _options.Endpoint;

        // Se a chave for API key (AIza), sempre usar v1beta2
        if (!string.IsNullOrWhiteSpace(apiKey) && apiKey.StartsWith("AIza"))
        {
            endpoint = $"https://generativelanguage.googleapis.com/v1beta2/models/{(_options.Model ?? "text-bison-001")}:generate";
            var ub = new UriBuilder(endpoint);
            var existing = ub.Query;
            if (!string.IsNullOrEmpty(existing) && existing.StartsWith("?")) existing = existing.Substring(1);
            if (!string.IsNullOrEmpty(existing)) existing += "&";
            existing += "key=" + Uri.EscapeDataString(apiKey);
            ub.Query = existing;
            endpoint = ub.ToString();
            _logger.LogInformation("Usando endpoint Gemini v1beta2 com API key: {Endpoint}", endpoint);
        }
        else if (string.IsNullOrWhiteSpace(endpoint))
        {
            // Se não for API key, tenta construir endpoint v1 (OAuth)
            if (!string.IsNullOrWhiteSpace(_options.ProjectId) && !string.IsNullOrWhiteSpace(_options.Model))
            {
                var project = Uri.EscapeDataString(_options.ProjectId);
                var model = Uri.EscapeDataString(_options.Model);
                var location = "us-central1";
                endpoint = $"https://generativelanguage.googleapis.com/v1/projects/{project}/locations/{location}/models/{model}:generate";
                _logger.LogInformation("Construído endpoint Gemini v1 OAuth: {Endpoint}", endpoint);
            }
            else
            {
                endpoint = "https://generativelanguage.googleapis.com/v1beta2/models/text-bison-001:generate";
                _logger.LogInformation("Usando endpoint Gemini v1beta2 default: {Endpoint}", endpoint);
            }
        }
        else if (!string.IsNullOrWhiteSpace(apiKey))
        {
            // Se endpoint custom e API key, adiciona ?key= se não houver
            if (apiKey.StartsWith("AIza") && !endpoint.Contains("key="))
            {
                var ub = new UriBuilder(endpoint);
                var existing = ub.Query;
                if (!string.IsNullOrEmpty(existing) && existing.StartsWith("?")) existing = existing.Substring(1);
                if (!string.IsNullOrEmpty(existing)) existing += "&";
                existing += "key=" + Uri.EscapeDataString(apiKey);
                ub.Query = existing;
                endpoint = ub.ToString();
                _logger.LogInformation("Adicionada API key como query param ao endpoint custom Gemini: {Endpoint}", endpoint);
            }
        }
        // Se não for API key, usa Bearer
        if (!string.IsNullOrWhiteSpace(apiKey) && !apiKey.StartsWith("AIza"))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
        _logger.LogInformation("Endpoint final Gemini: {Endpoint}", endpoint);

        // corpo simplificado, adaptável conforme a documentação real do Gemini
        var payload = new
        {
            // se o modelo for passado no endpoint, omite; caso contrário inclua aqui
            //model,
            prompt = new
            {
                text = systemPrompt + "\n\nUsuário: " + question
            }
        };

        var httpResponse = await client.PostAsJsonAsync(endpoint, payload);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync();
            _logger.LogWarning("Gemini returned non-success status {StatusCode} from {Endpoint}: {Body}", httpResponse.StatusCode, endpoint, errorBody);

            // Se for 404 e há um modelo configurado, tente um endpoint alternativo comum
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound && !string.IsNullOrWhiteSpace(_options.Model))
            {
                try
                {
                    var attemptModel = _options.Model;
                    if (attemptModel.StartsWith("models/", StringComparison.OrdinalIgnoreCase))
                        attemptModel = attemptModel.Substring("models/".Length);

                    var altEndpoint = $"https://generativelanguage.googleapis.com/v1/models/{Uri.EscapeDataString(attemptModel)}:generate";

                    // Re-apply API key as query param if needed
                    if (!string.IsNullOrWhiteSpace(_options.ApiKey) && _options.ApiKey.StartsWith("AIza"))
                    {
                        var ub2 = new UriBuilder(altEndpoint);
                        var existing2 = ub2.Query;
                        if (!string.IsNullOrEmpty(existing2) && existing2.StartsWith("?")) existing2 = existing2.Substring(1);
                        if (!string.IsNullOrEmpty(existing2)) existing2 += "&";
                        existing2 += "key=" + Uri.EscapeDataString(_options.ApiKey);
                        ub2.Query = existing2;
                        altEndpoint = ub2.ToString();
                    }

                    _logger.LogInformation("Tentando endpoint alternativo Gemini: {AltEndpoint}", altEndpoint);
                    var altResponse = await client.PostAsJsonAsync(altEndpoint, payload);
                    if (altResponse.IsSuccessStatusCode)
                    {
                        httpResponse = altResponse;
                    }
                    else
                    {
                        var altBody = await altResponse.Content.ReadAsStringAsync();
                        _logger.LogError("Endpoint alternativo Gemini também falhou {StatusCode}: {Body}", altResponse.StatusCode, altBody);
                        return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao tentar endpoint alternativo do Gemini.");
                    return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
                }
            }
            else
            {
                _logger.LogError("Gemini returned non-success status {StatusCode}: {Body}", httpResponse.StatusCode, errorBody);
                return new AiSupportResponse { Answer = AiSupportMessages.ServiceErrorFallback };
            }
        }

        var json = await httpResponse.Content.ReadFromJsonAsync<JsonElement>();
        // extrai campo de texto dependendo da estrutura de resposta real
        string answer = string.Empty;
        if (json.TryGetProperty("output", out var output)
            && output.ValueKind == JsonValueKind.Object
            && output.TryGetProperty("text", out var textProp))
        {
            answer = textProp.GetString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(answer))
        {
            answer = AiSupportMessages.NoResponseFallback;
        }

        return new AiSupportResponse { Answer = answer };
    }
}