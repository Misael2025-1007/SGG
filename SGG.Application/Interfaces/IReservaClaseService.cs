using SGG.Domain.Base;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Attendance;

namespace SGG.Application.Interfaces
{
    public interface IReservaClaseService
    {
        Task<OperationResult> GetAllAsync();
        Task<OperationResult> GetByIdAsync(int id);
        Task<OperationResult> GetBySocioAsync(int socioId);
        Task<OperationResult> SaveAsync(SaveReservaClaseDto dto);
        Task<OperationResult> UpdateAsync(UpdateReservaClaseDto dto);
        Task<OperationResult> RemoveAsync(RemoveDto dto);
    }
}
