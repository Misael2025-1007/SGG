using SGG.Domain.Base;
using SGG.Model.Dtos.Base;
using SGG.Model.Dtos.Membership;

namespace SGG.Application.Interfaces
{
    public interface IPagoService
    {
        Task<OperationResult> GetAllAsync();
        Task<OperationResult> GetByIdAsync(int id);
        Task<OperationResult> GetByMembresiaAsync(int membresiaId);
        Task<OperationResult> SaveAsync(SavePagoDto dto);
        Task<OperationResult> UpdateAsync(UpdatePagoDto dto);
        Task<OperationResult> RemoveAsync(RemoveDto dto);
    }
}
