using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ginasExcelUnitTests.Model
{
    internal class SubstanceProxy
    {
        internal SubstanceProxy()
        { }

        internal SubstanceProxy(string uuid, string type, bool deprecated, DateTime created,
            string createdBy, DateTime lastModified, string lastModifiedBy)
        {
            UUID = uuid;
            ShortType = type;
            Deprecated = deprecated;
            Created = created;
            CreatedBy = createdBy;
            LastModified = lastModified;
            LastModifiedBy = lastModifiedBy;
        }

        internal SubstanceProxy(string uuid, string type, bool deprecated, string status, DateTime created,
            string createdBy, DateTime lastModified, string lastModifiedBy, string structureId,
            string mixtureId, string nucleicAcidId, string polymerId, string proteinId,
            string specifiedSubstanceId, string structurallyDiverseId, string approvalId)
        {
            UUID = uuid;
            ShortType = type;
            Deprecated = deprecated;
            Status = status;
            Created = created;
            CreatedBy = createdBy;
            LastModified = lastModified;
            LastModifiedBy = lastModifiedBy;
            StructureId = structureId;
            MixtureId = mixtureId;
            NucleicAcidId = nucleicAcidId;
            PolymerId = polymerId;
            ProteinId = proteinId;
            SpecifiedSubstanceId = specifiedSubstanceId;
            StructurallyDiverseId = structurallyDiverseId;
            ApprovalId = approvalId;
        }

        internal string UUID
        {
            get;
            set;
        }

        internal string ShortType
        {
            get;
            set;
        }

        internal string Type
        {
            get
            {
                switch( ShortType)
                {
                    case "DIV":
                        return "structurallyDiverse";
                    case "CHE":
                        return "chemical";
                    case "PRO":
                        return "protein";
                    case "MIX":
                        return "mixture";
                    case "NA":
                        return "nucleicAcid";
                    case "POL":
                        return "polymer";
                    case "SUB":
                        return "concept";
                    default:
                        return string.Empty;

                }
            }
        }

        internal DateTime Created
        {
            get;
            set;
        }

        internal string CreatedBy
        {
            get;
            set;
        }

        internal DateTime LastModified
        {
            get;
            set;
        }

        internal string LastModifiedBy
        {
            get;
            set;
        }

        internal bool Deprecated
        {
            get;
            set;
        }

        internal string Status
        {
            get;
            set;
        }

        internal string StructureId
        {
            get;
            set;
        }

        internal string MixtureId
        {
            get;
            set;
        }

        internal string NucleicAcidId
        {
            get;
            set;
        }

        internal string PolymerId
        {
            get;
            set;
        }

        internal string ProteinId
        {
            get;
            set;
        }

        internal string SpecifiedSubstanceId
        {
            get;
            set;
        }

        internal string StructurallyDiverseId
        {
            get;
            set;
        }

        internal string ApprovalId
        {
            get;
            set;
        }

        internal string ApprovalIdDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ApprovalId)) return Status + " record";
                return ApprovalId;
            }
        }
    }
}
