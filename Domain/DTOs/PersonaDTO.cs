using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;
using Domain.Entities;

namespace Domain.DTOs
{
    public class PersonaDTO : BaseDTO
    {
        public string Nombre { get; set; }
        public string Biografia { get; set; }
        public string RutaFotoPerfil { get; set; }
        public string IdUsuario { get; set; }

    }
}

