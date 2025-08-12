using GSRSExcelTools.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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

        public bool GetDebugSetting()
        {
            return true;
        }

        public bool HasUserCancelled()
        {
            return false;
        }
    }
}
