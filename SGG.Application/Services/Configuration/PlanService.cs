using Microsoft.Extensions.Logging;
using SGG.Application.Base;
using SGG.Application.Interfaces;
using SGG.Domain.Base;
using SGG.Domain.Entities.Configuration;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Configuration;
using SGG.Persistence.Interfaces;

namespace SGG.Application.Services.Configuration
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;
        private readonly IMembresiaRepository _membresiaRepository;
        private readonly ILogger<PlanService> _logger;

        public PlanService(IPlanRepository planRepository,
                           IMembresiaRepository membresiaRepository,
                           ILogger<PlanService> logger)
        {
            _planRepository = planRepository;
            _membresiaRepository = membresiaRepository;
            _logger = logger;
        }

        public Task<OperationResult> GetAllAsync()
        {
            return ServiceResult.TryAsync(async () =>
            {
                OperationResult result = await _planRepository.GetAllAsync(p => p.Status);
                if (!result.Success) return result;

                List<Plan> plans = (List<Plan>)result.Data!;
                return ServiceResult.Ok(data: plans.OrderBy(p => p.Nombre).Select(ToDto).ToList());
            }, _logger, "Ocurrió un error obteniendo los planes.");
        }

        public Task<OperationResult> GetByIdAsync(int id)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Plan? plan = await _planRepository.GetEntityByIdAsync(id);
                if (plan == null || !plan.Status)
                    return ServiceResult.Fail("El plan no existe.");

                return ServiceResult.Ok(data: ToDto(plan));
            }, _logger, "Ocurrió un error obteniendo el plan.");
        }

        public Task<OperationResult> SaveAsync(SavePlanDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = Validate(dto);
                if (error != null) return ServiceResult.Fail(error);

                string nombre = dto.Nombre.Trim();
                if (await _planRepository.ExistsAsync(p => p.Nombre == nombre && p.Status))
                    return ServiceResult.Fail("Ya existe un plan con ese nombre.");

                Plan plan = new Plan
                {
                    Nombre = nombre,
                    Descripcion = dto.Descripcion?.Trim(),
                    Precio = dto.Precio,
                    DuracionDias = dto.DuracionDias
                };

                OperationResult result = await _planRepository.SaveEntityAsync(plan);
                if (!result.Success) return result;

                return ServiceResult.Ok("Plan guardado correctamente.", ToDto(plan));
            }, _logger, "Ocurrió un error guardando el plan.");
        }

        public Task<OperationResult> UpdateAsync(UpdatePlanDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                string? error = Validate(dto);
                if (error != null) return ServiceResult.Fail(error);

                Plan? plan = await _planRepository.GetEntityByIdAsync(dto.Id);
                if (plan == null || !plan.Status)
                    return ServiceResult.Fail("El plan no existe.");

                int id = dto.Id;
                string nombre = dto.Nombre.Trim();
                if (await _planRepository.ExistsAsync(p => p.Nombre == nombre && p.Id != id && p.Status))
                    return ServiceResult.Fail("Ya existe otro plan con ese nombre.");

                plan.Nombre = nombre;
                plan.Descripcion = dto.Descripcion?.Trim();
                plan.Precio = dto.Precio;
                plan.DuracionDias = dto.DuracionDias;
                plan.UserMod = dto.UserId;

                OperationResult result = await _planRepository.UpdateEntityAsync(plan);
                if (!result.Success) return result;

                return ServiceResult.Ok("Plan actualizado correctamente.", ToDto(plan));
            }, _logger, "Ocurrió un error actualizando el plan.");
        }

        public Task<OperationResult> RemoveAsync(RemoveDto dto)
        {
            return ServiceResult.TryAsync(async () =>
            {
                Plan? plan = await _planRepository.GetEntityByIdAsync(dto.Id);
                if (plan == null || !plan.Status)
                    return ServiceResult.Fail("El plan no existe.");

                int id = dto.Id;
                DateTime today = DateTime.Today;
                if (await _membresiaRepository.ExistsAsync(m => m.PlanId == id && m.Status && m.FechaVencimiento >= today))
                    return ServiceResult.Fail("No se puede eliminar el plan porque tiene membresías vigentes.");

                plan.Status = false;
                plan.UserMod = dto.UserId;

                OperationResult result = await _planRepository.UpdateEntityAsync(plan);
                if (!result.Success) return result;

                return ServiceResult.Ok("Plan eliminado correctamente.");
            }, _logger, "Ocurrió un error eliminando el plan.");
        }

        private static string? Validate(SavePlanDto? dto)
        {
            if (dto == null) return "Los datos del plan son requeridos.";
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return "El nombre del plan es requerido.";
            if (dto.Nombre.Trim().Length > 100) return "El nombre del plan no puede exceder 100 caracteres.";
            if (dto.Descripcion != null && dto.Descripcion.Trim().Length > 250) return "La descripción no puede exceder 250 caracteres.";
            if (dto.Precio <= 0) return "El precio debe ser mayor que cero.";
            if (dto.Precio > 99999999.99m) return "El precio excede el máximo permitido.";
            if (dto.DuracionDias <= 0) return "La duración en días debe ser mayor que cero.";
            return null;
        }

        private static PlanDto ToDto(Plan p) => new PlanDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            DuracionDias = p.DuracionDias,
            Status = p.Status,
            CreationDate = p.CreationDate,
            ModifyDate = p.ModifyDate
        };
    }
}
