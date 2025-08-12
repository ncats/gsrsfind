using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ginasExcelUnitTests.Model
{
    public class getItemMock
    {
        public getItemMock(string newData)
        {
            data = newData;
        }

        string data;

        public int length
        {
            get
            {
                return (data == null) ? 0 : data.Length;
            }
        }

        public object getItem(int k)
        {
            return "item for " + k + " " + data;
        }

        public object popItem(int v)
        {
            return "popped item for " + v + " " + data;
        }
    }
}
