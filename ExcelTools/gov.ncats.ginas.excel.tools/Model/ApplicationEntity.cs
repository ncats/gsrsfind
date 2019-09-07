using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{

    public class ApplicationEntity
    {
        

        public ApplicationEntity()
        {
            EntityFields = new List<ApplicationField>();
            LowerLevelEntities = new List<ApplicationEntity>();
        }

        public List<ApplicationField> EntityFields
        {
            get;
            set;
        }

        public ApplicationField.Level ItemLevel
        {
            get;
            set;
        }

        public List<ApplicationEntity> LowerLevelEntities
        {
            get;
            set;
        }
    }
}
