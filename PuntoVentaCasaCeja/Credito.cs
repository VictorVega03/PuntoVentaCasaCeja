using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuntoVentaCasaCeja
{
    public class Credito
    {
        public int id { get; set; }
        public string productos { get; set; }
        public double total { get; set; }
        public double total_pagado { get; set; }
        public string fecha_de_credito { get; set; }
        public string folio { get; set; }
        public int estado { get; set; }
        public int cliente_creditos_id { get; set; }
        public int id_cajero_registro { get; set; }
        public int sucursal_id { get; set; }
        public string observaciones { get; set; }
        public List<AbonoCredito> abonos { get; set; }
        public int temporal { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}
