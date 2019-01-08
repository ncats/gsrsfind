using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public interface IScriptExecutor
    {
        object ExecuteScript(string script);

        void SetScript(string script);

        void SetController(Controller.IController controller);
    }
}
