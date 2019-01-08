using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ginasExcelUnitTests.Model
{
    class CodeProxy
    {
        internal CodeProxy()
        {
        }

        internal CodeProxy(string uuid, string code, string codeSystem, string codeText, string comments, string type,
            string url)
        {
            UUID = uuid;
            Code = code;
            CodeSystem = codeSystem;
            CodeText = codeText;
            Comments = comments;
            Type = type;
        }


        internal string UUID
        {
            get;
            set;
        }

        internal string Code
        {
            get;
            set;
        }

        internal string CodeSystem
        {
            get;
            set;
        }

        internal string CodeText
        {
            get;
            set;
        }

        internal string Comments
        {
            get;
            set;
        }

        internal string Type
        {
            get;
            set;
        }

        internal string Url
        {
            get;
            set;
        }
    }
}
