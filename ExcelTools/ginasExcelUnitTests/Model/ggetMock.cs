using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ginasExcelUnitTests.Model
{
    public class ggetMock
    {
        public object gGet(object key)
        {
            return new getItemMock( "value for " + key);
        }
    }

    
}
