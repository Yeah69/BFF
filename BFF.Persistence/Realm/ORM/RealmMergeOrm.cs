using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using JetBrains.Annotations;
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

        public Task MergePayeeAsync([NotNull]IPayeeRealm from, [NotNull]IPayeeRealm to)
        {
            from = from ?? throw new ArgumentNullException(nameof(from));
            to = to ?? throw new ArgumentNullException(nameof(to));
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                _provideConnection.Backup($"BeforeMergeOfPayee{from.Name}ToPayee{to.Name}");
                realm.Write(() =>
                {
                    realm
                        .All<Trans>()
                        .Where(t => t.PayeeRef == from)
                        .ForEach(t =>
                        {
                            t.Payee = to;
                            realm.Add(t, update: true);
                        });
                    realm.Remove(from as RealmObject);
                });
            }
        }

        public Task MergeFlagAsync([NotNull]IFlagRealm from, [NotNull]IFlagRealm to)
        {
            from = from ?? throw new ArgumentNullException(nameof(from));
            to = to ?? throw new ArgumentNullException(nameof(to));
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                _provideConnection.Backup($"BeforeMergeOfFlag{from.Name}ToFlag{to.Name}");
                realm.Write(() =>
                {
                    realm
                        .All<Trans>()
                        .Where(t => t.FlagRef == from)
                        .ForEach(t =>
                        {
                            t.Flag = to;
                            realm.Add(t, update: true);
                        });
                    realm.Remove(from as RealmObject);
                });
            }
        }

        public Task MergeCategoryAsync([NotNull]ICategoryRealm from, [NotNull]ICategoryRealm to)
        {
            from = from ?? throw new ArgumentNullException(nameof(from));
            to = to ?? throw new ArgumentNullException(nameof(to));
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                _provideConnection.Backup($"BeforeMergeOfCategory{from.Name}ToCategory{to.Name}");
                realm.Write(() =>
                {
                    realm
                        .All<Trans>()
                        .Where(t => t.CategoryRef == from)
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
