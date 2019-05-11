using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Common;

namespace BFF.Persistence.Contexts
{
    internal class CreateProjectContext : ICreateProjectContext
    {
        private readonly IDisposable _disposeContext;
        private readonly ICreateBackendOrm _createBackendOrm;

        public CreateProjectContext(
            // parameters
            IDisposable disposeContext,

            // dependencies
            ICreateBackendOrm createBackendOrm)
        {
            _disposeContext = disposeContext;
            _createBackendOrm = createBackendOrm;
        }

        public void Dispose()
        {
            _disposeContext?.Dispose();
        }

        public Task CreateProjectAsync()
        {
            return _createBackendOrm.CreateAsync();
        }
    }
}
