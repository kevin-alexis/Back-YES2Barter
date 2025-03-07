using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.CreatePropuestaIntercambio
{
    public class CreatePropuestaIntercambioVM
    {
        public string IdUsuarioOfertante { get; set; }
        public string IdUsuarioReceptor { get; set; }
        public int IdObjetoOfertado { get; set; }
        public int IdObjetoSolicitado { get; set; }
        public DateOnly FechaPropuesta { get; set; }

    }
}
