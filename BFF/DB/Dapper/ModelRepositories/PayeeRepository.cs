using System;
using System.Collections.Generic;
using System.Data.Common;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreatePayeeTable : CreateTableBase
    {
        public CreatePayeeTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.Payee)}s](
            {nameof(Persistance.Payee.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.Payee.Name)} VARCHAR(100));";
        
    }
    
    public class PayeeComparer : Comparer<Domain.IPayee>
    {
        public override int Compare(Domain.IPayee x, Domain.IPayee y)
        {
            return Comparer<string>.Default.Compare(x.Name, y.Name);
        }
    }
    
    public class PayeeRepository : ObservableRepositoryBase<Domain.IPayee, Persistance.Payee>
    {
        public PayeeRepository(IProvideConnection provideConnection) : base(provideConnection, new PayeeComparer()) {}

        public override Domain.IPayee Create() =>
            new Domain.Payee(this);
        
        protected override Converter<Domain.IPayee, Persistance.Payee> ConvertToPersistance => domainPayee => 
            new Persistance.Payee
            {
                Id = domainPayee.Id,
                Name = domainPayee.Name
            };

        protected override Converter<(Persistance.Payee, DbConnection), Domain.IPayee> ConvertToDomain => tuple =>
        {
            (Persistance.Payee persistencePayee, _) = tuple;
            return new Domain.Payee(this, persistencePayee.Id, persistencePayee.Name);
        };
    }
}