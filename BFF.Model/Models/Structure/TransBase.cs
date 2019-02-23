using System;
using BFF.Core.Helper;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models.Structure
{
    public interface ITransBase : ITransLike
    {
        IFlag Flag { get; set; }

        string CheckNumber { get; set; }
        
        DateTime Date { get; set; }
        
        bool Cleared { get; set; }
    }

    internal abstract class TransBase<TDomain, TPersistence> : TransLike<TDomain, TPersistence>, ITransBase 
        where TDomain : class, ITransBase
        where TPersistence : class, IPersistenceModel
    {
        private DateTime _date;
        private bool _cleared;
        private string _checkNumber;
        private IFlag _flag;

        public IFlag Flag
        {
            get => _flag;
            set
            {
                if (_flag == value) return;
                _flag = value;
                UpdateAndNotify();
            }
        }

        public string CheckNumber
        {
            get => _checkNumber;
            set
            {
                if (_checkNumber == value) return;
                _checkNumber = value;
                UpdateAndNotify();
            }
        }
        
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date == value) return;
                var previousDate = _date;
                _date = value;
                UpdateAndNotify();
            }
        }
        
        public bool Cleared
        {
            get => _cleared;
            set
            {
                if (_cleared == value) return;
                _cleared = value;
                UpdateAndNotify();
            }
        }
        
        protected TransBase(
            TPersistence backingPersistenceModel,
            IRepository<TDomain, TPersistence> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            IFlag flag,
            string checkNumber,
            DateTime date,
            bool isInserted,
            string memo,
            bool? cleared) : base(backingPersistenceModel, repository, rxSchedulerProvider, isInserted, memo)
        {
            _flag = flag;
            _checkNumber = checkNumber;
            _date = date;
            _cleared = cleared ?? _cleared;
        }
    }
}