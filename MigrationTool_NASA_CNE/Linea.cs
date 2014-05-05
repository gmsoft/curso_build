using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTool_NASA_CNE.Entities
{
    public class Linea
    {
        public int Id { get; set; }
        public int NroLinea { get; set; }
        public int NroPaso { get; set; }
        public char? LetraPaso { get; set; }
        public string Comentario { get; set; }
        public string Dispositivo { get; set; }
        public string Operacion { get; set; }
        public bool DebeRetirarTarjeta { get; set; }
        public bool DebeColocarTarjeta { get; set; }
        public int IdOpo { get; set; }
    }
}
