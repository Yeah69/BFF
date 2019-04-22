using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MoreLinq;
using Realms;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmMergeOrm : IMergeOrm
    {
        private readonly IProvideRealmConnection _provideConnection;

        public RealmMergeOrm(IProvideRealmConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task MergePayeeAsync(IPayeeRealm from, IPayeeRealm to)
        {
            _provideConnection.Backup($"BeforeMergeOfPayee{from.Name}ToPayee{to.Name}");
            var realm = _provideConnection.Connection;
            await realm.WriteAsync(r =>
            {
                r.All<Trans>().Where(t => t.Payee != null && t.Payee.Name == from.Name).ForEach(t =>
                {
                    t.Payee = to;
                    r.Add(t, update: true);
                });
                r.Remove(from as RealmObject);
            }).ConfigureAwait(false);
        }

        public async Task MergeFlagAsync(IFlagRealm from, IFlagRealm to)
        {
            _provideConnection.Backup($"BeforeMergeOfFlag{from.Name}ToFlag{to.Name}");
            var realm = _provideConnection.Connection;
            await realm.WriteAsync(r =>
            {
                r.All<Trans>().Where(t => t.Flag != null && t.Flag.Name == from.Name).ForEach(t =>
                {
                    t.Flag = to;
                    r.Add(t, update: true);
                });
                r.Remove(from as RealmObject);
            }).ConfigureAwait(false);
        }

        public async Task MergeCategoryAsync(ICategoryRealm from, ICategoryRealm to)
        {
            _provideConnection.Backup($"BeforeMergeOfCategory{from.Name}ToCategory{to.Name}");
            var realm = _provideConnection.Connection;
            await realm.WriteAsync(r =>
            {
                r.All<Trans>().Where(t => t.Category != null && t.Category.Id == from.Id).ForEach(t =>
                {
                    t.Category = to;
                    r.Add(t, update: true);
                });
                r.Remove(from as RealmObject);
            }).ConfigureAwait(false);
        }
    }
}
