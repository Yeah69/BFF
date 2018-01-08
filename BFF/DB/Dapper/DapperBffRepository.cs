namespace BFF.DB.Dapper
{
    public class DapperBffRepository : BffRepository
    {
        public DapperBffRepository(IProvideConnection provideConnection) 
            : base(provideConnection)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}