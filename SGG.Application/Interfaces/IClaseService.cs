using SGG.Domain.Base;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Configuration;

namespace SGG.Application.Interfaces
{
    public interface IClaseService
    {
        Task<OperationResult> GetAllAsync();
        Task<OperationResult> GetByIdAsync(int id);
        Task<OperationResult> GetByEntrenadorAsync(int entrenadorId);
        Task<OperationResult> SaveAsync(SaveClaseDto dto);
        Task<OperationResult> UpdateAsync(UpdateClaseDto dto);
        Task<OperationResult> RemoveAsync(RemoveDto dto);
    }
}
