using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuntoVentaCasaCeja
{
    public class ProductoVenta
    {
        public int id { get; set; }
        public string codigo { get; set; }
        public string nombre { get; set; }
        public int cantidad { get; set; }
        public double precio_venta { get; set; }

        // CAMPOS PARA PRECIO ESPECIAL (ya existentes)
        public double precio_original { get; set; }
        public bool es_precio_especial { get; set; }
        public double descuento_unitario { get; set; }

        // NUEVOS CAMPOS PARA DESCUENTO POR CATEGORÍA
        public bool es_descuento_categoria { get; set; }
        public double descuento_categoria_unitario { get; set; }
        public double porcentaje_descuento_categoria { get; set; } // Para mostrar en tickets
        public bool tuvo_descuento_categoria_original { get; set; } = false;
        public double descuento_categoria_original { get; set; } = 0;
        public double porcentaje_categoria_original { get; set; } = 0;
    }
}
