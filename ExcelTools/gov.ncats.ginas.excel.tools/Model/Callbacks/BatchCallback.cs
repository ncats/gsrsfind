using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace gov.ncats.ginas.excel.tools.Model.Callbacks
{
    public class BatchCallback : Callback
    {
        private List<Callback> icbs = new List<Callback>();

        public BatchCallback(List<Callback> callbacks)
        {
            icbs = callbacks;
        }

        public BatchCallback addCallback(Callback cb)
        {
            icbs.Add(cb);
            return this;
        }

        public new void Execute(dynamic jsResp )
        {
            //base.Execute(jsResp);
            base.is_executed = true; // we cannot call base.Execute because 
            // of dispatching problems but we can accomplish the same thing 
            // via direct call to is_executed. a hack.

            Console.WriteLine("BatchCallback.Execute " + jsResp);
            foreach (Callback c in icbs)
            {
                if (c != null)
                {
                    if( c.getKey().Length >0)
                    {
                        dynamic dynamJsResp = /*(dynamic)*/jsResp;
                        Debug.WriteLine("dynamJs: " + (dynamJsResp as object).GetType().Name);
                        dynamic obj = dynamJsResp.gGet(c.getKey());
                        
                        for( int k = 0; k< obj.length; k++)
                        {
                            string value = obj.getItem(k);
                            c.Execute(value);
                        }
                    }

                }
            }
        }
        public void removeCallback(Callback cb)
        {
            if (icbs == null || icbs.Count == 0) return;
            
            List<int> itemsToRemove = new List<int>();
            for( int i = 0; i < icbs.Count; i++)
            {
                Callback testCb = icbs[i];
                if( testCb.getKey().Equals(cb.getKey()))
                {
                    itemsToRemove.Add(i);
                }
            }
            foreach(int j in itemsToRemove)
            {
                icbs.RemoveAt(j);
            }
        }

        public bool ContainsActiveCallback()
        {
            foreach(Callback cb in icbs)
            {
                if( !cb.hasExecuted() && ! cb.isExpiredNow())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
