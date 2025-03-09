using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.EditChat
{
    public class EditChatVM
    {
        public int Id { get; set; }
        public string IdUsuario1 { get; set; }
        public string IdUsuario2 { get; set; }
        public int IdPropuestaIntercambio { get; set; }

    }
}
