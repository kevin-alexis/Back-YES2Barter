using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Domain.ViewModels.EditObjeto
{
    public class EditObjetoVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateOnly FechaPublicacion { get; set; }
        public IFormFile RutaImagen { get; set; }
        public int IdCategoria { get; set; }
        public EstatusObjeto Estado { get; set; }


    }
}
