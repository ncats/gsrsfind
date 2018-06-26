using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model.Callbacks
{
    public class Callback
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string key;
        protected Boolean is_executed;
        DateTime expirationDate = DateTime.Now.AddYears(4);//set far in the future so IsExpiredNow will return false
        Boolean has_started;
        String script;

        public virtual void Execute(dynamic o)
        {
            is_executed = true;
        }

        //'common parts
        public string getKey()
        {
            return key;
        }

        public Callback Reset()
        {
            is_executed = false;
            return this;
        }

        public Callback setKey(String tkey)
        {
            key = tkey;
            return this;
        }

        public void Wait()
        {
            do
            {
                if (IsExpiredNow())
                {
                    return;
                }
                System.Threading.Thread.Sleep(100);
            } while (true);
        }

        public void SetExpiration(DateTime newExpirationDate)
        {
            //log.DebugFormat("Setting expiration to {0} ({1}) on Callback with key {2}. ",
            //    newExpirationDate.ToLongTimeString(), newExpirationDate.ToShortDateString(), key);
            expirationDate = newExpirationDate;
        }

        public Boolean IsExpiredNow()
        {
            Boolean expired = false;
            if (is_executed || DateTime.Now > expirationDate)
            {
                log.DebugFormat("Callback {0} with expiration date {1} has expired.  Executed: {2}",
                    key, expirationDate.ToLongTimeString(), is_executed);
                expired = true;
            }
            return expired;
        }

        public Boolean HasExecuted()
        {
            return is_executed;
        }


        public void Start()
        {
            has_started = true;
        }

        public Boolean HasStarted()
        {
            return has_started;
        }

        public void PreStart()
        {
            has_started = false;
        }

        public void SetScript(String scriptText)
        {
            script = scriptText;
        }

        public string getScript()
        {
            return script;
        }

        public int RowNUmber
        {
            get;
            set;
        }

        public void SetExecuted()
        {
            is_executed = true;
        }
    }
}
