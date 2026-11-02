using Microsoft.Extensions.Logging;
using SGG.Application.Base;
using SGG.Application.Helpers;
using SGG.Application.Interfaces;
using SGG.Domain.Base;
using SGG.Domain.Entities.Configuration;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Configuration;
using SGG.Persistence.Interfaces;

namespace SGG.Application.Services.Configuration
{
    public class EntrenadorService : IEntrenadorService
    {
        private readonly IEntrenadorRepository _entrenadorRepository;
        private readonly IClaseRepository _claseRepository;
        private readonly ILogger<EntrenadorService> _logger;

        public EntrenadorService(IEntrenadorRepository entrenadorRepository,
                                 IClaseRepository claseRepository,
                                 ILogger<EntrenadorService> logger)
        {
            _entrenadorRepository = entrenadorRepository;
            _claseRepository = claseRepository;
            _logger = logger;
        }

        public Task<OperationResult> GetAllAsync()
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _entrenadorRepository.GetAllAsync(e => e.Status);
                if (!result.Success) return result;

                List<Entrenador> entrenadores = (List<Entrenador>)result.Data!;
                return ServiceResult.Ok(data: entrenadores.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo los entrenadores.");
        }

        public Task<OperationResult> GetByIdAsync(int id)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Entrenador? entrenador = await _entrenadorRepository.GetEntityByIdAsync(id);
                if (entrenador == null || !entrenador.Status)
                    return ServiceResult.Fail("El entrenador no existe.");

                return ServiceResult.Ok(data: ToDto(entrenador));
            }, _logger, "Ocurrió un error obteniendo el entrenador.");
        }

        public Task<OperationResult> SaveAsync(SaveEntrenadorDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = Validate(dto);
                if (error != null) return ServiceResult.Fail(error);

                Entrenador entrenador = new Entrenador
                {
                    Nombre = dto.Nombre.Trim(),
                    Apellido = dto.Apellido.Trim(),
                    Especialidad = dto.Especialidad?.Trim(),
                    Telefono = dto.Telefono?.Trim()
                };

                OperationResult result = await _entrenadorRepository.SaveEntityAsync(entrenador);
                if (!result.Success) return result;

                return ServiceResult.Ok("Entrenador guardado correctamente.", ToDto(entrenador));
            }, _logger, "Ocurrió un error guardando el entrenador.");
        }

        public Task<OperationResult> UpdateAsync(UpdateEntrenadorDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = Validate(dto);
                if (error != null) return ServiceResult.Fail(error);

                Entrenador? entrenador = await _entrenadorRepository.GetEntityByIdAsync(dto.Id);
                if (entrenador == null || !entrenador.Status)
                    return ServiceResult.Fail("El entrenador no existe.");

                entrenador.Nombre = dto.Nombre.Trim();
                entrenador.Apellido = dto.Apellido.Trim();
                entrenador.Especialidad = dto.Especialidad?.Trim();
                entrenador.Telefono = dto.Telefono?.Trim();
                entrenador.UserMod = dto.UserId;

                OperationResult result = await _entrenadorRepository.UpdateEntityAsync(entrenador);
                if (!result.Success) return result;

                return ServiceResult.Ok("Entrenador actualizado correctamente.", ToDto(entrenador));
            }, _logger, "Ocurrió un error actualizando el entrenador.");
        }

        public Task<OperationResult> RemoveAsync(RemoveDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Entrenador? entrenador = await _entrenadorRepository.GetEntityByIdAsync(dto.Id);
                if (entrenador == null || !entrenador.Status)
                    return ServiceResult.Fail("El entrenador no existe.");

                int id = dto.Id;
                if (await _claseRepository.ExistsAsync(c => c.EntrenadorId == id && c.Status))
                    return ServiceResult.Fail("No se puede eliminar el entrenador porque tiene clases asignadas.");

                entrenador.Status = false;
                entrenador.UserMod = dto.UserId;

                OperationResult result = await _entrenadorRepository.UpdateEntityAsync(entrenador);
                if (!result.Success) return result;

                return ServiceResult.Ok("Entrenador eliminado correctamente.");
            }, _logger, "Ocurrió un error eliminando el entrenador.");
        }

        private static string? Validate(SaveEntrenadorDto? dto)
        {
            if (dto == null) return "Los datos del entrenador son requeridos.";
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return "El nombre es requerido.";
            if (dto.Nombre.Trim().Length > 100) return "El nombre no puede exceder 100 caracteres.";
            if (string.IsNullOrWhiteSpace(dto.Apellido)) return "El apellido es requerido.";
            if (dto.Apellido.Trim().Length > 100) return "El apellido no puede exceder 100 caracteres.";
            if (dto.Especialidad != null && dto.Especialidad.Trim().Length > 100) return "La especialidad no puede exceder 100 caracteres.";
            if (!string.IsNullOrWhiteSpace(dto.Telefono) && !ValidationHelper.IsValidPhone(dto.Telefono.Trim()))
                return "El teléfono no es válido.";
            return null;
        }

        private static EntrenadorDto ToDto(Entrenador e) => new EntrenadorDto
        {
            Id = e.Id,
            Nombre = e.Nombre,
            Apellido = e.Apellido,
            Especialidad = e.Especialidad,
            Telefono = e.Telefono,
            Status = e.Status,
            CreationDate = e.CreationDate,
            ModifyDate = e.ModifyDate
        };
    }
}
