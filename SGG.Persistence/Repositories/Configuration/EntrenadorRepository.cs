using Microsoft.Extensions.Logging;
using SGG.Domain.Base;
using SGG.Domain.Entities.Configuration;
using SGG.Persistence.Base;
using SGG.Persistence.Context;
using SGG.Persistence.Interfaces;

namespace SGG.Persistence.Repositories.Configuration
{
    public class EntrenadorRepository : BaseRepository<Entrenador>, IEntrenadorRepository
    {
        private readonly SGGContext _context;
        private readonly ILogger<EntrenadorRepository> _logger;

        public EntrenadorRepository(SGGContext context, ILogger<EntrenadorRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<OperationResult> SaveEntityAsync(Entrenador entity)
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

        public override async Task<OperationResult> UpdateEntityAsync(Entrenador entity)
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
