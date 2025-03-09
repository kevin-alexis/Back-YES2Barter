using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class ChatDTO : BaseDTO
    {
        public string IdUsuario1 { get; set; }
        public string IdUsuario2 { get; set; }
        public int IdPropuestaIntercambio { get; set; }
    }
}
