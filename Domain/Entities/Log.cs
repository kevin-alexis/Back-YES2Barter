using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Tbl_Logs")]
    public class Log
    {
        [Key]
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Nivel { get; set; }
        public string Mensaje { get; set; }
        public string Excepcion { get; set; }
    }
}
