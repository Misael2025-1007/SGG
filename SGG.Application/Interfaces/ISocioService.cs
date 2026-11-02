using SGG.Domain.Base;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Membership;

namespace SGG.Application.Interfaces
{
    public interface ISocioService
    {
        Task<OperationResult> GetAllAsync();
        Task<OperationResult> GetByIdAsync(int id);
        Task<OperationResult> SaveAsync(SaveSocioDto dto);
        Task<OperationResult> UpdateAsync(UpdateSocioDto dto);
        Task<OperationResult> RemoveAsync(RemoveDto dto);
    }
}
