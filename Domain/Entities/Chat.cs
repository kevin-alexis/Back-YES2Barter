using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Tbl_Chats")]
    public class Chat : BaseEntity
    {
        [ForeignKey(nameof(Usuario1))]
        public string IdUsuario1 { get; set; }
        public virtual ApplicationUser Usuario1 { get; set; }

        [ForeignKey(nameof(Usuario2))]
        public string IdUsuario2 { get; set; }
        public virtual ApplicationUser Usuario2 { get; set; }

    }
}
