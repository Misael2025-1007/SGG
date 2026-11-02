using SGG.Domain.Base;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Membership;

namespace SGG.Application.Interfaces
{
    public interface IMembresiaService
    {
        Task<OperationResult> GetAllAsync();
        Task<OperationResult> GetByIdAsync(int id);
        Task<OperationResult> GetBySocioAsync(int socioId);
        Task<OperationResult> SaveAsync(SaveMembresiaDto dto);
        Task<OperationResult> UpdateAsync(UpdateMembresiaDto dto);
        Task<OperationResult> RemoveAsync(RemoveDto dto);
    }
}
