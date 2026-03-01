# 🗄️ Migrando para Banco de Dados Com Async (Entity Framework Core)

## Por Que Migrar de InMemory?

| Aspecto | In-Memory | Banco de Dados Real |
|---------|-----------|-------------------|
| **Persistência** | ❌ Perde ao reiniciar | ✅ Permanente |
| **Escalabilidade** | ❌ Limitada à RAM | ✅ Ilimitada |
| **Concorrência** | ⚠️ Manual | ✅ Automática |
| **Backups** | ❌ Não | ✅ Sim |
| **Produção** | ❌ Não recomendado | ✅ Recomendado |

## Passo 1: Instalar Entity Framework Core

```bash
# Instalar pacotes necessários
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design

# Para PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# OU para SQL Server
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# OU para MySQL
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

## Passo 2: Criar DbContext

Crie um arquivo `Data/MentalHealthDbContext.cs`:

```csharp
using MentalHealthSupport.Models;
using Microsoft.EntityFrameworkCore;

namespace MentalHealthSupport.Data;

public class MentalHealthDbContext : DbContext
{
    public MentalHealthDbContext(DbContextOptions<MentalHealthDbContext> options)
        : base(options)
    {
    }

    public DbSet<Professional> Professionals { get; set; } = null!;
    public DbSet<SupportRequest> SupportRequests { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar Professional
        modelBuilder.Entity<Professional>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Specialty).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.ContactEmail).HasMaxLength(256);
            entity.Property(e => e.CrpOrCrm).HasMaxLength(50);

            // Índices para busca rápida
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.Specialty);
            entity.HasIndex(e => e.ContactEmail).IsUnique();
        });

        // Configurar SupportRequest
        modelBuilder.Entity<SupportRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índices
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Email);
        });

        // Seed de dados iniciais
        SeedInitialData(modelBuilder);
    }

    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Professional>().HasData(
            new Professional
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
            new Professional
            {
                Id = 2,
                Name = "Marcos Machado",
                Specialty = "Psiquiatria",
                Approach = "Acompanhamento medicamentoso integrado com psicoterapia.",
                City = "Rio de Janeiro",
                ContactEmail = "marcos.lima@example.com",
                OnlineSessions = false,
                CrpOrCrm = "CRM 00/00000"
            }
        );
    }
}
```

## Passo 3: Criar Repository com EF Core (Async)

Crie `Services/EFMentalHealthRepository.cs`:

```csharp
using MentalHealthSupport.Data;
using MentalHealthSupport.Models;
using Microsoft.EntityFrameworkCore;

namespace MentalHealthSupport.Services;

public class EFMentalHealthRepository : IMentalHealthRepository
{
    private readonly MentalHealthDbContext _context;
    private readonly ILogger<EFMentalHealthRepository> _logger;

