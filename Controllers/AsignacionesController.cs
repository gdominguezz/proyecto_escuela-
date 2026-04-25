using Microsoft.AspNetCore.Mvc;
using proyecto_escuela.Repositories;

namespace proyecto_escuela.Controllers;

public class AsignacionesController : Controller
{
    private readonly AsignacionRepository _repo;

    public AsignacionesController(AsignacionRepository repo)
    {
        _repo = repo;
    }

    public async Task<IActionResult> Index()
    {
        var paquetes = await _repo.ObtenerPaquetesEnAlmacenAsync();
        var repartidores = await _repo.ObtenerRepartidoresActivosAsync();

        ViewBag.Paquetes = paquetes;
        ViewBag.Repartidores = repartidores;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Asignar(int paqueteId, int repartidorId)
    {
        await _repo.AsignarAsync(paqueteId, repartidorId);
        return RedirectToAction("Index");
    }
}