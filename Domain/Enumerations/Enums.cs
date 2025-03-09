using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enumerations
{
    public class Enums
    {
        public enum Roles
        {
            ADMINISTRADOR, INTERCAMBIADOR
        }

        public enum EstatusObjeto
        {
            DISPONIBLE,     // El objeto está disponible para intercambio
            NO_DISPONIBLE   // El objeto no está disponible para intercambio (Fue concretada la propuesta)
        }

        public enum EstatusPropuestaIntercambio
        {
            ENVIADA,        // La propuesta ha sido enviada, pero no ha sido aceptada ni rechazada
            ACEPTADA,       // La propuesta fue aceptada
            RECHAZADA,      // La propuesta fue rechazada
            CONCRETADA,     // La propuesta fue concretada (finalizaron el chat de forma satisfactoria)
            NO_CONCRETADA   // La propuesta no fue concretada (finalizaron el chat de forma no satisfactoria)
        }
    }
}
