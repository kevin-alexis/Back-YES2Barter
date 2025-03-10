using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.CreateMensaje
{
    public class CreateMensajeVM : BaseDTO
    {
        public int IdChat { get; set; }
        public string IdUsuarioEmisor { get; set; }
        public string IdUsuarioReceptor { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}