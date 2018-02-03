using System;
using System.Collections.Generic;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class PayeeComparer : Comparer<Domain.IPayee>
    {
        public override int Compare(Domain.IPayee x, Domain.IPayee y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    public interface IPayeeRepository : IObservableRepositoryBase<Domain.IPayee>
    {
    }

    public sealed class PayeeRepository : ObservableRepositoryBase<Domain.IPayee, Payee>, IPayeeRepository
    {
        public PayeeRepository(IProvideConnection provideConnection, ICrudOrm crudOrm) : base(provideConnection, crudOrm, new PayeeComparer()) {}
        
        protected override Converter<Domain.IPayee, Payee> ConvertToPersistence => domainPayee => 
            new Payee
            {
                Id = domainPayee.Id,
                Name = domainPayee.Name
            };

        protected override Converter<Payee, Domain.IPayee> ConvertToDomain => persistencePayee =>
            new Domain.Payee(this, persistencePayee.Id, persistencePayee.Name);
    }
}