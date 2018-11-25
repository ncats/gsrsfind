using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ginasExcelUnitTests.Model
{
    internal class RelatedSubstanceProxy
    {
        public RelatedSubstanceProxy()
        {
        }

        public RelatedSubstanceProxy(string uuid, string refPName, string refUuid, string approvalId)
        {
            UUID = uuid;
            RefPName = refPName;
            RefUIID = refUuid;
            ApprovalId = approvalId;
        }

        internal string UUID
        {
            get;
            set;
        }

        internal string RefPName
        {
            get;
            set;
        }

        internal string RefUIID
        {
            get;
            set;
        }

        internal string ApprovalId
        {
            get;
            set;
        }
    }
}
