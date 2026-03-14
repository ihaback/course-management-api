namespace Backend.Domain.Common.Base;

public interface IRepositoryBase<TModel, TId>
{
    Task<TModel> AddAsync(TModel model, CancellationToken ct);
    Task<TModel?> GetByIdAsync(TId id, CancellationToken ct);
    Task<IReadOnlyList<TModel>> GetAllAsync(CancellationToken ct);
    Task<TModel?> UpdateAsync(TId id, TModel model, CancellationToken ct);
    Task<bool> RemoveAsync(TId id, CancellationToken ct);
}
