using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Mensaje
    {
        public int IdMensaje { get; set; }
        public int IdEmisor { get; set; }
        public int IdReceptor { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaEnvio { get; set; } = DateTime.Now;
    }
}
