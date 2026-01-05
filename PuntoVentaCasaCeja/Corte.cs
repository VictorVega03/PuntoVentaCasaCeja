using System;

public class Corte
{
    public int id { get; set; }
    public string folio_corte { get; set; }
    public decimal fondo_apertura { get; set; }
    public decimal total_efectivo { get; set; }
    public decimal total_tarjetas_debito { get; set; }
    public decimal total_tarjetas_credito { get; set; }
    public decimal total_cheques { get; set; }
    public decimal total_transferencias { get; set; }
    public decimal efectivo_apartados { get; set; }
    public decimal efectivo_creditos { get; set; }
    public string gastos { get; set; }
    public string ingresos { get; set; }
    public decimal sobrante { get; set; }
    public string fecha_apertura_caja { get; set; }
    public string fecha_corte_caja { get; set; }
    public int sucursal_id { get; set; }
    public int usuario_id { get; set; }
    public int estado { get; set; }
    public string detalles { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }

    // Nuevas propiedades agregadas
    public decimal total_apartados { get; set; }
    public decimal total_creditos { get; set; }
}