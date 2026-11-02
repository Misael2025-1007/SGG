using System.Globalization;
using System.Text;

namespace SGG.Application.Helpers
{
    public static class WeekDays
    {
        private static readonly string[] Names =
        {
            "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"
        };

        private static string Normalize(string value)
        {
            string decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>Devuelve el nombre canónico del día (ej. "miercoles" => "Miércoles") o null si no es válido.</summary>
        public static string? ToCanonical(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            string normalized = Normalize(value);
            return Names.FirstOrDefault(n => Normalize(n) == normalized);
        }

        public static string FromDate(DateTime date) => Names[(int)date.DayOfWeek];

        public static bool Matches(string? dia, DateTime date)
        {
            string? canonical = ToCanonical(dia);
            return canonical != null && canonical == FromDate(date);
        }
    }
}
