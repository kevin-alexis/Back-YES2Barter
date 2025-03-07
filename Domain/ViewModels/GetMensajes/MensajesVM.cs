using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.GetMensajes
{
    public class MensajesVM
    {
        public int Id { get; set; }
        public string IdUsuarioEmisor { get; set; }
        public string IdUsuarioReceptor { get; set; }
        public string Contenido { get; set; }
        public DateOnly FechaEnvio { get; set; }
    }
}
