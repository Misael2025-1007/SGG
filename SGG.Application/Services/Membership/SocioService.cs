using Microsoft.Extensions.Logging;
using SGG.Application.Base;
using SGG.Application.Helpers;
using SGG.Application.Interfaces;
using SGG.Domain.Base;
using SGG.Domain.Entities.Membership;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Membership;
using SGG.Persistence.Interfaces;

namespace SGG.Application.Services.Membership
{
    public class SocioService : ISocioService
    {
        private readonly ISocioRepository _socioRepository;
        private readonly IMembresiaRepository _membresiaRepository;
        private readonly ILogger<SocioService> _logger;

        public SocioService(ISocioRepository socioRepository,
                            IMembresiaRepository membresiaRepository,
                            ILogger<SocioService> logger)
        {
            _socioRepository = socioRepository;
            _membresiaRepository = membresiaRepository;
            _logger = logger;
        }

        public Task<OperationResult> GetAllAsync()
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _socioRepository.GetAllAsync(s => s.Status);
                if (!result.Success) return result;

                List<Socio> socios = (List<Socio>)result.Data!;
                return ServiceResult.Ok(data: socios.OrderBy(s => s.Apellido).ThenBy(s => s.Nombre).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo los socios.");
        }

        public Task<OperationResult> GetByIdAsync(int id)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Socio? socio = await _socioRepository.GetEntityByIdAsync(id);
                if (socio == null || !socio.Status)
                    return ServiceResult.Fail("El socio no existe.");

                return ServiceResult.Ok(data: ToDto(socio));
            }, _logger, "Ocurrió un error obteniendo el socio.");
        }

        public Task<OperationResult> SaveAsync(SaveSocioDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = Validate(dto, out string? cedula);
                if (error != null) return ServiceResult.Fail(error);

                string? email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
                error = await CheckUniqueAsync(cedula, email, 0);
                if (error != null) return ServiceResult.Fail(error);

                Socio socio = new Socio
                {
                    Nombre = dto.Nombre.Trim(),
                    Apellido = dto.Apellido.Trim(),
                    Cedula = cedula,
                    Telefono = dto.Telefono?.Trim(),
                    Email = email,
                    FechaNacimiento = dto.FechaNacimiento?.Date
                };

                OperationResult result = await _socioRepository.SaveEntityAsync(socio);
                if (!result.Success) return result;

                return ServiceResult.Ok("Socio guardado correctamente.", ToDto(socio));
            }, _logger, "Ocurrió un error guardando el socio.");
        }

        public Task<OperationResult> UpdateAsync(UpdateSocioDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = Validate(dto, out string? cedula);
                if (error != null) return ServiceResult.Fail(error);

                Socio? socio = await _socioRepository.GetEntityByIdAsync(dto.Id);
                if (socio == null || !socio.Status)
                    return ServiceResult.Fail("El socio no existe.");

                string? email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
                error = await CheckUniqueAsync(cedula, email, dto.Id);
                if (error != null) return ServiceResult.Fail(error);

                socio.Nombre = dto.Nombre.Trim();
                socio.Apellido = dto.Apellido.Trim();
                socio.Cedula = cedula;
                socio.Telefono = dto.Telefono?.Trim();
                socio.Email = email;
                socio.FechaNacimiento = dto.FechaNacimiento?.Date;
                socio.UserMod = dto.UserId;

                OperationResult result = await _socioRepository.UpdateEntityAsync(socio);
                if (!result.Success) return result;

                return ServiceResult.Ok("Socio actualizado correctamente.", ToDto(socio));
            }, _logger, "Ocurrió un error actualizando el socio.");
        }

        public Task<OperationResult> RemoveAsync(RemoveDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Socio? socio = await _socioRepository.GetEntityByIdAsync(dto.Id);
                if (socio == null || !socio.Status)
                    return ServiceResult.Fail("El socio no existe.");

                if (await _membresiaRepository.ExistsAsync(MembresiaRules.VigenteEn(dto.Id, DateTime.Today)))
                    return ServiceResult.Fail("No se puede eliminar el socio porque tiene una membresía vigente.");

                socio.Status = false;
                socio.UserMod = dto.UserId;

                OperationResult result = await _socioRepository.UpdateEntityAsync(socio);
                if (!result.Success) return result;

                return ServiceResult.Ok("Socio eliminado correctamente.");
            }, _logger, "Ocurrió un error eliminando el socio.");
        }

        private async Task<string?> CheckUniqueAsync(string? cedula, string? email, int id)
        {
            if (cedula != null && await _socioRepository.ExistsAsync(s => s.Cedula == cedula && s.Id != id && s.Status))
                return "Ya existe un socio con esa cédula.";
            if (email != null && await _socioRepository.ExistsAsync(s => s.Email == email && s.Id != id && s.Status))
                return "Ya existe un socio con ese correo.";
            return null;
        }

        private static string? Validate(SaveSocioDto? dto, out string? cedula)
        {
            cedula = null;
            if (dto == null) return "Los datos del socio son requeridos.";
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return "El nombre es requerido.";
            if (dto.Nombre.Trim().Length > 100) return "El nombre no puede exceder 100 caracteres.";
            if (string.IsNullOrWhiteSpace(dto.Apellido)) return "El apellido es requerido.";
            if (dto.Apellido.Trim().Length > 100) return "El apellido no puede exceder 100 caracteres.";

            if (!string.IsNullOrWhiteSpace(dto.Cedula))
            {
                if (!ValidationHelper.TryNormalizeCedula(dto.Cedula.Trim(), out string normalized))
                    return "La cédula no es válida (debe tener 11 dígitos).";
                cedula = normalized;
            }

            if (!string.IsNullOrWhiteSpace(dto.Telefono) && !ValidationHelper.IsValidPhone(dto.Telefono.Trim()))
                return "El teléfono no es válido.";

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                string email = dto.Email.Trim();
                if (email.Length > 150) return "El correo no puede exceder 150 caracteres.";
                if (!ValidationHelper.IsValidEmail(email)) return "El correo no es válido.";
            }

            if (dto.FechaNacimiento.HasValue
                && (dto.FechaNacimiento.Value.Date > DateTime.Today || dto.FechaNacimiento.Value.Date < DateTime.Today.AddYears(-120)))
                return "La fecha de nacimiento no es válida.";

            return null;
        }

        private static SocioDto ToDto(Socio s) => new SocioDto
        {
            Id = s.Id,
            Nombre = s.Nombre,
            Apellido = s.Apellido,
            Cedula = s.Cedula,
            Telefono = s.Telefono,
            Email = s.Email,
            FechaNacimiento = s.FechaNacimiento,
            Status = s.Status,
            CreationDate = s.CreationDate,
            ModifyDate = s.ModifyDate
        };
    }
}
