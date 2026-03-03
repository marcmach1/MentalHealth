namespace MentalHealthSupport.Services;

/// <summary>
/// Configurações para integração com OpenAI de forma type-safe.
/// </summary>
public class OpenAiOptions
{
    /// <summary>
    /// Chave de API da OpenAI.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Endpoint da API de chat completions. Padrão: https://api.openai.com/v1/chat/completions
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Modelo GPT a usar. Padrão: gpt-4o-mini
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Prompt do sistema para o assistente de apoio emocional.
    /// </summary>
    public string? SystemPrompt { get; set; }
}
