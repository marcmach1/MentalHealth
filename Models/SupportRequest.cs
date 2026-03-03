namespace MentalHealthSupport.Models;

/// <summary>
/// Representa uma solicitação de apoio enviada por um usuário.
/// </summary>
public class SupportRequest
{
    /// <summary>
    /// ID único da solicitação de suporte.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID do usuário que fez a solicitação (opcional).
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Nome do usuário que fez a solicitação.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário que fez a solicitação.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Cidade do usuário (opcional).
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Tópico geral da solicitação (ex: Ansiedade, Depressão).
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo detalhado da solicitação de suporte.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora em UTC cuando a solicitação foi criada.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID do profissional atribuído à solicitação (opcional).
    /// </summary>
    public int? ProfessionalId { get; set; }
}
