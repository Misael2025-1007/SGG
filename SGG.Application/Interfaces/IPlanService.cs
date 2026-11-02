using SGG.Domain.Base;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Configuration;

namespace SGG.Application.Interfaces
{
    public interface IPlanService
    {
        Task<OperationResult> GetAllAsync();
        Task<OperationResult> GetByIdAsync(int id);
        Task<OperationResult> SaveAsync(SavePlanDto dto);
        Task<OperationResult> UpdateAsync(UpdatePlanDto dto);
        Task<OperationResult> RemoveAsync(RemoveDto dto);
    }
}
