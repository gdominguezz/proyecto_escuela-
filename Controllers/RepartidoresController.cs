using Microsoft.AspNetCore.Mvc;
using proyecto_escuela.Models;
using proyecto_escuela.Repositories;

namespace proyecto_escuela.Controllers;

public class RepartidoresController : Controller
{
    private readonly RepartidorRepository _repo;

    public RepartidoresController(RepartidorRepository repo)
    {
        _repo = repo;
    }

    public async Task<IActionResult> Index()
    {
        var repartidores = await _repo.ObtenerTodosAsync();
        return View(repartidores);
    }

    [HttpPost]
    public async Task<IActionResult> Agregar(Repartidor repartidor)
    {
        await _repo.AgregarAsync(repartidor);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        await _repo.ToggleActivoAsync(id);
        return RedirectToAction("Index");
    }
}