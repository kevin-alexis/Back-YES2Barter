using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Domain.ViewModels.CreatePropuestaIntercambio
{
    public class CreatePropuestaIntercambioVM
    {
        public string IdUsuarioOfertante { get; set; }
        public string IdUsuarioReceptor { get; set; }
        public int IdObjetoOfertado { get; set; }
        public int IdObjetoSolicitado { get; set; }
        public DateTime FechaPropuesta { get; set; }
        public EstatusPropuestaIntercambio Estado { get; set; }


    }
}
