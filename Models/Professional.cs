using System.ComponentModel.DataAnnotations;

namespace MentalHealthSupport.Models;

/// <summary>
/// Representa um profissional de saúde mental cadastrado na plataforma.
/// </summary>
public class Professional
{
    /// <summary>
    /// ID único do profissional.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome completo do profissional.
    /// </summary>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Especialidade do profissional (ex: Psicologia, Psiquiatria).
    /// </summary>
    [Required(ErrorMessage = "A especialidade é obrigatória.")]
    [Display(Name = "Especialidade")]
    public string Specialty { get; set; } = string.Empty;

    /// <summary>
    /// Abordagem ou método terapêutico utilizado.
    /// </summary>
    [Display(Name = "Abordagem / método")]
    public string? Approach { get; set; }

    /// <summary>
    /// Cidade onde o profissional atua.
    /// </summary>
    [Display(Name = "Cidade")]
    public string? City { get; set; }

    /// <summary>
    /// Email de contato do profissional.
    /// </summary>
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail de contato")]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Indica se o profissional oferece atendimento online.
    /// </summary>
    [Display(Name = "Atende online?")]
    public bool OnlineSessions { get; set; }

    /// <summary>
    /// Número de registro profissional (CRP para psicólogos, CRM para médicos).
    /// </summary>
    [Display(Name = "CRP ou CRM")]
    public string? CrpOrCrm { get; set; }
}
