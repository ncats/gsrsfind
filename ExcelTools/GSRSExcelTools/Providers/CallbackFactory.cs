using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;
using GSRSExcelTools.Model.Callbacks;

namespace GSRSExcelTools.Providers
{
    public class CallbackFactory
    {
        public static ResolverCallback CreateResolverCallback(Range t)
        {
            ResolverCallback rcall = new ResolverCallback(t); ;
            return rcall;
        }

        public static CursorBasedResolverCallback CreateCursorBasedResolverCallback(RangeWrapper t )
        {
            CursorBasedResolverCallback rcall = new CursorBasedResolverCallback(t);
            return rcall;
        }

        public static BatchCallback CreateBatchCallback()
        {
            List<Callback> cbs = new List<Callback>();
            return new BatchCallback(cbs);
        }

        //Used to process an update operation
        public static UpdateCallback CreateUpdateCallback(Range s)
        {
            return new UpdateCallback(s); ;
        }

        public static Callback CreateDummyCallback()
        {
            return new Callback();
        }

        public static ImgCallback CreateImgCallback(Range t)
        {
            return new ImgCallback(t);
        }

        public static Update2Callback CreateUpdate2Callback(Range r1, Range r2)
        {
            return new Update2Callback(r1, r2);
        }
    }
}
