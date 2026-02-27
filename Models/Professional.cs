using System.ComponentModel.DataAnnotations;

namespace MentalHealthSupport.Models;

public class Professional
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A especialidade é obrigatória.")]
    [Display(Name = "Especialidade")]
    public string Specialty { get; set; } = string.Empty;

    [Display(Name = "Abordagem / método")]
    public string? Approach { get; set; }

    [Display(Name = "Cidade")]
    public string? City { get; set; }

    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail de contato")]
    public string? ContactEmail { get; set; }

    [Display(Name = "Atende online?")]
    public bool OnlineSessions { get; set; }

    [Display(Name = "CRP ou CRM")]
    public string? CrpOrCrm { get; set; }
}
