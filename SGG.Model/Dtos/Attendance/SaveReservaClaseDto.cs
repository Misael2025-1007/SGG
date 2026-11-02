using SGG.Model.Dtos.Base;

namespace SGG.Model.Dtos.Attendance
{
    public class SaveReservaClaseDto : DtoBase
    {
        public int SocioId { get; set; }
        public int ClaseId { get; set; }
        public DateTime FechaClase { get; set; }
    }
}
