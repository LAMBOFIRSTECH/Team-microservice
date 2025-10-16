using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Persistence.DAL;

namespace Teams.INFRA.Layer
{
    public class UnitOfWork : IDisposable
    {
        private ApiContext context = new ApiContext();
        private CommonRepository<Team>? teamRepository;
        public CommonRepository<Team>  TeamRepository
        {
            get
            {

                if (this.teamRepository == null)
                {
                    this.teamRepository = new CommonRepository<Team>(context);
                }
                return teamRepository;
            }
        }
        public void Save() => context.SaveChanges();

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}