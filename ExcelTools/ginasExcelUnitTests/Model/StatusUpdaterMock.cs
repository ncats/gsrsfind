using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using gov.ncats.ginas.excel.tools.Model;

namespace ginasExcelUnitTests.Model
{
    class StatusUpdaterMock : IStatusUpdater
    {
        public void Complete()
        {
            Console.WriteLine("Complete");
        }

        public void UpdateStatus(string message)
        {
            Console.WriteLine(message);
        }
    }
}
