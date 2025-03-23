using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.EditPersona
{
    public class EditPersonaVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Biografia { get; set; }
        public IFormFile? RutaFotoPerfil { get; set; }
    }

}
