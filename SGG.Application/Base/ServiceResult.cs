using Microsoft.Extensions.Logging;
using SGG.Domain.Base;

namespace SGG.Application.Base
{
    /// <summary>
    /// Fábrica de OperationResult para que los servicios respondan de forma uniforme.
    /// </summary>
    public static class ServiceResult
    {
        public static OperationResult Ok(string? message = null, object? data = null)
        {
            return new OperationResult { Success = true, Message = message, Data = data };
        }

        public static OperationResult Fail(string message)
        {
            return new OperationResult { Success = false, Message = message };
        }

        /// <summary>
        /// Ejecuta una operación del servicio y, si lanza una excepción, la registra en el log y devuelve un error controlado.
        /// </summary>
        public static async Task<OperationResult> TryAsync(Func<Task<OperationResult>> action, ILogger logger, string errorMessage)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, errorMessage);
                return Fail(errorMessage);
            }
        }
    }
}
