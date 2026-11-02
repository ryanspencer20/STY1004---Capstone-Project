using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTPlatform.Domain
{
    public interface IViolationLogRepository
    {
        public void SaveViolation(ViolationLog violation);
    }
}