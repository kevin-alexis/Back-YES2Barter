using Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Domain.Entities
{
    [Table("Tbl_Mensajes")]
    public class Mensaje : BaseEntity
    {
        [ForeignKey(nameof(Chat))]
        public int IdChat { get; set; }
        public virtual Chat Chat { get; set; }

        [ForeignKey(nameof(UsuarioEmisor))]
        public string IdUsuarioEmisor { get; set; }
        public virtual ApplicationUser UsuarioEmisor { get; set; }

        [ForeignKey(nameof(UsuarioReceptor))]
        public string IdUsuarioReceptor { get; set; }
        public virtual ApplicationUser UsuarioReceptor { get; set; } 
        public string Contenido { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
