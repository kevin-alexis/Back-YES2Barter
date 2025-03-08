using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Tbl_Categorias")]
    public class Categoria : BaseEntity
    {
        public string Nombre { get; set; }

        public virtual ICollection<Objeto> Objetos { get; set; } = new List<Objeto>();
    }
}
