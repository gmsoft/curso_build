using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTool_NASA_CNE.Entities
{
    public class OPO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Codigo { get; set; }
        public IList<Linea> Lineas { get; set; }
        public int CantidadTarjetas { get; set; }
        public int CantidadLineas { get; set; }
        public bool Eliminado { get; set; }
        public string Estado { get; set; }
        public string Bsi { get; set; }
        public string Letra { get; set; }
        public string Numero { get; set; }

    }
}
