using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Domain.DTOs
{
    public class PersonaDTO : BaseDTO
    {
        public string Nombre { get; set; }
        public string Biografia { get; set; }
    }
}

