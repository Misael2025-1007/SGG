using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SGG.Application.Base;
using SGG.Application.Configuration;
using SGG.Application.Helpers;
using SGG.Application.Interfaces;
using SGG.Domain.Base;
using SGG.Domain.Entities.Attendance;
using SGG.Domain.Entities.Configuration;
using SGG.Domain.Entities.Membership;
using SGG.Model.Dtos.Attendance;
using SGG.Model.Dtos.Base;
using SGG.Persistence.Interfaces;

namespace SGG.Application.Services.Attendance
{
    public class ReservaClaseService : IReservaClaseService
    {
        private readonly IReservaClaseRepository _reservaRepository;
        private readonly ISocioRepository _socioRepository;
        private readonly IClaseRepository _claseRepository;
        private readonly IMembresiaRepository _membresiaRepository;
        private readonly GymRulesOptions _rules;
        private readonly ILogger<ReservaClaseService> _logger;

        public ReservaClaseService(IReservaClaseRepository reservaRepository,
                                   ISocioRepository socioRepository,
                                   IClaseRepository claseRepository,
                                   IMembresiaRepository membresiaRepository,
                                   IOptions<GymRulesOptions> options,
                                   ILogger<ReservaClaseService> logger)
        {
            _reservaRepository = reservaRepository;
            _socioRepository = socioRepository;
            _claseRepository = claseRepository;
            _membresiaRepository = membresiaRepository;
            _rules = options.Value;
            _logger = logger;
        }

        public Task<OperationResult> GetAllAsync()
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _reservaRepository.GetAllAsync(r => r.Status);
                if (!result.Success) return result;

                List<ReservaClase> reservas = (List<ReservaClase>)result.Data!;
                return ServiceResult.Ok(data: reservas.OrderByDescending(r => r.FechaClase).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo las reservas.");
        }

        public Task<OperationResult> GetByIdAsync(int id)
        {
            return ServiceResult.TryAsync(async () =>
            {
                ReservaClase? reserva = await _reservaRepository.GetEntityByIdAsync(id);
                if (reserva == null || !reserva.Status) return ServiceResult.Fail("La reserva no existe.");

                return ServiceResult.Ok(data: ToDto(reserva));
            }, _logger, "Ocurrió un error obteniendo la reserva.");
        }

        public Task<OperationResult> GetBySocioAsync(int socioId)
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _reservaRepository.GetAllAsync(r => r.SocioId == socioId && r.Status);
                if (!result.Success) return result;

                List<ReservaClase> reservas = (List<ReservaClase>)result.Data!;
                return ServiceResult.Ok(data: reservas.OrderByDescending(r => r.FechaClase).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo las reservas del socio.");
        }

