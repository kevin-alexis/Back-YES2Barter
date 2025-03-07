using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Domain.Entities
{
    [Table("Tbl_PropuestasIntercambios")]
    public class PropuestaIntercambio : BaseEntity
    {

        [ForeignKey(nameof(UsuarioOfertante))]
        public string IdUsuarioOfertante { get; set; }
        public virtual ApplicationUser UsuarioOfertante { get; set; }

        [ForeignKey(nameof(UsuarioReceptor))]
        public string IdUsuarioReceptor { get; set; }
        public virtual ApplicationUser UsuarioReceptor { get; set; }

        [ForeignKey(nameof(ObjetoOfertado))]
        public int IdObjetoOfertado { get; set; }
        public virtual Objeto ObjetoOfertado { get; set; }

        [ForeignKey(nameof(ObjetoSolicitado))]
        public int IdObjetoSolicitado { get; set; }
        public virtual Objeto ObjetoSolicitado { get; set; }

        //public int estado {  get; set; }

        public DateOnly FechaPropuesta { get; set; }


    }
}
