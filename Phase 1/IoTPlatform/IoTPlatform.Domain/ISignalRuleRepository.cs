using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTPlatform.Domain
{
    public interface ISignalRuleRepository
    {
        public void Add(SignalRule rule);
        public SignalRule? GetById(Guid id);
        public IReadOnlyList<SignalRule> GetAll();
    }
}