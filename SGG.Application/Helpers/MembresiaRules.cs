using SGG.Domain.Entities.Membership;
using System.Linq.Expressions;

namespace SGG.Application.Helpers
{
    public static class MembresiaRules
    {
        /// <summary>Membresía activa del socio que cubre la fecha indicada (se compara solo la parte fecha).</summary>
        public static Expression<Func<Membresia, bool>> VigenteEn(int socioId, DateTime fecha)
        {
            DateTime day = fecha.Date;
            return m => m.SocioId == socioId && m.Status && m.FechaInicio <= day && m.FechaVencimiento >= day;
        }
    }
}
