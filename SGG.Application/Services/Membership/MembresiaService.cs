using Microsoft.Extensions.Logging;
using SGG.Application.Base;
using SGG.Application.Interfaces;
using SGG.Domain.Base;
using SGG.Domain.Entities.Configuration;
using SGG.Domain.Entities.Membership;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Membership;
using SGG.Persistence.Interfaces;

namespace SGG.Application.Services.Membership
{
    public class MembresiaService : IMembresiaService
    {
        private readonly IMembresiaRepository _membresiaRepository;
        private readonly ISocioRepository _socioRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IPagoRepository _pagoRepository;
        private readonly ILogger<MembresiaService> _logger;

        public MembresiaService(IMembresiaRepository membresiaRepository,
                                ISocioRepository socioRepository,
                                IPlanRepository planRepository,
                                IPagoRepository pagoRepository,
                                ILogger<MembresiaService> logger)
        {
            _membresiaRepository = membresiaRepository;
            _socioRepository = socioRepository;
            _planRepository = planRepository;
            _pagoRepository = pagoRepository;
            _logger = logger;
        }

        public Task<OperationResult> GetAllAsync()
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _membresiaRepository.GetAllAsync(m => m.Status);
                if (!result.Success) return result;

                List<Membresia> membresias = (List<Membresia>)result.Data!;
                return ServiceResult.Ok(data: membresias.OrderByDescending(m => m.FechaInicio).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo las membresías.");
        }

        public Task<OperationResult> GetByIdAsync(int id)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Membresia? membresia = await _membresiaRepository.GetEntityByIdAsync(id);
                if (membresia == null || !membresia.Status)
                    return ServiceResult.Fail("La membresía no existe.");

                return ServiceResult.Ok(data: ToDto(membresia));
            }, _logger, "Ocurrió un error obteniendo la membresía.");
        }

        public Task<OperationResult> GetBySocioAsync(int socioId)
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _membresiaRepository.GetAllAsync(m => m.SocioId == socioId && m.Status);
                if (!result.Success) return result;

                List<Membresia> membresias = (List<Membresia>)result.Data!;
                return ServiceResult.Ok(data: membresias.OrderByDescending(m => m.FechaInicio).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo las membresías del socio.");
        }

        public Task<OperationResult> SaveAsync(SaveMembresiaDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                if (dto == null) return ServiceResult.Fail("Los datos de la membresía son requeridos.");

                Socio? socio = await _socioRepository.GetEntityByIdAsync(dto.SocioId);
                if (socio == null || !socio.Status) return ServiceResult.Fail("El socio indicado no existe.");

                Plan? plan = await _planRepository.GetEntityByIdAsync(dto.PlanId);
                if (plan == null || !plan.Status) return ServiceResult.Fail("El plan indicado no existe.");

                DateTime inicio = (dto.FechaInicio ?? DateTime.Today).Date;
                DateTime vencimiento = inicio.AddDays(plan.DuracionDias);

                string? error = await CheckOverlapAsync(dto.SocioId, inicio, vencimiento, 0);
                if (error != null) return ServiceResult.Fail(error);

                Membresia membresia = new Membresia
                {
                    SocioId = dto.SocioId,
                    PlanId = dto.PlanId,
                    FechaInicio = inicio,
                    FechaVencimiento = vencimiento
                };

                OperationResult result = await _membresiaRepository.SaveEntityAsync(membresia);
                if (!result.Success) return result;

                return ServiceResult.Ok("Membresía guardada correctamente.", ToDto(membresia));
            }, _logger, "Ocurrió un error guardando la membresía.");
        }

        public Task<OperationResult> UpdateAsync(UpdateMembresiaDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                if (dto == null) return ServiceResult.Fail("Los datos de la membresía son requeridos.");

                Membresia? membresia = await _membresiaRepository.GetEntityByIdAsync(dto.Id);
                if (membresia == null || !membresia.Status) return ServiceResult.Fail("La membresía no existe.");

                if (dto.SocioId != membresia.SocioId)
                    return ServiceResult.Fail("No se puede cambiar el socio de una membresía.");

                Plan? plan = await _planRepository.GetEntityByIdAsync(dto.PlanId);
                if (plan == null || !plan.Status) return ServiceResult.Fail("El plan indicado no existe.");

                DateTime inicio = (dto.FechaInicio ?? membresia.FechaInicio).Date;
                DateTime vencimiento = inicio.AddDays(plan.DuracionDias);

                string? error = await CheckOverlapAsync(membresia.SocioId, inicio, vencimiento, dto.Id);
                if (error != null) return ServiceResult.Fail(error);

                membresia.PlanId = dto.PlanId;
                membresia.FechaInicio = inicio;
                membresia.FechaVencimiento = vencimiento;
                membresia.UserMod = dto.UserId;

                OperationResult result = await _membresiaRepository.UpdateEntityAsync(membresia);
                if (!result.Success) return result;

                return ServiceResult.Ok("Membresía actualizada correctamente.", ToDto(membresia));
            }, _logger, "Ocurrió un error actualizando la membresía.");
        }

        public Task<OperationResult> RemoveAsync(RemoveDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Membresia? membresia = await _membresiaRepository.GetEntityByIdAsync(dto.Id);
                if (membresia == null || !membresia.Status) return ServiceResult.Fail("La membresía no existe.");

                int id = dto.Id;
                if (await _pagoRepository.ExistsAsync(p => p.MembresiaId == id && p.Status))
                    return ServiceResult.Fail("No se puede eliminar la membresía porque tiene pagos registrados.");

                membresia.Status = false;
                membresia.UserMod = dto.UserId;

                OperationResult result = await _membresiaRepository.UpdateEntityAsync(membresia);
                if (!result.Success) return result;

                return ServiceResult.Ok("Membresía eliminada correctamente.");
            }, _logger, "Ocurrió un error eliminando la membresía.");
        }

        private async Task<string?> CheckOverlapAsync(int socioId, DateTime inicio, DateTime vencimiento, int id)
        {
            bool solapa = await _membresiaRepository.ExistsAsync(m => m.SocioId == socioId
                                                                      && m.Status
                                                                      && m.Id != id
                                                                      && m.FechaInicio <= vencimiento
                                                                      && m.FechaVencimiento >= inicio);
            return solapa ? "El socio ya tiene una membresía vigente en ese período." : null;
        }

        private static MembresiaDto ToDto(Membresia m) => new MembresiaDto
        {
            Id = m.Id,
            SocioId = m.SocioId,
            PlanId = m.PlanId,
            FechaInicio = m.FechaInicio,
            FechaVencimiento = m.FechaVencimiento,
            Vigente = m.Status && m.FechaInicio <= DateTime.Today && m.FechaVencimiento >= DateTime.Today,
            Status = m.Status,
            CreationDate = m.CreationDate,
            ModifyDate = m.ModifyDate
        };
    }
}
