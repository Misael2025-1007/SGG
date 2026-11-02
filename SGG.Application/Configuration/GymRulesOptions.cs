namespace SGG.Application.Configuration
{
    /// <summary>
    /// Reglas de negocio configurables del gimnasio (se leen de la sección "GymRules" del appsettings).
    /// </summary>
    public class GymRulesOptions
    {
        public const string SectionName = "GymRules";

        public string[] MetodosPagoPermitidos { get; set; } = { "Efectivo", "Tarjeta", "Transferencia", "Cheque" };

        public int CapacidadMaximaClase { get; set; } = 100;

        public int MaxReservasActivasPorSocio { get; set; } = 5;
    }
}
