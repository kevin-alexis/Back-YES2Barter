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
    [Table("Tbl_Objetos")]
    public class Objeto : BaseEntity
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateOnly FechaPublicacion { get; set; }
        public string RutaImagen { get; set; }

        [ForeignKey(nameof(Categoria))]
        public int IdCategoria { get; set; }
        public virtual Categoria Categoria { get; set; }

        //public int estado {  get; set; }

    }
}
