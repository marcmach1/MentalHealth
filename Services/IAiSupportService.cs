using MentalHealthSupport.Models;

namespace MentalHealthSupport.Services;

/// <summary>
/// Interface para serviço de suporte assistido por IA.
/// Responsável por orquestrar chamadas à API de IA e tratamento de erros.
/// </summary>
public interface IAiSupportService
{
    /// <summary>
    /// Processa uma pergunta e retorna uma resposta de apoio de IA.
    /// </summary>
    /// <param name="question">A pergunta ou preocupação do usuário.</param>
    /// <returns>Resposta de apoio de IA.</returns>
    Task<AiSupportResponse> GetSupportResponseAsync(string question);
}
