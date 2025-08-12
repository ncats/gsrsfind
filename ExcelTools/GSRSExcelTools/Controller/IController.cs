using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

using GSRSExcelTools.Model;

namespace GSRSExcelTools.Controller
{
    public interface IController
    {
        void StartOperation();

        Task<object> HandleResults(string resultsKey, string message);

        Task<bool> StartResolution(bool newSheet);

        void SetExcelWindow(Window window);

        void SetScriptExecutor(IScriptExecutor scriptExecutor);

        void ContinueSetup();

        void Dispose();

        Task ReceiveVocabulary(string rawVocab);

        void CancelOperation(string reason);

        bool OkToWrite(int numberOfColumns);
    }
}
