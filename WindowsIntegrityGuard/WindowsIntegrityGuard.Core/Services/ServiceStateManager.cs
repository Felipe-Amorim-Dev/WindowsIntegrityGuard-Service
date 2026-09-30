using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class ServiceStateManager
    {
        private readonly object _lock = new();
        private ServiceStatus _status = ServiceStatus.Idle;

        public ServiceStatus Status
        {
            get
            {
                lock (_lock)
                {
                    return _status;
                }
            }
        }

        public bool TryTransition(ServiceStatus nextStatus)
        {
            lock (_lock)
            {
                bool isValid = (_status, nextStatus) switch
                {
                    (ServiceStatus.Idle, ServiceStatus.Scanning) => true,
                    (ServiceStatus.Scanning, ServiceStatus.Repairing) => true,
                    (ServiceStatus.Scanning, ServiceStatus.Completed) => true,
                    (ServiceStatus.Scanning, ServiceStatus.Failed) => true,
                    (ServiceStatus.Repairing, ServiceStatus.Completed) => true,
                    (ServiceStatus.Repairing, ServiceStatus.Failed) => true,
                    (ServiceStatus.Completed, ServiceStatus.Idle) => true,
                    (ServiceStatus.Failed, ServiceStatus.Idle) => true,
                    _ => false
                };

                if (!isValid)
                {
                    return false;
                }

                _status = nextStatus;
                return true;
            }
        }
    }
