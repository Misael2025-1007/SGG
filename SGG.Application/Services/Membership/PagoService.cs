using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SGG.Application.Base;
using SGG.Application.Configuration;
using SGG.Application.Interfaces;
using SGG.Domain.Base;
using SGG.Domain.Entities.Membership;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Membership;
using SGG.Persistence.Interfaces;

namespace SGG.Application.Services.Membership
{
    public class PagoService : IPagoService
    {
        private readonly IPagoRepository _pagoRepository;
        private readonly IMembresiaRepository _membresiaRepository;
        private readonly GymRulesOptions _rules;
        private readonly ILogger<PagoService> _logger;

        public PagoService(IPagoRepository pagoRepository,
                           IMembresiaRepository membresiaRepository,
                           IOptions<GymRulesOptions> options,
                           ILogger<PagoService> logger)
        {
            _pagoRepository = pagoRepository;
            _membresiaRepository = membresiaRepository;
            _rules = options.Value;
            _logger = logger;
        }

        public Task<OperationResult> GetAllAsync()
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _pagoRepository.GetAllAsync(p => p.Status);
                if (!result.Success) return result;

                List<Pago> pagos = (List<Pago>)result.Data!;
                return ServiceResult.Ok(data: pagos.OrderByDescending(p => p.FechaPago).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo los pagos.");
        }

        public Task<OperationResult> GetByIdAsync(int id)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Pago? pago = await _pagoRepository.GetEntityByIdAsync(id);
                if (pago == null || !pago.Status) return ServiceResult.Fail("El pago no existe.");

                return ServiceResult.Ok(data: ToDto(pago));
            }, _logger, "Ocurrió un error obteniendo el pago.");
        }

        public Task<OperationResult> GetByMembresiaAsync(int membresiaId)
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _pagoRepository.GetAllAsync(p => p.MembresiaId == membresiaId && p.Status);
                if (!result.Success) return result;

                List<Pago> pagos = (List<Pago>)result.Data!;
                return ServiceResult.Ok(data: pagos.OrderByDescending(p => p.FechaPago).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo los pagos de la membresía.");
        }

        public Task<OperationResult> SaveAsync(SavePagoDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = Validate(dto, out string? metodo, out DateTime fecha);
                if (error != null) return ServiceResult.Fail(error);

                Membresia? membresia = await _membresiaRepository.GetEntityByIdAsync(dto.MembresiaId);
                if (membresia == null || !membresia.Status) return ServiceResult.Fail("La membresía indicada no existe.");

                Pago pago = new Pago
                {
                    MembresiaId = dto.MembresiaId,
                    Monto = dto.Monto,
                    FechaPago = fecha,
                    MetodoPago = metodo
                };

                OperationResult result = await _pagoRepository.SaveEntityAsync(pago);
                if (!result.Success) return result;

                return ServiceResult.Ok("Pago registrado correctamente.", ToDto(pago));
            }, _logger, "Ocurrió un error registrando el pago.");
        }

        public Task<OperationResult> UpdateAsync(UpdatePagoDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = Validate(dto, out string? metodo, out DateTime fecha);
                if (error != null) return ServiceResult.Fail(error);

                Pago? pago = await _pagoRepository.GetEntityByIdAsync(dto.Id);
                if (pago == null || !pago.Status) return ServiceResult.Fail("El pago no existe.");

                if (dto.MembresiaId != pago.MembresiaId)
                    return ServiceResult.Fail("No se puede cambiar la membresía de un pago.");

                pago.Monto = dto.Monto;
                pago.FechaPago = fecha;
                pago.MetodoPago = metodo;
                pago.UserMod = dto.UserId;

                OperationResult result = await _pagoRepository.UpdateEntityAsync(pago);
                if (!result.Success) return result;

                return ServiceResult.Ok("Pago actualizado correctamente.", ToDto(pago));
            }, _logger, "Ocurrió un error actualizando el pago.");
        }

        public Task<OperationResult> RemoveAsync(RemoveDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Pago? pago = await _pagoRepository.GetEntityByIdAsync(dto.Id);
                if (pago == null || !pago.Status) return ServiceResult.Fail("El pago no existe.");

                pago.Status = false;
                pago.UserMod = dto.UserId;

                OperationResult result = await _pagoRepository.UpdateEntityAsync(pago);
                if (!result.Success) return result;

                return ServiceResult.Ok("Pago eliminado correctamente.");
            }, _logger, "Ocurrió un error eliminando el pago.");
        }

        private string? Validate(SavePagoDto? dto, out string? metodo, out DateTime fecha)
        {
            metodo = null;
            fecha = DateTime.Now;
            if (dto == null) return "Los datos del pago son requeridos.";
            if (dto.Monto <= 0) return "El monto debe ser mayor que cero.";
            if (dto.Monto > 99999999.99m) return "El monto excede el máximo permitido.";

            fecha = dto.FechaPago ?? DateTime.Now;
            if (fecha > DateTime.Now.AddMinutes(5)) return "La fecha del pago no puede ser futura.";

            if (!string.IsNullOrWhiteSpace(dto.MetodoPago))
            {
                string solicitado = dto.MetodoPago.Trim();
                metodo = _rules.MetodosPagoPermitidos
                    .FirstOrDefault(m => string.Equals(m, solicitado, StringComparison.OrdinalIgnoreCase));
                if (metodo == null)
                    return $"El método de pago no es válido. Permitidos: {string.Join(", ", _rules.MetodosPagoPermitidos.Distinct())}.";
            }

            return null;
        }

        private static PagoDto ToDto(Pago p) => new PagoDto
        {
            Id = p.Id,
            MembresiaId = p.MembresiaId,
            Monto = p.Monto,
            FechaPago = p.FechaPago,
            MetodoPago = p.MetodoPago,
            Status = p.Status,
            CreationDate = p.CreationDate,
            ModifyDate = p.ModifyDate
        };
    }
}
