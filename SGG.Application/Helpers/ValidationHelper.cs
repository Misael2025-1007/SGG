using System.Text.RegularExpressions;

namespace SGG.Application.Helpers
{
    public static class ValidationHelper
    {
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex PhoneRegex = new(@"^[0-9+()\-\s]{7,20}$", RegexOptions.Compiled);

        public static bool IsValidEmail(string email) => EmailRegex.IsMatch(email);

        public static bool IsValidPhone(string phone) => PhoneRegex.IsMatch(phone);

        /// <summary>Valida una cédula dominicana (11 dígitos) y la devuelve con formato 000-0000000-0.</summary>
        public static bool TryNormalizeCedula(string cedula, out string normalized)
        {
            string digits = new string(cedula.Where(char.IsDigit).ToArray());
            bool valid = digits.Length == 11 && cedula.All(c => char.IsDigit(c) || c == '-');
            normalized = valid ? $"{digits[..3]}-{digits[3..10]}-{digits[10]}" : cedula;
            return valid;
        }
    }
}
