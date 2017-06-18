using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    
    public class PayeeRepository : CachingRepositoryBase<Domain.Payee, Persistance.Payee>
    {
        public ObservableCollection<Domain.IPayee> All { get; }

        public PayeeRepository(IProvideConnection provideConnection) : base(provideConnection)
        {
            List<Domain.Payee> payees = FindAll().ToList();
            All = new ObservableCollection<Domain.IPayee>(payees);
        }

        public override Domain.Payee Create() =>
            new Domain.Payee(this);
        
        protected override Converter<Domain.Payee, Persistance.Payee> ConvertToPersistance => domainPayee => 
            new Persistance.Payee
            {
                Id = domainPayee.Id,
                Name = domainPayee.Name
            };

        protected override Converter<Persistance.Payee, Domain.Payee> ConvertToDomain => persistancePayee =>
            new Domain.Payee(this, persistancePayee.Id, persistancePayee.Name);
    }
}