namespace MentalHealthSupport.Models;

/// <summary>
/// Requisição de suporte emocional assistido por IA.
/// </summary>
public class AiSupportRequest
{
    /// <summary>
    /// Pergunta ou preocupação do usuário para a IA processar.
    /// </summary>
    public string Question { get; set; } = string.Empty;
}

/// <summary>
/// Resposta de suporte emocional fornecida pela IA.
/// </summary>
public class AiSupportResponse
{
    /// <summary>
    /// Resposta de apoio emocional em português para a pergunta do usuário.
    /// </summary>
    public string Answer { get; set; } = string.Empty;
}

