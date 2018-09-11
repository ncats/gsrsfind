using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using gov.ncats.ginas.excel.tools.Model;
namespace gov.ncats.ginas.excel.tools.Utils
{
    /// <summary>
    /// Handles details of working with the Scripts in the ginas JavaScript
    /// </summary>
    public class ScriptUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private object LOCK_OBJECT = new object();

        private Dictionary<string, Vocab> vocabularies = new Dictionary<string, Vocab>();
        public Dictionary<string, Vocab> Vocabularies
        {
            get
            {
                return vocabularies;
            }
        }

        private List<string> expectedVocabularies = new List<string>();

        public List<string> ExpectedVocabularies
        {
            get
            {
                return expectedVocabularies;
            }
            set
            {
                expectedVocabularies = value;
            }
        }

        public void MarkVocabArrived(string vocabName)
        {
            lock (LOCK_OBJECT)
            {
                expectedVocabularies.Remove(vocabName);
            }
            
        }

        private Dictionary<string, ScriptParameter> scriptParameters = new Dictionary<string, ScriptParameter>();

        public Dictionary<string, ScriptParameter> ScriptParameters
        {
            get
            {
                return scriptParameters;
            }
        }

        public string ScriptName
        {
            get;
            set;
        }

        public IScriptExecutor ScriptExecutor
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the names of the vocabularies to be retrieved assynchronously
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="ScriptExecutor"></param>
        /// <returns></returns>
        public List<string> StartVocabularyRetrievals()
        {
            List<string> vocabularyNames = new List<string>();
            log.DebugFormat("StartVocabularyRetrievals for script name {0}", ScriptName);
            ScriptExecutor.ExecuteScript("tmpScript=Scripts.get('" + ScriptName + "');");
            ScriptExecutor.ExecuteScript("tmpRunner=tmpScript.runner();");
            object lengthRaw = ScriptExecutor.ExecuteScript("tmpScript.arguments.length");
            int argListLength = Convert.ToInt32(lengthRaw);
            for (int i = 0; i < argListLength; i++)
            {
                //see about a controlled vocabulary
                string vocabularyName = GetVocabName(i);
                if (!string.IsNullOrWhiteSpace(vocabularyName))
                {
                    string vocabScript = "CVHelper.getDictionary('" + vocabularyName + "').get(function(s) {window.external.Notify(s);});";
                    ScriptExecutor.ExecuteScript(vocabScript);
                    vocabularyNames.Add(vocabularyName);
                }
            }
            lock (LOCK_OBJECT)
            {
                expectedVocabularies = vocabularyNames;
            }
            return vocabularyNames;
        }

        public string GetVocabName(int itemNumber)
        {
            object argTypeRaw = ScriptExecutor.ExecuteScript("tmpScript.arguments.getItem("
                + itemNumber + ").type");
            log.DebugFormat("GetVocab looking at argTypeRaw {0} for arg {1}",
                argTypeRaw, itemNumber);
            if (argTypeRaw != null && argTypeRaw is string && (argTypeRaw as string).Equals("cv",
                StringComparison.CurrentCultureIgnoreCase))
            {
                object cvTypeRaw = ScriptExecutor.ExecuteScript("tmpScript.arguments.getItem("
                    + itemNumber + ").cvType");
                if (cvTypeRaw != null && cvTypeRaw is string)
                {
                    string cvType = cvTypeRaw as string;

                    if (!string.IsNullOrWhiteSpace(cvType))
                    {
                        return cvType;
                    }
                }
            }
            return string.Empty;
        }


        public List<VocabItem> GetVocabItems(string vocabName)
        {
            List<VocabItem> vocabItems = new List<VocabItem>();
            if (!vocabularies.ContainsKey(vocabName)) return vocabItems;
            Vocab vocab = vocabularies[vocabName];
            foreach (VocabTerm term in vocab.Content[0].Terms)
            {
                VocabItem vocabItem = new VocabItem(term.Value, term.Display, term.Deprecated);
                vocabItems.Add(vocabItem);
            }
            return vocabItems;
        }

