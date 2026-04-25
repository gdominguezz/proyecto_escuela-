using Microsoft.AspNetCore.Mvc;
using proyecto_escuela.Repositories;

namespace proyecto_escuela.Controllers;

public class EntregasController : Controller
{
    private readonly PaqueteRepository _repo;

    public EntregasController(PaqueteRepository repo)
    {
        _repo = repo;
    }

    public async Task<IActionResult> Index(string? codigo)
    {
        var enRuta = await _repo.ObtenerEnRutaAsync();
        ViewBag.EnRuta = enRuta;
        ViewBag.Codigo = codigo;

        if (!string.IsNullOrEmpty(codigo))
        {
            var paquete = await _repo.ObtenerPorIdUnicoAsync(codigo.Trim().ToUpper());
            ViewBag.Paquete = paquete;
            ViewBag.NoEncontrado = paquete == null;
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Entregar(int paqueteId, string? comentario, string idUnico)
    {
        await _repo.EntregarAsync(paqueteId, comentario);
        TempData["Entregado"] = idUnico;
        return RedirectToAction("Index");
    }
}