    public EFMentalHealthRepository(
        MentalHealthDbContext context,
        ILogger<EFMentalHealthRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // PROFESSIONALS
    public async Task<IReadOnlyCollection<Professional>> GetProfessionalsAsync()
    {
        try
        {
            var professionals = await _context.Professionals
                .OrderBy(p => p.Name)
                .AsNoTracking() // Não precisa rastrear em memória
                .ToListAsync();

            return professionals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar profissionais");
            throw;
        }
    }

    public async Task<Professional?> GetProfessionalAsync(int id)
    {
        try
        {
            var professional = await _context.Professionals
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return professional;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar profissional com id {Id}", id);
            throw;
        }
    }

    public async Task<Professional> AddProfessionalAsync(Professional prof)
    {
        try
        {
            _context.Professionals.Add(prof);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Profissional criado: {ProfessionalId}", prof.Id);
            return prof;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar profissional");
            throw;
        }
    }

    // SUPPORT REQUESTS
    public async Task<IReadOnlyCollection<SupportRequest>> GetSupportRequestsAsync()
    {
        try
        {
            var requests = await _context.SupportRequests
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return requests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar solicitações de suporte");
            throw;
        }
    }

    public async Task<SupportRequest> AddSupportRequestAsync(SupportRequest request)
    {
        try
        {
            request.CreatedAt = DateTime.UtcNow;
            _context.SupportRequests.Add(request);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Solicitação de suporte criada: {RequestId}", request.Id);
            return request;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar solicitação de suporte");
            throw;
        }
    }
}
```

## Passo 4: Configurar no Program.cs

```csharp
using MentalHealthSupport.Data;
using MentalHealthSupport.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// DbContext
builder.Services.AddDbContext<MentalHealthDbContext>(options =>
{
    // PostgreSQL
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Ou SQL Server
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Habilitar logging de queries em desenvolvimento
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// Repository - Mudar de InMemory para EF
builder.Services.AddScoped<IMentalHealthRepository, EFMentalHealthRepository>();

var app = builder.Build();

// Aplicar migrações automaticamente em desenvolvimento
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MentalHealthDbContext>();
    db.Database.Migrate();
}

// ... resto da configuração
```

## Passo 5: Configurar Connection String

Em `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mentalhealthsupport;Username=postgres;Password=sua_senha"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

Em `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

Para produção, use **AWS Secrets Manager** ou variáveis de ambiente:

```csharp
// No Program.cs (para Secrets Manager)
if (!app.Environment.IsDevelopment())
{
    var secretClient = new Amazon.SecretsManager.AmazonSecretsManagerClient();
    var secret = await secretClient.GetSecretValueAsync(
        new Amazon.SecretsManager.Model.GetSecretValueRequest
        {
            SecretId = "mentalhealthsupport/db-connection"
        });
    
    var connectionString = secret.SecretString;
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
}
```

## Passo 6: Criar Migrações

```bash
# Criar primeira migração
dotnet ef migrations add InitialCreate

# Aplicar migração
dotnet ef database update

# Ver status
dotnet ef migrations list
```

## Passo 7: Migração de Dados (Se necessário)

```csharp
// MigrationService.cs - Migrar dados de InMemory para BD
public class DataMigrationService
{
    private readonly InMemoryMentalHealthRepository _inMemory;
    private readonly MentalHealthDbContext _dbContext;

    public async Task MigrateAsync()
    {
        // Copiar profissionais
        var professionals = await _inMemory.GetProfessionalsAsync();
        foreach (var prof in professionals)
        {
            _dbContext.Professionals.Add(prof);
        }

        // Copiar requests
        var requests = await _inMemory.GetSupportRequestsAsync();
        foreach (var req in requests)
        {
            _dbContext.SupportRequests.Add(req);
        }

        await _dbContext.SaveChangesAsync();
    }
}
```

## Otimizações de Query com EF Core

### 1. AsNoTracking (Leitura apenas)
```csharp
var professionals = await _context.Professionals
    .AsNoTracking()  // Não rastreia mudanças
    .ToListAsync();
```

### 2. Select Projections
```csharp
// Buscar apenas campos necessários
var names = await _context.Professionals
    .Select(p => new { p.Id, p.Name })
    .ToListAsync();
```

### 3. Include (Eager loading)
```csharp
var professionals = await _context.Professionals
    .Include(p => p.UserProfile)
    .ToListAsync();
```

### 4. Paginação
```csharp
public async Task<IReadOnlyCollection<Professional>> GetProfessionalsAsync(
    int pageNumber = 1,
    int pageSize = 10)
{
    return await _context.Professionals
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToListAsync();
}
```

### 5. Filtros Dinâmicos
```csharp
public async Task<IReadOnlyCollection<Professional>> SearchAsync(
    string? city = null,
    string? specialty = null)
{
    var query = _context.Professionals.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(city))
        query = query.Where(p => p.City.Contains(city));

    if (!string.IsNullOrWhiteSpace(specialty))
        query = query.Where(p => p.Specialty.Contains(specialty));

    return await query.ToListAsync();
}
```

## Testes com EF Core

```csharp
[TestClass]
public class EFRepositoryAsyncTests
{
    private MentalHealthDbContext _context = null!;
    private EFMentalHealthRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<MentalHealthDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _context = new MentalHealthDbContext(options);
        _repository = new EFMentalHealthRepository(_context, new MockLogger());
    }

    [Testcleanup]
    public void Cleanup()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }

    [TestMethod]
    public async Task AddAndRetrieve_Professional()
    {
        // Arrange
        var prof = new Professional { Name = "Test" };

        // Act
        await _repository.AddProfessionalAsync(prof);
        var retrieved = await _repository.GetProfessionalAsync(prof.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Test", retrieved.Name);
    }
}
```

## Checklist de Migração

- [ ] Instalar Entity Framework Core
- [ ] Criar DbContext
- [ ] Criar novo Repository
- [ ] Atualizar Program.cs
- [ ] Configurar Connection Strings
- [ ] Criar e aplicar migrações
- [ ] Testar com banco de dados real
- [ ] Migrar dados de In-Memory (se necessário)
- [ ] Testar em produção com AWS RDS
- [ ] Configurar backups

## Recursos Úteis

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [EF Core Async Methods](https://learn.microsoft.com/en-us/ef/core/querying/async)
- [AWS RDS Setup Guide](https://docs.aws.amazon.com/rds/)
- [PostgreSQL + EF Core](https://www.npgsql.org/efcore/)

---

**Quando estiver pronto, siga este guia para atualizar seu projeto com um banco de dados de verdade! 🚀**
