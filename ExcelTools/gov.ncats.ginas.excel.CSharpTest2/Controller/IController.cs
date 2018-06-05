using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.CSharpTest2.Model;

namespace gov.ncats.ginas.excel.CSharpTest2.Controller
{
    public interface IController
    {
        void StartOperation(Window window);

        void HandleResults(string resultsKey, string message);

        bool StartResolution(bool newSheet);

        void SetExcelWindow(Window window);

        void SetScriptExecutor(IScriptExecutor scriptExecutor);

        void ContinueSetup();
    }
}
