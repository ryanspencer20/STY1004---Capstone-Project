using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTPlatform.Domain
{
    public interface ISignalRuleRepository
    {
        public void Add(SignalRule rule);
        public void Apply(IntersectionLocation location, SignalRule rule);
        public IReadOnlyList<SignalRule> GetAll();
    }
}