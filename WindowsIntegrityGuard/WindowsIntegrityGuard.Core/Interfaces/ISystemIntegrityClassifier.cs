using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Interfaces
{
    public interface ISystemIntegrityClassifier
    {
        IntegrityStatus Classify(IntegrityStatus sfcStatus, ComponentStoreHealthStatus componentStoreStatus);
    }
}
