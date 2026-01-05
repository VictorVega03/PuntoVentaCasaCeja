using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuntoVentaCasaCeja
{
    public class Apartado
    {
        public int id { get; set; }
        public string productos { get; set; }
        public double total { get; set; }
        public double total_pagado { get; set; }
        public string fecha_de_apartado { get; set; }

        public string folio { get; set; }
        public string folio_corte { get; set; }
        public string fecha_entrega { get; set; }
        public int estado { get; set; }
        public int cliente_creditos_id { get; set; }
        public int id_cajero_registro { get; set; }
        public string id_cajero_entrega { get; set; }
        public int sucursal_id { get; set; }
        public string observaciones { get; set; }
        public int temporal { get; set; }
        public List<AbonoApartado> abonos { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}
