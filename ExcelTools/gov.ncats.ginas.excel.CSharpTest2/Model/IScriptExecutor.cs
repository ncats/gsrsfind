using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.CSharpTest2.Model
{
    public interface IScriptExecutor
    {
        object ExecuteScript(string script);
    }
}
