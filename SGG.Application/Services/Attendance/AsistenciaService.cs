using Microsoft.Extensions.Logging;
using SGG.Application.Base;
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
    public class AsistenciaService : IAsistenciaService
    {
        private readonly IAsistenciaRepository _asistenciaRepository;
        private readonly ISocioRepository _socioRepository;
        private readonly IClaseRepository _claseRepository;
        private readonly IMembresiaRepository _membresiaRepository;
        private readonly ILogger<AsistenciaService> _logger;

        public AsistenciaService(IAsistenciaRepository asistenciaRepository,
                                 ISocioRepository socioRepository,
                                 IClaseRepository claseRepository,
                                 IMembresiaRepository membresiaRepository,
                                 ILogger<AsistenciaService> logger)
        {
            _asistenciaRepository = asistenciaRepository;
            _socioRepository = socioRepository;
            _claseRepository = claseRepository;
            _membresiaRepository = membresiaRepository;
            _logger = logger;
        }

        public Task<OperationResult> GetAllAsync()
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _asistenciaRepository.GetAllAsync(a => a.Status);
                if (!result.Success) return result;

                List<Asistencia> asistencias = (List<Asistencia>)result.Data!;
                return ServiceResult.Ok(data: asistencias.OrderByDescending(a => a.FechaEntrada).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo las asistencias.");
        }

        public Task<OperationResult> GetByIdAsync(int id)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Asistencia? asistencia = await _asistenciaRepository.GetEntityByIdAsync(id);
                if (asistencia == null || !asistencia.Status) return ServiceResult.Fail("La asistencia no existe.");

                return ServiceResult.Ok(data: ToDto(asistencia));
            }, _logger, "Ocurrió un error obteniendo la asistencia.");
        }

        public Task<OperationResult> GetBySocioAsync(int socioId)
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _asistenciaRepository.GetAllAsync(a => a.SocioId == socioId && a.Status);
                if (!result.Success) return result;

                List<Asistencia> asistencias = (List<Asistencia>)result.Data!;
                return ServiceResult.Ok(data: asistencias.OrderByDescending(a => a.FechaEntrada).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo las asistencias del socio.");
        }

        public Task<OperationResult> SaveAsync(SaveAsistenciaDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                (string? error, DateTime fecha) = await ValidateAsync(dto, 0);
                if (error != null) return ServiceResult.Fail(error);

                Asistencia asistencia = new Asistencia
                {
                    SocioId = dto.SocioId,
                    ClaseId = dto.ClaseId,
                    FechaEntrada = fecha
                };

                OperationResult result = await _asistenciaRepository.SaveEntityAsync(asistencia);
                if (!result.Success) return result;

                return ServiceResult.Ok("Asistencia registrada correctamente.", ToDto(asistencia));
            }, _logger, "Ocurrió un error registrando la asistencia.");
        }

        public Task<OperationResult> UpdateAsync(UpdateAsistenciaDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                if (dto == null) return ServiceResult.Fail("Los datos de la asistencia son requeridos.");

                Asistencia? asistencia = await _asistenciaRepository.GetEntityByIdAsync(dto.Id);
                if (asistencia == null || !asistencia.Status) return ServiceResult.Fail("La asistencia no existe.");

                if (dto.SocioId != asistencia.SocioId)
                    return ServiceResult.Fail("No se puede cambiar el socio de una asistencia.");

                (string? error, DateTime fecha) = await ValidateAsync(dto, dto.Id);
                if (error != null) return ServiceResult.Fail(error);

                asistencia.ClaseId = dto.ClaseId;
                asistencia.FechaEntrada = fecha;
                asistencia.UserMod = dto.UserId;

                OperationResult result = await _asistenciaRepository.UpdateEntityAsync(asistencia);
                if (!result.Success) return result;

                return ServiceResult.Ok("Asistencia actualizada correctamente.", ToDto(asistencia));
            }, _logger, "Ocurrió un error actualizando la asistencia.");
        }

        public Task<OperationResult> RemoveAsync(RemoveDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Asistencia? asistencia = await _asistenciaRepository.GetEntityByIdAsync(dto.Id);
                if (asistencia == null || !asistencia.Status) return ServiceResult.Fail("La asistencia no existe.");

                asistencia.Status = false;
                asistencia.UserMod = dto.UserId;

                OperationResult result = await _asistenciaRepository.UpdateEntityAsync(asistencia);
                if (!result.Success) return result;

                return ServiceResult.Ok("Asistencia eliminada correctamente.");
            }, _logger, "Ocurrió un error eliminando la asistencia.");
        }

        private async Task<(string? Error, DateTime Fecha)> ValidateAsync(SaveAsistenciaDto? dto, int id)
        {
            DateTime fecha = dto?.FechaEntrada ?? DateTime.Now;
            if (dto == null) return ("Los datos de la asistencia son requeridos.", fecha);
            if (fecha > DateTime.Now.AddMinutes(5)) return ("La fecha de entrada no puede ser futura.", fecha);

            Socio? socio = await _socioRepository.GetEntityByIdAsync(dto.SocioId);
            if (socio == null || !socio.Status) return ("El socio indicado no existe.", fecha);

            if (dto.ClaseId.HasValue)
            {
                Clase? clase = await _claseRepository.GetEntityByIdAsync(dto.ClaseId.Value);
                if (clase == null || !clase.Status) return ("La clase indicada no existe.", fecha);
            }

            if (!await _membresiaRepository.ExistsAsync(MembresiaRules.VigenteEn(dto.SocioId, fecha)))
                return ("El socio no tiene una membresía vigente en esa fecha.", fecha);

            int socioId = dto.SocioId;
            int? claseId = dto.ClaseId;
            DateTime day = fecha.Date;
            DateTime nextDay = day.AddDays(1);
            bool duplicada = await _asistenciaRepository.ExistsAsync(a => a.SocioId == socioId
                                                                           && a.Status
                                                                           && a.Id != id
                                                                           && a.ClaseId == claseId
                                                                           && a.FechaEntrada >= day
                                                                           && a.FechaEntrada < nextDay);
            if (duplicada) return ("El socio ya registró esta asistencia el mismo día.", fecha);

            return (null, fecha);
        }

        private static AsistenciaDto ToDto(Asistencia a) => new AsistenciaDto
        {
            Id = a.Id,
            SocioId = a.SocioId,
            ClaseId = a.ClaseId,
            FechaEntrada = a.FechaEntrada,
            Status = a.Status,
            CreationDate = a.CreationDate,
            ModifyDate = a.ModifyDate
        };
    }
}
