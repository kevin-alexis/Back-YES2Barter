using Domain.Entities;
using Domain.ViewModels.GetPropuestasIntercambios;

namespace Domain.ViewModels.GetChats
{
    public class GetChatsVM
    {
        public int Id { get; set; }
        public string IdUsuario1 { get; set; }
        public Persona PersonaEmisor { get; set; }
        public string IdUsuario2 { get; set; }
        public Persona PersonaReceptor { get; set; }
        public int IdPropuestaIntercambio { get; set; }
        public PropuestasIntercambiosVM PropuestaIntercambio { get; set; }

    }
}
