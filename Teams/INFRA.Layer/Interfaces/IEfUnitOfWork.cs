namespace Teams.CORE.Layer.Interfaces;

public interface IEfUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
