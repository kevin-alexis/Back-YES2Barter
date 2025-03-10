using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.GetMensajes
{
    public class GetMensajesVM
    {
        public int Id { get; set; }
        public int IdChat { get; set; }
        public string IdUsuarioEmisor { get; set; }
        public Persona PersonaEmisor { get; set; }
        public string IdUsuarioReceptor { get; set; }
        public Persona PersonaReceptor { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
