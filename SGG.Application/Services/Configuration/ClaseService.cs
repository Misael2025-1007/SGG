using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SGG.Application.Base;
using SGG.Application.Configuration;
using SGG.Application.Helpers;
using SGG.Application.Interfaces;
using SGG.Domain.Base;
using SGG.Domain.Entities.Configuration;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Configuration;
using SGG.Persistence.Interfaces;

namespace SGG.Application.Services.Configuration
{
    public class ClaseService : IClaseService
    {
        private readonly IClaseRepository _claseRepository;
        private readonly IEntrenadorRepository _entrenadorRepository;
        private readonly IReservaClaseRepository _reservaClaseRepository;
        private readonly GymRulesOptions _rules;
        private readonly ILogger<ClaseService> _logger;

        public ClaseService(IClaseRepository claseRepository,
                            IEntrenadorRepository entrenadorRepository,
                            IReservaClaseRepository reservaClaseRepository,
                            IOptions<GymRulesOptions> options,
                            ILogger<ClaseService> logger)
        {
            _claseRepository = claseRepository;
            _entrenadorRepository = entrenadorRepository;
            _reservaClaseRepository = reservaClaseRepository;
            _rules = options.Value;
            _logger = logger;
        }

        public Task<OperationResult> GetAllAsync()
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _claseRepository.GetAllAsync(c => c.Status);
                if (!result.Success) return result;

                List<Clase> clases = (List<Clase>)result.Data!;
                return ServiceResult.Ok(data: clases.OrderBy(c => c.Nombre).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo las clases.");
        }

        public Task<OperationResult> GetByIdAsync(int id)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Clase? clase = await _claseRepository.GetEntityByIdAsync(id);
                if (clase == null || !clase.Status)
                    return ServiceResult.Fail("La clase no existe.");

                return ServiceResult.Ok(data: ToDto(clase));
            }, _logger, "Ocurrió un error obteniendo la clase.");
        }

        public Task<OperationResult> GetByEntrenadorAsync(int entrenadorId)
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _claseRepository.GetAllAsync(c => c.EntrenadorId == entrenadorId && c.Status);
                if (!result.Success) return result;

                List<Clase> clases = (List<Clase>)result.Data!;
                return ServiceResult.Ok(data: clases.OrderBy(c => c.Nombre).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo las clases del entrenador.");
        }

        public Task<OperationResult> SaveAsync(SaveClaseDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = await ValidateAsync(dto, null);
                if (error != null) return ServiceResult.Fail(error);

                Clase clase = new Clase
                {
                    Nombre = dto.Nombre.Trim(),
                    EntrenadorId = dto.EntrenadorId,
                    Capacidad = dto.Capacidad,
                    DiaSemana = WeekDays.ToCanonical(dto.DiaSemana)!,
                    HoraInicio = dto.HoraInicio,
                    HoraFin = dto.HoraFin
                };

                OperationResult result = await _claseRepository.SaveEntityAsync(clase);
                if (!result.Success) return result;

                return ServiceResult.Ok("Clase guardada correctamente.", ToDto(clase));
            }, _logger, "Ocurrió un error guardando la clase.");
        }

        public Task<OperationResult> UpdateAsync(UpdateClaseDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = await ValidateAsync(dto, dto?.Id);
                if (error != null) return ServiceResult.Fail(error);

                Clase? clase = await _claseRepository.GetEntityByIdAsync(dto!.Id);
                if (clase == null || !clase.Status)
                    return ServiceResult.Fail("La clase no existe.");

                clase.Nombre = dto.Nombre.Trim();
                clase.EntrenadorId = dto.EntrenadorId;
                clase.Capacidad = dto.Capacidad;
                clase.DiaSemana = WeekDays.ToCanonical(dto.DiaSemana)!;
                clase.HoraInicio = dto.HoraInicio;
                clase.HoraFin = dto.HoraFin;
                clase.UserMod = dto.UserId;

                OperationResult result = await _claseRepository.UpdateEntityAsync(clase);
                if (!result.Success) return result;

                return ServiceResult.Ok("Clase actualizada correctamente.", ToDto(clase));
            }, _logger, "Ocurrió un error actualizando la clase.");
        }

        public Task<OperationResult> RemoveAsync(RemoveDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Clase? clase = await _claseRepository.GetEntityByIdAsync(dto.Id);
                if (clase == null || !clase.Status)
                    return ServiceResult.Fail("La clase no existe.");

                int id = dto.Id;
                DateTime now = DateTime.Now;
                if (await _reservaClaseRepository.ExistsAsync(r => r.ClaseId == id && r.Status && r.FechaClase >= now))
                    return ServiceResult.Fail("No se puede eliminar la clase porque tiene reservas pendientes.");

                clase.Status = false;
                clase.UserMod = dto.UserId;

                OperationResult result = await _claseRepository.UpdateEntityAsync(clase);
                if (!result.Success) return result;

                return ServiceResult.Ok("Clase eliminada correctamente.");
            }, _logger, "Ocurrió un error eliminando la clase.");
        }

        private async Task<string?> ValidateAsync(SaveClaseDto? dto, int? claseId)
        {
            if (dto == null) return "Los datos de la clase son requeridos.";
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return "El nombre de la clase es requerido.";
            if (dto.Nombre.Trim().Length > 100) return "El nombre de la clase no puede exceder 100 caracteres.";

            Entrenador? entrenador = await _entrenadorRepository.GetEntityByIdAsync(dto.EntrenadorId);
            if (entrenador == null || !entrenador.Status) return "El entrenador indicado no existe.";

            if (dto.Capacidad < 1 || dto.Capacidad > _rules.CapacidadMaximaClase)
                return $"La capacidad debe estar entre 1 y {_rules.CapacidadMaximaClase}.";

            string? dia = WeekDays.ToCanonical(dto.DiaSemana);
            if (dia == null) return "El día de la semana no es válido (Lunes a Domingo).";

            if (dto.HoraInicio < TimeSpan.Zero || dto.HoraFin >= TimeSpan.FromHours(24))
                return "Las horas deben estar entre 00:00 y 23:59.";
            if (dto.HoraInicio >= dto.HoraFin)
                return "La hora de inicio debe ser menor que la hora de fin.";

            int id = claseId ?? 0;
            int entrenadorId = dto.EntrenadorId;
            TimeSpan inicio = dto.HoraInicio;
            TimeSpan fin = dto.HoraFin;
            bool solapa = await _claseRepository.ExistsAsync(c => c.Status
                                                                  && c.EntrenadorId == entrenadorId
                                                                  && c.Id != id
                                                                  && c.DiaSemana == dia
                                                                  && c.HoraInicio < fin
                                                                  && c.HoraFin > inicio);
            if (solapa) return "El entrenador ya tiene una clase en ese horario.";

            return null;
        }

        private static ClaseDto ToDto(Clase c) => new ClaseDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            EntrenadorId = c.EntrenadorId,
            Capacidad = c.Capacidad,
            DiaSemana = c.DiaSemana,
            HoraInicio = c.HoraInicio,
            HoraFin = c.HoraFin,
            Status = c.Status,
            CreationDate = c.CreationDate,
            ModifyDate = c.ModifyDate
        };
    }
}
