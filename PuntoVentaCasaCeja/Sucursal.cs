using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuntoVentaCasaCeja
{
    public class Sucursal
    {
        public int id { get; set; }
        public string puerta_enlace1 { get; set; }
        public string puerta_enlace2 { get; set; }
        public string puerta_enlace3 { get; set; }
        public string puerta_enlace4 { get; set; }
        public string razon_social { get; set; }
        public string direccion { get; set; }
        public string correo { get; set; }
        public int activo { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}
