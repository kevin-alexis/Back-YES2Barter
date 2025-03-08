using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.GetLogs
{
    public class GetLogsVM
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Nivel { get; set; }
        public string Mensaje { get; set; }
        public string Excepcion { get; set; }
    }
}
