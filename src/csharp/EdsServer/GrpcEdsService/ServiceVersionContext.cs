using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcEdsService
{
    public class ServiceVersionContext
    {
        public string Version { get; set; }

        public ServiceVersionContext(string version)
        {
            Version = version;
        }
    }
}
