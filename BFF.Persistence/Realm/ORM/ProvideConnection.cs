using BFF.Core.Persistence;
using BFF.Persistence.Common;
using BFF.Persistence.Contexts;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Realms;

namespace BFF.Persistence.Realm.ORM
{

    public interface IProvideRealmConnection : IProvideConnection<Realms.Realm>
    { }

    internal class ProvideConnection : ProvideConnectionBase<Realms.Realm>, IProvideRealmConnection
    {
        private readonly byte[] _hash;

        public override Realms.Realm Connection
        {
            get
            {
                var config = new RealmConfiguration(DbPath)
                {
                    SchemaVersion = 0
                };
                if (_hash != null)
                    config.EncryptionKey = _hash;
                var realm = Realms.Realm.GetInstance(config);
                return realm;
            }
        }

        protected override string ConnectionString => DbPath;

        public ProvideConnection(
            IRealmFileAccessConfiguration config) : base(config)
        {
            if (config.Password != null)
                _hash = GetHash(config.Password);
        }

        private static byte[] GetHash(string password)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(password);
            IDigest digest = new Sha3Digest(512);
            digest.BlockUpdate(data, 0, data.Length);
            var result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);
            return result;
        }
    }
}