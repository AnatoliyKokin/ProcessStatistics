using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessStatistics
{
    struct ProcessDebugInfo
    {
        public DateTime Time { get; set; }

        public float CpuUsage { get; set; }

        public long WorkingSet { get; set; }

        public long PrivateBytes { get; set; }

        public int HandleCount { get; set; }

    }

}
