using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class SystemIntegrityClassifier : ISystemIntegrityClassifier
    {
        public IntegrityStatus Classify(IntegrityStatus sfcStatus, ComponentStoreHealthStatus componentStoreStatus)
        {
            if (sfcStatus == IntegrityStatus.Failed)
            {
                return IntegrityStatus.Failed;
            }

            if (sfcStatus == IntegrityStatus.Corrupted)
            {
                return IntegrityStatus.Corrupted;
            }

            if (componentStoreStatus is ComponentStoreHealthStatus.Repairable or ComponentStoreHealthStatus.NonRepairable)
            {
                return IntegrityStatus.Corrupted;
            }

            if (sfcStatus == IntegrityStatus.Healthy && componentStoreStatus == ComponentStoreHealthStatus.Healthy)
            {
                return IntegrityStatus.Healthy;
            }

            return IntegrityStatus.Inconclusive;
        }
    }
}
