using gov.ncats.ginas.excel.tools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model.FDAApplication
{
    /// <summary>
    /// Manages the metadat for Application creation
    /// </summary>
    public class ApplicationMetadata
    {
        static private List<ApplicationField> metadata;
        static ApplicationMetadata()
        {
            BuildMetadata();
        }

        public static IEnumerable<string> GetFieldNames(ApplicationField.Level level)
        {
            return GetFields(level).Select(s => s.FieldName);
        }

        public static IEnumerable<ApplicationField> GetFields()
        {
            return metadata.Select(item => item.Clone()).ToList();
        }

        public static IEnumerable<ApplicationField> GetFields(ApplicationField.Level level)
        {
            return metadata.Where(f => f.FieldLevel == level);
        }

        public static IEnumerable<ApplicationField> GetVocabularyFields()
        {
            return metadata.Where(f => !string.IsNullOrEmpty(f.VocabularyName));
        }

        public static ApplicationField GetFieldForVocab(string vocabName)
        {
            return metadata.Where(f => !string.IsNullOrEmpty(f.VocabularyName) && f.VocabularyName.Equals(vocabName)).First();
        }

        public static ApplicationField GetFieldForVocab(string vocabName, ApplicationField.Level level)
        {
            return metadata.FirstOrDefault(f => !string.IsNullOrEmpty(f.VocabularyName) && f.VocabularyName.Equals(vocabName) && f.FieldLevel==level);
        }

        public static List<ApplicationField> Metadata
        {
            get
            {
                return metadata;
            }
        }

        private static void BuildMetadata()
        {
            metadata = FileUtils.GetApplicationMetadata();
        }
    }
}
