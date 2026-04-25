using Microsoft.AspNetCore.Mvc;
using proyecto_escuela.Models;
using proyecto_escuela.Repositories;

namespace proyecto_escuela.Controllers;

public class PaquetesController : Controller
{
    private readonly PaqueteRepository _repo;

    public PaquetesController(PaqueteRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult Registrar() => View();

    [HttpPost]
    public async Task<IActionResult> Registrar(Paquete paquete)
    {
        var idUnico = await _repo.RegistrarAsync(paquete);
        return RedirectToAction("Etiqueta", new { idUnico });
    }

    public async Task<IActionResult> Etiqueta(string idUnico)
    {
        var paquete = await _repo.ObtenerPorIdUnicoAsync(idUnico);
        if (paquete == null) return NotFound();
        return View(paquete);
    }
    public async Task<IActionResult> Index()
    {
        var paquetes = await _repo.ObtenerTodosAsync();
        return View(paquetes);
    }
}