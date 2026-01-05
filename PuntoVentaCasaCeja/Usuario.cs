using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuntoVentaCasaCeja
{
    public class Usuario
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string correo { get; set; }
        public int confirmacion { get; set; }
        public string telefono { get; set; }
        public string imagen { get; set; }
        public string usuario { get; set; }
        public string clave { get; set; }
        public int es_raiz { get; set; }
        public int activo { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}