        /// <summary>
        /// Create a case-insensitive dictionary that can be used to convert what a user
        /// types into a cell or selects off a drop-down into the value that the server recognizes.
        /// </summary>
        /// <param name="vocab"></param>
        /// <returns>case-insensitive dictionary </returns>
        private Dictionary<string, string> GetTranslationDictionary(Vocab vocab)
        {
            Dictionary<string, string> items = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (VocabTerm term in vocab.Content[0].Terms)
            {
                if( !term.Deprecated)
                {
                    if( items.ContainsKey(term.Display))
                    {
                        log.WarnFormat("Input contains duplicate vocabulary entry.  Term: {0}. Display: {1}",
                            term.Value, term.Display);
                        continue;
                    }
                    items.Add(term.Display, term.Value);
                }
            }
            return items;
        }

        public void BuildScriptParameters(IEnumerable<string> keys)
        {
            string tempScriptName = "tmpScript";
            log.DebugFormat("{0} using script name {1} ",
                MethodBase.GetCurrentMethod().Name, ScriptName);
            ScriptExecutor.ExecuteScript(tempScriptName + "=Scripts.get('" + ScriptName+ "');");
            string runnerName = "tmpRunner";
            ScriptExecutor.ExecuteScript(runnerName + "=" + tempScriptName + ".runner();");

            foreach (string key in keys)
            {
                string testScript = tempScriptName + ".hasArgumentByName('" + key + "')";
                object testValue = ScriptExecutor.ExecuteScript(testScript);
                
                if (testValue is string && (testValue as string).Equals("true",
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    object param =
                        ScriptExecutor.ExecuteScript(tempScriptName
                        + ".getArgumentByName('" + key + "')");
                    ScriptParameter parameter = JSTools.GetScriptParameterFromString(param as string);
                    scriptParameters.Add(key, parameter);
                }
            }
        }

        public void StartOneLoad(Dictionary<string, string> parameterValues, string loadingKey)
        {
            string runnerName = "tmpRunner";
            
            ScriptExecutor.ExecuteScript(runnerName + ".clearValues();");
            try
            {
                foreach (string key in parameterValues.Keys)
                {
                    //see if there's a vocabulary translation
                    string parameterValue = parameterValues[key];

                    if (scriptParameters.ContainsKey(key.ToUpper()))
                    {
                        ScriptParameter parameter = scriptParameters[key.ToUpper()];
                        if (parameter.Vocabulary != null && parameter.Vocabulary.Count > 0
                            && !parameter.Vocabulary.ContainsValue(parameterValue)
                            && parameter.Vocabulary.ContainsKey(parameterValue))
                        {
                            string newParameterValue =
                                parameter.Vocabulary[parameterValue];
                            log.DebugFormat("Used vocabulary to translate {0} to {1}",
                                parameterValue, newParameterValue);
                            parameterValue = newParameterValue;
                        }
                    }
                    string paramValueScript = string.Format(runnerName + ".setValue('{0}', '{1}')",
                                key, parameterValue);
                    ScriptExecutor.ExecuteScript(paramValueScript);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            string executionScript = runnerName + ".execute()" +
                                                     ".get(function(b){cresults['" +
                                                    loadingKey + "']=b;window.external.Notify('"
                                                    + loadingKey + "');})";
            ScriptExecutor.ExecuteScript(executionScript);
        }

        public void AssignVocabularies()
        {
            foreach(string key in ScriptParameters.Keys)
            {
                ScriptParameter scriptParameter = ScriptParameters[key];
                if (vocabularies.ContainsKey(scriptParameter.cvType))
                {
                    Dictionary<string, string> translation = GetTranslationDictionary(vocabularies[scriptParameter.cvType]);
                    scriptParameter.Vocabulary = translation;
                }
            }
        }
    }

}