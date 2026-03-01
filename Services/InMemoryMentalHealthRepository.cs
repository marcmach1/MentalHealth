using MentalHealthSupport.Models;

namespace MentalHealthSupport.Services;

public interface IMentalHealthRepository
{
    Task<IReadOnlyCollection<Professional>> GetProfessionalsAsync();
    Task<Professional?> GetProfessionalAsync(int id);
    Task<Professional> AddProfessionalAsync(Professional prof);

    Task<IReadOnlyCollection<SupportRequest>> GetSupportRequestsAsync();
    Task<SupportRequest> AddSupportRequestAsync(SupportRequest request);
}

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

    public Task<IReadOnlyCollection<Professional>> GetProfessionalsAsync()
    {
        // Simular latência de I/O assincron
        return Task.FromResult<IReadOnlyCollection<Professional>>(_professionals);
    }

    public Task<Professional?> GetProfessionalAsync(int id)
    {
        // Simular latência de I/O assincron
        var result = _professionals.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(result);
    }

    public Task<Professional> AddProfessionalAsync(Professional prof)
    {
        // Simular latência de I/O assincron
        var nextId = _professionals.Count == 0 ? 1 : _professionals.Max(p => p.Id) + 1;
        prof.Id = nextId;
        _professionals.Add(prof);
        return Task.FromResult(prof);
    }

    public Task<IReadOnlyCollection<SupportRequest>> GetSupportRequestsAsync()
    {
        // Simular latência de I/O assincron
        var result = _supportRequests
            .OrderByDescending(r => r.CreatedAt)
            .ToList() as IReadOnlyCollection<SupportRequest>;
        return Task.FromResult(result);
    }

    public Task<SupportRequest> AddSupportRequestAsync(SupportRequest request)
    {
        // Simular latência de I/O assincron
        request.Id = _nextRequestId++;
        request.CreatedAt = DateTime.UtcNow;
        _supportRequests.Add(request);
        return Task.FromResult(request);
    }
}
