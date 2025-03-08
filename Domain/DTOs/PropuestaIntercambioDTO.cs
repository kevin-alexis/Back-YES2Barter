using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Domain.DTOs
{
    public class PropuestaIntercambioDTO : BaseDTO
    {
        public string IdUsuarioOfertante { get; set; }
        public string IdUsuarioReceptor { get; set; }
        public int IdObjetoOfertado { get; set; }
        public int IdObjetoSolicitado { get; set; }
        public DateOnly FechaPropuesta { get; set; }
        public EstatusPropuestaIntercambio Estado { get; set; }


    }
}
