using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Persistence.DAL;

namespace Teams.INFRA.Layer;

public class UnitOfWork : IDisposable
{
    private readonly ApiContext _context;
    private GenericRepository<Team>? _teamRepository;
    public UnitOfWork(ApiContext context) => _context = context;
    public GenericRepository<Team> TeamRepository
    {
        get
        {
            _teamRepository ??= new GenericRepository<Team>(_context); return _teamRepository;
        }
    }
    public async Task SaveAsync() => await _context.SaveChangesAsync();
    public void Dispose() => _context.Dispose();
}