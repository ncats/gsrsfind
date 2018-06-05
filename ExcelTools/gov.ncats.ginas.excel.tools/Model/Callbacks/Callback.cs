using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model.Callbacks
{
    public class Callback
    {

        string key;
        protected Boolean is_executed;
        DateTime expirationDate;
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
                if (isExpiredNow())
                {
                    return;
                }
                System.Threading.Thread.Sleep(100);
            } while (true);
        }

        public void setExpiration(DateTime newExpirationDate)
        {
            this.expirationDate = newExpirationDate;
        }

        public Boolean isExpiredNow()
        {
            Boolean expired = false;
            if (is_executed || DateTime.Now > expirationDate)
            {
                expired = true;
            }
            return expired;
        }

        public Boolean hasExecuted()
        {
            return is_executed;
        }


        public void start()
        {
            has_started = true;
        }

        public Boolean hasStarted()
        {
            return has_started;
        }

        public void PreStart()
        {
            has_started = false;
        }

        public void setScript(String scriptText)
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
    }
}
