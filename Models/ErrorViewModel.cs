namespace proyecto_escuela.Models;

public class Paquete
{
    public int Id { get; set; }
    public string IdUnico { get; set; } = string.Empty;
    public string Remitente { get; set; } = string.Empty;
    public string Destinatario { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public string Estado { get; set; } = "EN_ALMACEN";
    public decimal Peso { get; set; }
    public string? Comentarios { get; set; }
}