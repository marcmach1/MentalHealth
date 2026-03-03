using MentalHealthSupport.Models;

namespace MentalHealthSupport.Services;

/// <summary>
/// Interface para repositório de dados de saúde mental.
/// Define operações CRUD para profissionais e solicitações de suporte.
/// </summary>
public interface IMentalHealthRepository
{
    /// <summary>
    /// Obtém lista de todos os profissionais cadastrados.
    /// </summary>
    Task<IReadOnlyCollection<Professional>> GetProfessionalsAsync();

    /// <summary>
    /// Obtém um profissional específico pelo ID.
    /// </summary>
    /// <param name="id">ID do profissional.</param>
    /// <returns>Profissional encontrado ou null.</returns>
    Task<Professional?> GetProfessionalAsync(int id);

    /// <summary>
    /// Adiciona um novo profissional ao repositório.
    /// </summary>
    /// <param name="prof">Dados do novo profissional.</param>
    /// <returns>Profissional criado com ID gerado.</returns>
    Task<Professional> AddProfessionalAsync(Professional prof);

    /// <summary>
    /// Obtém lista de todas as solicitações de suporte.
    /// </summary>
    Task<IReadOnlyCollection<SupportRequest>> GetSupportRequestsAsync();

    /// <summary>
    /// Adiciona uma nova solicitação de suporte.
    /// </summary>
    /// <param name="request">Dados da solicitação.</param>
    /// <returns>Solicitação criada com ID e timestamp gerados.</returns>
    Task<SupportRequest> AddSupportRequestAsync(SupportRequest request);
}

/// <summary>
/// Implementação em memória do repositório de saúde mental.
/// Armazena dados em coleções List em memória, ideal para prototipagem e testes.
/// </summary>
public class InMemoryMentalHealthRepository : IMentalHealthRepository
{
    private readonly List<Professional> _professionals =
    [
        new()
        {
            Id = 1,
            Name = "Ana Paula Souza",
            Specialty = "Psicologia Cognitivo-Comportamental",
            Approach = "Foco em ansiedade, depressão e burnout.",
            City = "São Paulo",
            ContactEmail = "ana.souza@example.com",
            OnlineSessions = true,
            CrpOrCrm = "CRP 00/00000"
        },
        new()
        {
            Id = 2,
            Name = "Marcos Lima",
            Specialty = "Psiquiatria",
            Approach = "Acompanhamento medicamentoso integrado com psicoterapia.",
            City = "Rio de Janeiro",
            ContactEmail = "marcos.lima@example.com",
            OnlineSessions = false,
            CrpOrCrm = "CRM 00/00000"
        }
    ];

    private readonly List<SupportRequest> _supportRequests = [];
    private int _nextRequestId = 1;

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Professional>> GetProfessionalsAsync()
    {
        return Task.FromResult<IReadOnlyCollection<Professional>>(_professionals);
    }

    /// <inheritdoc />
    public Task<Professional?> GetProfessionalAsync(int id)
    {
        var result = _professionals.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<Professional> AddProfessionalAsync(Professional prof)
    {
        var nextId = _professionals.Count == 0 ? 1 : _professionals.Max(p => p.Id) + 1;
        prof.Id = nextId;
        _professionals.Add(prof);
        return Task.FromResult(prof);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<SupportRequest>> GetSupportRequestsAsync()
    {
        var result = _supportRequests
            .OrderByDescending(r => r.CreatedAt)
            .ToList() as IReadOnlyCollection<SupportRequest>;
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<SupportRequest> AddSupportRequestAsync(SupportRequest request)
    {
        request.Id = _nextRequestId++;
        request.CreatedAt = DateTime.UtcNow;
        _supportRequests.Add(request);
        return Task.FromResult(request);
    }
}
