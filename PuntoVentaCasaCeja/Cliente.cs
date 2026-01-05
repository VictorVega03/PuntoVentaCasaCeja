using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuntoVentaCasaCeja
{
    public class Cliente
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string rfc { get; set; }
        public string calle { get; set; }
        public string numero_exterior { get; set; }
        public string numero_interior { get; set; }
        public string codigo_postal { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }       
        public int activo { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}
