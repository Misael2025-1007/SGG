using Microsoft.Extensions.Logging;
using SGG.Domain.Base;
using SGG.Domain.Entities.Attendance;
using SGG.Persistence.Base;
using SGG.Persistence.Context;
using SGG.Persistence.Interfaces;

namespace SGG.Persistence.Repositories.Attendance
{
    public class ReservaClaseRepository : BaseRepository<ReservaClase>, IReservaClaseRepository
    {
        private readonly SGGContext _context;
        private readonly ILogger<ReservaClaseRepository> _logger;

        public ReservaClaseRepository(SGGContext context, ILogger<ReservaClaseRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<OperationResult> SaveEntityAsync(ReservaClase entity)
        {
            OperationResult result = new OperationResult();
            try
            {
                if (entity == null)
                {
                    result.Success = false;
                    result.Message = "La entidad es requerida.";
                    return result;
                }
                return await base.SaveEntityAsync(entity);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Ocurrió un error guardando los datos.";
                _logger.LogError(ex, result.Message);
            }
            return result;
        }

        public override async Task<OperationResult> UpdateEntityAsync(ReservaClase entity)
        {
            OperationResult result = new OperationResult();
            try
            {
                if (entity == null)
                {
                    result.Success = false;
                    result.Message = "La entidad es requerida.";
                    return result;
                }
                entity.ModifyDate = DateTime.Now;
                return await base.UpdateEntityAsync(entity);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Ocurrió un error actualizando los datos.";
                _logger.LogError(ex, result.Message);
            }
            return result;
        }
    }
}
