using GSRSExcelTools.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace GSRSExcelTools.Utils
{
    /// <summary>
    /// Handles details of working with the Scripts in the ginas JavaScript
    /// </summary>
    public class ScriptUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ScriptUtils));

        private object LOCK_OBJECT = new object();

        private Dictionary<string, Vocab> vocabularies = new Dictionary<string, Vocab>();
        internal const string BOOLEAN_VOCABULARY_NAME = "BOOLEAN VOCABULARY";

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

        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Returns the names of the vocabularies to be retrieved assynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> StartVocabularyRetrievals()
        {
            string thisMethodName = "StartVocabularyRetrievals";
            List<string> vocabularyNames = new List<string>();
            log.DebugFormat("StartVocabularyRetrievals for script name {0}", ScriptName);
            await ScriptExecutor.ExecuteScriptNoReturn("tmpScript=Scripts.get('" + ScriptName + "');");
            await ScriptExecutor.ExecuteScriptNoReturn("tmpRunner=tmpScript.runner();");
            //string debugScript = "JSON.stringify(tmpScript)";
            //object debugResult = await ScriptExecutor.ExecuteScript(debugScript);
            //log.DebugFormat("{0} ran script {1} and got result: {2}", thisMethodName, debugScript, debugResult);
            object lengthRaw = await ScriptExecutor.ExecuteScript("tmpScript.arguments.length");
            int argListLength = Convert.ToInt32(lengthRaw);
            for (int i = 0; i < argListLength; i++)
            {
                //see about a controlled vocabulary
                string vocabularyName = await GetVocabName(i);
                vocabularyName = TextUtils.StripQuotes(vocabularyName);
                log.DebugFormat("in {0}, got vocabularyName: {1}", thisMethodName,
                        vocabularyName);
                if (!string.IsNullOrWhiteSpace(vocabularyName))
                {
                    string vocabScript = "CVHelper.getDictionary('" + vocabularyName + "').get(function(s) {sendMessageBackToCSharp(s);});";
                    await ScriptExecutor.ExecuteScriptNoReturn(vocabScript);
                    vocabularyNames.Add(vocabularyName);
                }
            }
            //semaphore code from https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/

            await semaphoreSlim.WaitAsync();
            try
            {
                expectedVocabularies = vocabularyNames;
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
            }
            log.DebugFormat("In {0}, expectedVocabularies now has {1} items; vocabularyNames has {2}", 
                thisMethodName, expectedVocabularies.Count, vocabularyNames.Count);
            return vocabularyNames;
        }

        public async Task<List<string>> StartVocabularyRetrievals(List<string> vocabularyNames)
        {
            foreach(string vocabularyName in vocabularyNames)
            {
                string vocabScript = "CVHelper.getDictionary('" + vocabularyName + "').get(function(s) {sendMessageBackToCSharp(s);});";
                await ScriptExecutor.ExecuteScriptNoReturn(vocabScript);
            }
            lock (LOCK_OBJECT)
            {
                expectedVocabularies = vocabularyNames;
            }
            return vocabularyNames;
        }

        public async Task<string> GetVocabName(int itemNumber)
        {
            object argTypeRaw = await ScriptExecutor.ExecuteScript("tmpScript.arguments["
                + itemNumber + "].type");
            log.DebugFormat("GetVocab looking at argTypeRaw {0} for arg {1}",
                argTypeRaw, itemNumber);
            if (argTypeRaw != null && argTypeRaw is string && TextUtils.StripQuotes((argTypeRaw as string)).Equals("cv",
                StringComparison.CurrentCultureIgnoreCase))
            {
                object cvTypeRaw = await ScriptExecutor.ExecuteScript("tmpScript.arguments["
                    + itemNumber + "].cvType");
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
            if( string.IsNullOrEmpty(vocabName))
            {
                log.Info("vocabName is null or empty");
                return new List<VocabItem>();
            }
            List<VocabItem> vocabItems = new List<VocabItem>();
            log.DebugFormat("In GetVocabItems, vocabName:{0}", vocabName);
            if( vocabName.Equals(BOOLEAN_VOCABULARY_NAME))
            {
                return GetBooleanVocabularyItems();
            }
            if (!vocabularies.ContainsKey(vocabName)) return vocabItems;
            Vocab vocab = vocabularies[vocabName];
            if(vocab == null || vocab.Content ==null || vocab.Content.Length==0 || vocab.Content[0] ==null)
            {
                log.Warn("No vocab content found!");
                return vocabItems;
            }
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

        public async Task BuildScriptParameters(IEnumerable<string> keys)
        {
            string thisMethodName = "BuildScriptParameters";
            string tempScriptName = "tmpScript";
            log.DebugFormat("{0} using script name {1} ",
                thisMethodName, ScriptName);
            await ScriptExecutor.ExecuteScriptNoReturn(tempScriptName + "=Scripts.get('" + ScriptName+ "');");
            object scriptResult = await ScriptExecutor.ExecuteScript(tempScriptName);
            log.DebugFormat("result of test script: {0}", scriptResult);
            if( scriptResult == null || scriptResult.ToString().Length == 0 )
            {
                await ScriptExecutor.ExecuteScript(tempScriptName + "=Scripts.get('" + ScriptName + "');");
                object scriptResult2 = await ScriptExecutor.ExecuteScript(tempScriptName);
                log.DebugFormat("result of test script(2): {0}", scriptResult2);
            }
            string runnerName = "tmpRunner";
            await ScriptExecutor.ExecuteScript(runnerName + "=" + tempScriptName + ".runner();");

            foreach (string key in keys)
            {
                string testScript = tempScriptName + ".hasArgumentByName('" + key + "')";
                object testValue = await ScriptExecutor.ExecuteScript(testScript);
                log.DebugFormat("restValue: {0}", testValue);
                
                if (testValue is string && (testValue as string).Equals("true",
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    object param =
                        await ScriptExecutor.ExecuteScript(tempScriptName
                        + ".getArgumentByName('" + key + "')");
                    ScriptParameter parameter = JSTools.GetScriptParameterFromString(param as string);
                    scriptParameters.Add(key, parameter);
                }
                else if ( key.StartsWith("property:", StringComparison.CurrentCultureIgnoreCase))
                {
                    log.DebugFormat("going to create arg with name: {0}", key);
                    string addParmScript = tempScriptName + ".addArgument({\"key\": \"" + key + "\", \"name\": \"" 
                        + key + "\"});";
                    await ScriptExecutor.ExecuteScriptNoReturn(addParmScript);
                    object param =
                        await ScriptExecutor.ExecuteScript(tempScriptName
                        + ".getArgumentByName('" + key + "')");
                    ScriptParameter parameter = JSTools.GetScriptParameterFromString(param as string);
                    scriptParameters.Add(key, parameter);
                }
            }
        }

        public async void RunPreliminaries()
        {
            await ScriptExecutor.ExecuteScriptNoReturn("tmpScript=Scripts.get('" + ScriptName + "');");
            await ScriptExecutor.ExecuteScriptNoReturn("tmpRunner=tmpScript.runner();");
        }

        public async void StartOneLoad(Dictionary<string, string> parameterValues, string loadingKey,
            GinasToolsConfiguration configuration)
        {
            string thisMethodName = "StartOneLoad";
            log.DebugFormat("{0} handling loadingKey: {1}", thisMethodName,
                loadingKey);
            string runnerName = "tmpRunner";
            string clearScript = runnerName + ".clearValues();";
            log.DebugFormat("going to run script {0}", clearScript);
            if (ScriptExecutor == null)
            {
                log.Debug("ScriptExecutor null");
            }
            else
            {
                log.Debug("ScriptExecutor has a value");
            }

            await ScriptExecutor.ExecuteScript(clearScript);
            try
            {
                log.Debug("setting up headers");
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("auth-userName", configuration.SelectedServer.Username);
                headers.Add("auth-key", configuration.SelectedServer.PrivateKey);
                foreach (string key in parameterValues.Keys)
                {
                    log.DebugFormat("processing at key {0}", key);
                    //see if there's a vocabulary translation
                    string parameterValue = parameterValues[key];
                    log.Debug("scriptParameters: " + scriptParameters ==null ? "null" : scriptParameters.Count.ToString());
                    if (scriptParameters != null && scriptParameters.ContainsKey(key.ToUpper()))
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
                        if (key.Contains("file path"))
                        {
                            string url = (string) await ScriptExecutor.ExecuteScript("GlobalSettings.getHomeURL()")
                                + "upload";
                            log.DebugFormat("url for uploads: " + url);
                            string filePath = parameterValue.Replace(@"\", "/");
                            log.DebugFormat("uploading using URL: {0} and file path: {1}", url, filePath);
                            bool binaryFile = FileUtils.IsBinary(filePath);
                            //MIME type is not really used on the server but we need a value for the http request...
                            string mimeType = binaryFile ? "application/octet-stream" : "text/plain";
                            FilePostReturn dataResult = RestUtils.ProcessFileSaveRequest(url, "POST", filePath, null,
                                mimeType, headers, binaryFile);
                            log.DebugFormat("posted file {0}; retrieved url: {1}", filePath, dataResult.url);
                            if( dataResult.id.Equals("ERROR"))
                            {
                                UIUtils.ShowMessageToUser(dataResult.name);
                                return;
                            }
                            parameterValue = dataResult.url;
                        }
                    }
                    string paramValueScript = string.Format(runnerName + ".setValue('{0}', '{1}')",
                                key, parameterValue);
                    if (ScriptExecutor == null) 
                    {
                        log.Debug("ScriptExecutor null");
                    }
                    else
                    {
                        log.Debug("has a value");
                    }
                    await ScriptExecutor.ExecuteScript(paramValueScript);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in {0}. message: {1}", thisMethodName, ex.Message);
                log.Warn(ex.StackTrace);
            }
            string executionScript = runnerName + ".execute()" +
                                                     ".get(function(b){cresults['" +
                                                    loadingKey + "']=b;sendMessageBackToCSharp('"
                                                    + loadingKey + "');})";
            _=ScriptExecutor.ExecuteScript(executionScript);
        }

        public void AssignVocabularies()
        {
            DateTime start = DateTime.Now;
            foreach(string key in ScriptParameters.Keys)
            {
                ScriptParameter scriptParameter = ScriptParameters[key];
                if (vocabularies.ContainsKey(scriptParameter.cvType))
                {
                    if(vocabularies[scriptParameter.cvType].Content != null && vocabularies[scriptParameter.cvType].Content.Length > 0)
                    {
                        log.DebugFormat("About to handle vocabulary {0}", scriptParameter.cvType);
                        Dictionary<string, string> translation = GetTranslationDictionary(vocabularies[scriptParameter.cvType]);
                        scriptParameter.Vocabulary = translation;

                    }
                    log.WarnFormat("vocabulary {0} is empty!", scriptParameter.cvType);
                }
            }
            TimeSpan elapsed = DateTime.Now.Subtract(start);
            log.DebugFormat("millisec in {0}: {1}", this,
                elapsed.TotalMilliseconds);
        }

        public List<VocabItem> GetBooleanVocabularyItems()
        {
            List<VocabItem> booleanVocab = new List<VocabItem>();
            booleanVocab.Add(new VocabItem("true", "TRUE", false));
            booleanVocab.Add(new VocabItem("false", "FALSE", false));
            return booleanVocab;
        }
    }

}