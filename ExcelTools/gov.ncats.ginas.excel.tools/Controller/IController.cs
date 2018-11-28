using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.Model;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public interface IController
    {
        void StartOperation();

        object HandleResults(string resultsKey, string message);

        bool StartResolution(bool newSheet);

        void SetExcelWindow(Window window);

        void SetScriptExecutor(IScriptExecutor scriptExecutor);

        void ContinueSetup();

        void Dispose();

        void ReceiveVocabulary(string rawVocab);

        void CancelOperation(string reason);
    }
}
