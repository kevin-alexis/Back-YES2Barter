﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class MensajeDTO : BaseDTO
    {
        public string IdUsuarioEmisor { get; set; }
        public string IdUsuarioReceptor { get; set; }
        public string Contenido { get; set; }
        public DateOnly FechaEnvio { get; set; }
    }
}
