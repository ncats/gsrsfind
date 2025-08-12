using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    public interface IScriptExecutor
    {
        Task<object> ExecuteScript(string script);

        Task ExecuteScriptNoReturn(string script);

        void SetScript(string script);

        void SetController(Controller.IController controller);
    }
}
