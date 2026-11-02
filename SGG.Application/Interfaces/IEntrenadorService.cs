using SGG.Domain.Base;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Configuration;

namespace SGG.Application.Interfaces
{
    public interface IEntrenadorService
    {
        Task<OperationResult> GetAllAsync();
        Task<OperationResult> GetByIdAsync(int id);
        Task<OperationResult> SaveAsync(SaveEntrenadorDto dto);
        Task<OperationResult> UpdateAsync(UpdateEntrenadorDto dto);
        Task<OperationResult> RemoveAsync(RemoveDto dto);
    }
}
