using MentalHealthSupport.Models;
using MentalHealthSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthSupport.Controllers;

public class ProfessionalsController : Controller
{
    private readonly IMentalHealthRepository _repository;

    public ProfessionalsController(IMentalHealthRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Professional prof)
    {
        if (!ModelState.IsValid)
        {
            return View(prof);
        }

        _repository.AddProfessional(prof);
        TempData["SuccessMessage"] = "Obrigado pelo cadastro! Logo sua informação estará disponível para usuários.";
        // redirect to clear form
        return RedirectToAction(nameof(Create));
    }
}
