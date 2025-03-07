using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class ObjetoDTO : BaseDTO
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateOnly FechaPublicacion { get; set; }
        public string RutaImagen { get; set; }
        public int IdCategoria { get; set; }
    }

}
