using MentalHealthSupport.Models;

namespace MentalHealthSupport.Services;

public interface IMentalHealthRepository
{
    IReadOnlyCollection<Professional> GetProfessionals();
    Professional? GetProfessional(int id);
    Professional AddProfessional(Professional prof);

    IReadOnlyCollection<SupportRequest> GetSupportRequests();
    SupportRequest AddSupportRequest(SupportRequest request);
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

    public IReadOnlyCollection<Professional> GetProfessionals() => _professionals;

    public Professional? GetProfessional(int id) => _professionals.FirstOrDefault(p => p.Id == id);

    public Professional AddProfessional(Professional prof)
    {
        // assign a new id
        var nextId = _professionals.Count == 0 ? 1 : _professionals.Max(p => p.Id) + 1;
        prof.Id = nextId;
        _professionals.Add(prof);
        return prof;
    }

    public IReadOnlyCollection<SupportRequest> GetSupportRequests() => _supportRequests
        .OrderByDescending(r => r.CreatedAt)
        .ToList();

    public SupportRequest AddSupportRequest(SupportRequest request)
    {
        request.Id = _nextRequestId++;
        request.CreatedAt = DateTime.UtcNow;
        _supportRequests.Add(request);
        return request;
    }
}
