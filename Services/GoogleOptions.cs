namespace MentalHealthSupport.Services;

/// <summary>
/// Configurações necessárias para o uso do Gemini (ou outro provedor Google).
/// </summary>
public class GoogleOptions
{
    /// <summary>
    /// Chave de API (ou token) fornecida pelo Google Cloud.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// ID do projeto no Google Cloud associado à API.
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// Nome/modelo do Gemini a ser utilizado (ex: "models/text-bison-001").
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Endpoint HTTP para a API de Gemini. Deve incluir caminho completo.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Prompt de sistema a ser enviado ao modelo.
    /// </summary>
    public string? SystemPrompt { get; set; }
}