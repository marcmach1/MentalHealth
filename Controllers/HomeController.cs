using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MentalHealthSupport.Models;

namespace MentalHealthSupport.Controllers;

/// <summary>
/// Controller para página inicial e conteúdo geral do site.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Inicializa nova instância do HomeController.
    /// </summary>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retorna a página inicial do site.
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Retorna a página de privacidade.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Retorna página de erro com informações de rastreamento da requisição.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
