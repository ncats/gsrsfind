using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ginasExcelUnitTests.Model
{
    internal class RelationshipProxy
    {

        internal RelationshipProxy()
        {

        }

        internal RelationshipProxy(string uuid, string ownerUuid, string relatedSubstanceUUID, 
            string relationshipType)
        {
            this.UUID = uuid;
            this.OwnerUUID = ownerUuid;
            this.RelatedSubstanceUUID = relatedSubstanceUUID;
            this.RelationshipType = relationshipType;
        }

        internal string UUID
        {
            get;
            set;
        }

        internal string OwnerUUID
        {
            get;
            set;
        }

        internal string RelatedSubstanceUUID
        {
            get;
            set;
        }

        internal string RelationshipType
        {
            get;
            set;
        }

    }
}
