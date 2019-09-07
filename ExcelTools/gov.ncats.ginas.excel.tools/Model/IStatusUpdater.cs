using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public interface IStatusUpdater
    {
        void UpdateStatus(string message);

        void Complete();

        bool GetDebugSetting();

    }
}
