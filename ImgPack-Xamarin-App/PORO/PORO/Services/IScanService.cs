using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PORO.Services
{
    public interface IScanService
    {
        Task<string> ScanAsync();
    }
}
