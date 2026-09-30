using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsIntegrityGuard.Core.Enums
{
    public enum DigitalSignatureStatus
    {
        Trusted,
        Unsigned,
        Untrusted,
        Inconclusive,
        Failed
    }
}
