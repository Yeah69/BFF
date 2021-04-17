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
        private readonly IRealmOperations _realmOperations;

        public RealmMergeOrm(
            IProvideRealmConnection provideConnection,
            IRealmOperations realmOperations)
        {
            _provideConnection = provideConnection;
            _realmOperations = realmOperations;
        }

        public Task MergePayeeAsync(Payee from, Payee to)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                _provideConnection.Backup($"BeforeMergeOfPayee{from.Name}ToPayee{to.Name}");
                realm.Write(() =>
                {
                    realm
                        .All<Trans>()
                        .Where(t => t.Payee == from)
                        .ForEach(t =>
                        {
                            t.Payee = to;
                            realm.Add(t, update: true);
                        });
                    realm.Remove(from as RealmObject);
                });
            }
        }

        public Task MergeFlagAsync(Flag from, Flag to)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                _provideConnection.Backup($"BeforeMergeOfFlag{from.Name}ToFlag{to.Name}");
                realm.Write(() =>
                {
                    realm
                        .All<Trans>()
                        .Where(t => t.Flag == from)
                        .ForEach(t =>
                        {
                            t.Flag = to;
                            realm.Add(t, update: true);
                        });
                    realm.Remove(from as RealmObject);
                });
            }
        }

        public Task MergeCategoryAsync(Category from, Category to)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                _provideConnection.Backup($"BeforeMergeOfCategory{from.Name}ToCategory{to.Name}");
                realm.Write(() =>
                {
                    realm
                        .All<Trans>()
                        .Where(t => t.Category == from)
                        .ForEach(t =>
                        {
                            t.Category = to;
                            realm.Add(t, update: true);
                        });
                    realm.Remove(from as RealmObject);
                });
            }
        }
    }
}
