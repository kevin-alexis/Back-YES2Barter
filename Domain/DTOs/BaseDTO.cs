using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class BaseDTO
    {
        public int Id { get; set; }
        public bool EsBorrado { get; set; } = false;
    }
}