        public Task<OperationResult> SaveAsync(SaveReservaClaseDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                (string? error, DateTime fechaClase) = await ValidateAsync(dto, 0);
                if (error != null) return ServiceResult.Fail(error);

                ReservaClase reserva = new ReservaClase
                {
                    SocioId = dto.SocioId,
                    ClaseId = dto.ClaseId,
                    FechaReserva = DateTime.Now,
                    FechaClase = fechaClase
                };

                OperationResult result = await _reservaRepository.SaveEntityAsync(reserva);
                if (!result.Success) return result;

                return ServiceResult.Ok("Reserva realizada correctamente.", ToDto(reserva));
            }, _logger, "Ocurrió un error guardando la reserva.");
        }

        public Task<OperationResult> UpdateAsync(UpdateReservaClaseDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                if (dto == null) return ServiceResult.Fail("Los datos de la reserva son requeridos.");

                ReservaClase? reserva = await _reservaRepository.GetEntityByIdAsync(dto.Id);
                if (reserva == null || !reserva.Status) return ServiceResult.Fail("La reserva no existe.");

                if (dto.SocioId != reserva.SocioId || dto.ClaseId != reserva.ClaseId)
                    return ServiceResult.Fail("Solo se puede cambiar la fecha de la clase; para otra clase o socio cree una nueva reserva.");

                (string? error, DateTime fechaClase) = await ValidateAsync(dto, dto.Id);
                if (error != null) return ServiceResult.Fail(error);

                reserva.FechaClase = fechaClase;
                reserva.UserMod = dto.UserId;

                OperationResult result = await _reservaRepository.UpdateEntityAsync(reserva);
                if (!result.Success) return result;

                return ServiceResult.Ok("Reserva actualizada correctamente.", ToDto(reserva));
            }, _logger, "Ocurrió un error actualizando la reserva.");
        }

        public Task<OperationResult> RemoveAsync(RemoveDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                ReservaClase? reserva = await _reservaRepository.GetEntityByIdAsync(dto.Id);
                if (reserva == null || !reserva.Status) return ServiceResult.Fail("La reserva no existe.");

                if (reserva.FechaClase <= DateTime.Now)
                    return ServiceResult.Fail("No se puede cancelar una reserva de una clase que ya inició.");

                reserva.Status = false;
                reserva.UserMod = dto.UserId;

                OperationResult result = await _reservaRepository.UpdateEntityAsync(reserva);
                if (!result.Success) return result;

                return ServiceResult.Ok("Reserva cancelada correctamente.");
            }, _logger, "Ocurrió un error cancelando la reserva.");
        }

        private async Task<(string? Error, DateTime FechaClase)> ValidateAsync(SaveReservaClaseDto? dto, int id)
        {
            DateTime fechaClase = DateTime.Now;
            if (dto == null) return ("Los datos de la reserva son requeridos.", fechaClase);

            Socio? socio = await _socioRepository.GetEntityByIdAsync(dto.SocioId);
            if (socio == null || !socio.Status) return ("El socio indicado no existe.", fechaClase);

            Clase? clase = await _claseRepository.GetEntityByIdAsync(dto.ClaseId);
            if (clase == null || !clase.Status) return ("La clase indicada no existe.", fechaClase);

            DateTime day = dto.FechaClase.Date;
            if (!WeekDays.Matches(clase.DiaSemana, day))
                return ($"La clase se imparte los {clase.DiaSemana}; la fecha indicada no coincide.", fechaClase);

            fechaClase = day + clase.HoraInicio;
            DateTime now = DateTime.Now;
            if (fechaClase <= now) return ("La clase indicada ya inició o ya pasó.", fechaClase);

            int socioId = dto.SocioId;
            int claseId = dto.ClaseId;
            DateTime fecha = fechaClase;

            if (!await _membresiaRepository.ExistsAsync(MembresiaRules.VigenteEn(socioId, day)))
                return ("El socio no tiene una membresía vigente en la fecha de la clase.", fechaClase);

            if (await _reservaRepository.ExistsAsync(r => r.SocioId == socioId && r.ClaseId == claseId
                                                          && r.Status && r.Id != id && r.FechaClase == fecha))
                return ("El socio ya tiene una reserva para esa clase en esa fecha.", fechaClase);

            OperationResult ocupados = await _reservaRepository.GetAllAsync(r => r.ClaseId == claseId
                                                                               && r.Status && r.Id != id && r.FechaClase == fecha);
            if (!ocupados.Success) return (ocupados.Message, fechaClase);
            if (((List<ReservaClase>)ocupados.Data!).Count >= clase.Capacidad)
                return ("La clase ya alcanzó su capacidad máxima.", fechaClase);

            OperationResult activas = await _reservaRepository.GetAllAsync(r => r.SocioId == socioId
                                                                              && r.Status && r.Id != id && r.FechaClase >= now);
            if (!activas.Success) return (activas.Message, fechaClase);
            if (((List<ReservaClase>)activas.Data!).Count >= _rules.MaxReservasActivasPorSocio)
                return ($"El socio alcanzó el máximo de {_rules.MaxReservasActivasPorSocio} reservas activas.", fechaClase);

            return (null, fechaClase);
        }

        private static ReservaClaseDto ToDto(ReservaClase r) => new ReservaClaseDto
        {
            Id = r.Id,
            SocioId = r.SocioId,
            ClaseId = r.ClaseId,
            FechaReserva = r.FechaReserva,
            FechaClase = r.FechaClase,
            Status = r.Status,
            CreationDate = r.CreationDate,
            ModifyDate = r.ModifyDate
        };
    }
}
