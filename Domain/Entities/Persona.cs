using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Tbl_Personas")]
    public class Persona : BaseEntity
    {
        public string Nombre { get; set; }
        public string Biografia { get; set; }
        public string RutaFotoPerfil { get; set; }

        // Referencia a AspNetUsers
        [ForeignKey(nameof(Usuario))]
        public string IdUsuario { get; set; }
        public virtual ApplicationUser Usuario { get; set; }
    }
}
