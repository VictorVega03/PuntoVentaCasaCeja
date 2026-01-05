using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuntoVentaCasaCeja
{
    public class CurrentData
    {
        public WebDataManager webDM { get; set; }
        public Cliente cliente { get; set; }
        public string sucursalDir { get; set; }
        public string folioCorte { get; set; }
        public string sucursalName { get; set; }
        public List<ProductoVenta> carrito { get; set; }
        public double totalcarrito { get; set; }
        public double totalabonado { get; set; }
        public int idCaja { get; set; }
        public int idSucursal { get; set; }
        public int fontSize { get; set; }
        public int idCorte { get; set; }
        public bool successful { get; set; }
        public int printerType { get; set; }
        public string fontName { get; set; }
        public Usuario usuario { get; set; }
        public bool esDescuento { get; set; }
        public double descuento { get; set; }
        public bool desbloqDesc { get; set; }
        public double porcentajeDesc { get; set; }
        public bool isventa { get; set; }
        public double totalDescuentoCategoria { get; set; }
        public double totalDescuentoPrecioEspecial { get; set; }
    }
}
