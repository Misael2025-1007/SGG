using SGG.Domain.Base;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Attendance;

namespace SGG.Application.Interfaces
{
    public interface IAsistenciaService
    {
        Task<OperationResult> GetAllAsync();
        Task<OperationResult> GetByIdAsync(int id);
        Task<OperationResult> GetBySocioAsync(int socioId);
        Task<OperationResult> SaveAsync(SaveAsistenciaDto dto);
        Task<OperationResult> UpdateAsync(UpdateAsistenciaDto dto);
        Task<OperationResult> RemoveAsync(RemoveDto dto);
    }
}
