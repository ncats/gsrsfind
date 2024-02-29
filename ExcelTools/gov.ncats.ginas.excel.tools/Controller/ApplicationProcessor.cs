using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Configuration;

using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Model.FDAApplication;
using Application = gov.ncats.ginas.excel.tools.Model.FDAApplication.Application;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
using gov.ncats.ginas.excel.tools.Providers;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public class ApplicationProcessor : ControllerBase, IController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private SheetUtils _sheetUtils = new SheetUtils();
        private Worksheet _worksheet;
        private ApplicationEntity _application = null;

        private List<ApplicationEntity> _ingredients;
        private Dictionary<string, ApplicationEntity> _lookups;
        private string _urlEndPoint = "addApplication";
        private string _updateUrlEndPoint = "api/v1/applicationssrs";//"updateApplication";//"applicationssrs";//
        internal const string APPLICATION_ID_PROPERTY = "Application ID";
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); // Add an Application Setting.
        private int _scriptNumber = 0;
        private readonly float _secondsPerScript = 6;
        internal const string ADD_INGREDIENT_SCRIPT_NAME = "Add Ingredient";
        internal const string DEFAULT_PROVENANCE = "DARRTS";

        public ApplicationProcessor()
        {
            CurrentOperationType = OperationType.ProcessApplication;
        }

        public string CreateApplicationJson(ApplicationEntity application)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{");
            if (!AppendEntityFieldsToJson(application, stringBuilder)) return string.Empty; ;

            stringBuilder.Append(", \"applicationProductList\": [");
            stringBuilder.Append("{");

            for (int p = 0; p < application.LowerLevelEntities.Count; p++)
            {
                AppendEntityFieldsToJson(application.LowerLevelEntities[p], stringBuilder);
                stringBuilder.Append(", ");
                stringBuilder.Append(" \"applicationProductNameList\": [");

                List<ApplicationEntity> productNames =
                    application.LowerLevelEntities[p].LowerLevelEntities.Where(
                        e => e.ItemLevel == ApplicationField.Level.ProductName).ToList();
                for (int pn = 0; pn < productNames.Count; pn++)
                {
                    stringBuilder.Append("{");
                    AppendEntityFieldsToJson(productNames[pn], stringBuilder);
                    stringBuilder.Append("}");
                    if (pn < productNames.Count - 1) stringBuilder.Append(",");
                }
                stringBuilder.Append("]");
                stringBuilder.Append(", \"applicationIngredientList\": [");
                List<ApplicationEntity> ingredients =
                    application.LowerLevelEntities[p].LowerLevelEntities.Where(
                        e => e.ItemLevel == ApplicationField.Level.Ingredient).ToList();
                for (int i = 0; i < ingredients.Count; i++)
                {
                    stringBuilder.Append("{");

                    AppendEntityFieldsToJson(ingredients[i], stringBuilder);
                    stringBuilder.Append("}");
                    if (i < ingredients.Count - 1) stringBuilder.Append(",");
                }
                stringBuilder.Append(" ] ");
                stringBuilder.Append("}");
            }
            stringBuilder.Append(" ] ");
            stringBuilder.Append("}");

            return stringBuilder.ToString();
        }

        private bool AppendEntityFieldsToJson(ApplicationEntity entity, StringBuilder json)
        {
            if (_urlEndPoint.Contains("updateApplication")
                && entity.ItemLevel == ApplicationField.Level.Application)
            {
                object applicationId = SheetUtils.GetSheetPropertyValue(_worksheet, APPLICATION_ID_PROPERTY);
                if (applicationId != null)
                {
                    log.Debug("Detected application update with existing ID: " + applicationId);
                    json.Append("\"id\" :");
                    json.Append(applicationId);
                    json.Append(",");
                }
                else
                {
                    UIUtils.ShowMessageToUser("Error! you are updating an Application but the ID was not found within the sheet.");
                    return false;
                }
            }

            List<ApplicationField> fields = entity.EntityFields;
            for (int f = 0; f < fields.Count; f++)
            {
                ApplicationField field = fields[f];
                if (string.IsNullOrEmpty(field.JsonFieldName)) continue;

                if (!string.IsNullOrEmpty(field.ParentEntityName))
                {
                    json.Append("\"");
                    json.Append(field.ParentEntityName);
                    json.Append("\": [{");
                }
                json.AppendFormat("\"{0}\" : \"{1}\"", field.JsonFieldName, field.GetValue());

                if (!string.IsNullOrEmpty(field.ParentEntityName))
                {
                    json.Append(" } ]");
                }
                log.DebugFormat("f: {0}; fields.Count: {1}; json last char {2}", f, fields.Count,
                    json.ToString().Substring(json.Length - 1));
                if (f < fields.Count - 1) json.Append(",");
            }
            return true;
        }

        public ApplicationEntity GetApplication(Worksheet worksheet)
        {
            _worksheet = worksheet;

            List<ApplicationEntity> ingredients = ParseEntity(worksheet, ApplicationField.Level.Ingredient);
            if (ScriptExecutor != null) ResolveIngredients(ingredients);

            List<ApplicationEntity> productNames = ParseEntity(worksheet, ApplicationField.Level.ProductName);

            List<ApplicationEntity> products = ParseEntity(worksheet, ApplicationField.Level.Product);
            //safe assumption because we support only 1 product for now
            products.First().LowerLevelEntities = ingredients;
            products.First().LowerLevelEntities.AddRange(productNames);

            List<ApplicationEntity> topLevel = ParseEntity(worksheet, ApplicationField.Level.Application);
            ApplicationEntity application = topLevel.First();
            application.LowerLevelEntities = products;

            return application;
        }

        public Dictionary<string, string> GetValues(Worksheet worksheet, List<string> parameterNames)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (string parameter in parameterNames)
            {
                Range header = SheetUtils.FindFirstCellWithText(worksheet.UsedRange, parameter);
                string center = header.Offset[1, 0].Value2 as string;
                values.Add(parameter, center);
            }

            return values;
        }

        public List<ApplicationEntity> ParseEntity(Worksheet worksheet, ApplicationField.Level level)
        {
            List<ApplicationEntity> entities = new List<ApplicationEntity>();

            List<ApplicationField> fields = ApplicationMetadata.GetFields(level).Select(f => f.Clone()).ToList();
            string firstFieldName = fields.First().FieldName;
            Range startingCell = SheetUtils.FindFirstCellWithText(worksheet.UsedRange, firstFieldName);

            List<Range> headerCells = new List<Range>();
            Range headerRange = startingCell.EntireRow;
            headerRange = worksheet.Application.Intersect(worksheet.UsedRange, headerRange);
            log.DebugFormat("looking for headers in range with {0} cells", headerRange.Cells.Count);
            foreach (ApplicationField field in fields)
            {
                Range headerCell = SheetUtils.FindFirstCellWithText(headerRange, field.FieldName);
                headerCells.Add(headerCell);
            }
            bool haveData = true;
            int rowOffset = 1;
            while (haveData)
            {
                Range currentRowStart = startingCell.Offset[rowOffset, 0];
                if (currentRowStart.Value2 == null)
                {
                    haveData = false;
                    break;
                }
                ApplicationEntity entity = new ApplicationEntity();
                entity.ItemLevel = level;
                foreach (Range headerCell in headerCells)
                {
                    if (headerCell == null || headerCell.Offset[rowOffset, 0].Value2 == null) continue;
                    string fieldName = headerCell.Value2 as string;
                    string dataType = headerCell.Offset[rowOffset, 0].Value2.GetType().Name;
                    ApplicationField baseField = fields.First(f => f.FieldLevel == level
                        && f.FieldName.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase));
                    ApplicationField entityField = baseField.Clone();

                    if (entityField.IsDate())
                    {
                        log.Debug("Handling a date field");
                        try
                        {
                            entityField.FieldValue = DateTime.FromOADate(Convert.ToDouble(headerCell.Offset[rowOffset, 0].Value2));
                        }
                        catch (ArgumentException)
                        {
                            log.Warn("Date value not recognized: " + headerCell.Offset[rowOffset, 0].Value2);
                            entityField.FieldValue = "";
                        }
                    }
                    else
                    {
                        string fieldValue = headerCell.Offset[rowOffset, 0].Value2.ToString();
                        entityField.FieldValue = fieldValue;
                    }
                    entity.EntityFields.Add(entityField);
                }
                //now add the fields with fixed values

                ApplicationMetadata.GetFields(level)
                    .Where(f => !f.IncludeInSheet && !string.IsNullOrWhiteSpace(f.ResolvedValue))
                    .ToList()
                    .ForEach(f => {
                        ApplicationField entityField = f.Clone();
                        entityField.FieldValue = f.ResolvedValue;
                        entity.EntityFields.Add(entityField);
                        });

                entities.Add(entity);
                rowOffset++;
            }
            return entities;
        }


        /// <summary>
        /// This method exists for compatibility with the interface.
        /// </summary>
        public void StartOperation()
        {
        }

        /// <summary>
        /// Called directly by the ribbon.
        /// </summary>
        public void StartOperation(string endPoint)
        {
            _urlEndPoint = endPoint;
            log.Debug("Starting in StartOperation");
        }

        /// <summary>
        /// Called by ScriptExecutor when it receives info from JavaScript
        /// </summary>
        /// <param name="resultsKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public object HandleResults(string resultsKey, string message)
        {
            log.DebugFormat("Received results for key {0} message {1}", resultsKey, message);
            string appIdInfo = "Created Application with ID";
            if (message.Contains("{\"") && message.EndsWith("]}"))
            {
                MarkLookupReceived(resultsKey, message);
            }
            else
            {
                ApplicationProcessingResult processingResult = JSTools.GetApplicationResultFromString(message);
                log.DebugFormat("received result with message {0}", processingResult.message);
                if (processingResult.valid && processingResult.message.Contains(appIdInfo))
                {
                    int pos = message.IndexOf(appIdInfo) + appIdInfo.Length;
                    int pos2 = message.IndexOf("\"", pos + 1);
                    string appId = message.Substring(pos + 1, (pos2 - pos - 1));
                    UIUtils.ShowMessageToUser("Your application has been created. The ID is: "
                        + appId);
                    SheetUtils.SetSheetPropertyValue(_worksheet, APPLICATION_ID_PROPERTY, appId);
                }
                else if (Callbacks != null)
                {
                    Callback callbackWithKey = Callbacks.FirstOrDefault(c => c.Key.Equals(resultsKey)).Value;
                    if (callbackWithKey != null)
                    {
                        if (callbackWithKey is UpdateCallback)
                            ((UpdateCallback)callbackWithKey).SetRangeText(processingResult.message);
                        Callbacks.Remove(resultsKey);
                        if (Callbacks.Count > 0)
                        {
                            log.Debug(" Starting next callback");
                            StartFirstUpdateCallback();
                        }
                    }
                }
                else
                {
                    UIUtils.ShowMessageToUser(processingResult.message);
                }
                if (CurrentOperationType != OperationType.AddIngredient || Callbacks == null || Callbacks.Count == 0)
                {
                    log.Debug("calling StatusUpdater.Complete");
                    StatusUpdater.Complete();
                }
            }
            return message;
        }

        public bool StartResolution(bool newSheet)
        {
            //translate any vocabulary values
            log.DebugFormat("Starting in {0}", MethodBase.GetCurrentMethod().Name);
            TranslateVocabularies(_application);
            log.Debug("after TranslateVocabularies");
            string applicationJson = CreateApplicationJson(_application);
            if (string.IsNullOrEmpty(applicationJson)) return false;

            log.Debug("created application JSON: " + applicationJson);
            string applicationJsonNoNewLines = applicationJson.Replace(Environment.NewLine, "");
            string url = GinasConfiguration.SelectedServer.ServerUrl;
            url = url + "api/v1/applications";
            //+ "applications/" + _urlEndPoint;
            if (_urlEndPoint.Contains("updateApplication"))
            {
                object AppId = SheetUtils.GetSheetPropertyValue(_worksheet, APPLICATION_ID_PROPERTY);
                url += "?applicationId=" + AppId;
            }
            Authenticate();
            log.DebugFormat("using URL {0}", url);
            ScriptUtils scriptUtils = new ScriptUtils();
            scriptUtils.ScriptExecutor = ScriptExecutor;
            string scriptName = "Process Application";
            scriptUtils.ScriptName = scriptName;
            Dictionary<string, string> scriptParameters = new Dictionary<string, string>();
            scriptParameters.Add("url", url);
            scriptParameters.Add("json", applicationJsonNoNewLines);
            string key = JSTools.RandomIdentifier(10);
            scriptUtils.BuildScriptParameters(scriptParameters.Keys);
            log.Debug("finished scriptUtils.BuildScriptParameters");
            scriptUtils.StartOneLoad(scriptParameters, key, GinasConfiguration);
            log.Debug("completed scriptUtils.StartOneLoad");
            return true;
        }


        public void ContinueSetup()
        {
            log.Debug("ContinueSetup");
            Authenticate();
            if (CurrentOperationType.Equals(OperationType.AddIngredient))
            {
                ScriptUtils scriptUtils = new ScriptUtils();
                if (ScriptExecutor == null) log.Debug("ScriptExecutor is null!!!");
                scriptUtils.ScriptExecutor = ScriptExecutor;
                Callbacks = new Dictionary<string, Callback>();
                ProcessIngredientsFromExcel((Worksheet)ExcelWindow.ActiveSheet);
            }
            else
            {
                ApplicationEntity application = GetApplication((Worksheet)ExcelWindow.SelectedSheets.Item[1]);
                if (application == null)
                {
                    log.Debug("Application creation aborted because of missing fields");
                    return;
                }
                _application = application;
            }
        }

        public bool OkToWrite(int numberOfColumns)
        {
            return true;
        }


        public void ResolveIngredients(List<ApplicationEntity> ingredients)
        {
            log.DebugFormat("Starting in {0} with {1} ingredients", MethodBase.GetCurrentMethod().Name,
                ingredients.Count);
            _ingredients = ingredients;
            _lookups = new Dictionary<string, ApplicationEntity>();
            foreach (ApplicationEntity ingredient in ingredients)
            {
                string identifier = JSTools.RandomIdentifier();
                log.DebugFormat("going to resolve {0}", ingredient.EntityFields.First(
                    f => f.FieldName.ToLower().Contains("name")));

                IEnumerable<ApplicationField> lookupFields = ingredient.EntityFields.Where(f => !string.IsNullOrEmpty(f.Lookup) && f.FieldValue != null);
                IEnumerable<string> queries = lookupFields.Select(f => f.FieldValue.ToString()).Distinct();
                IEnumerable<string> resolvers = lookupFields.Select(f => f.Lookup).Distinct();
                _lookups.Add(identifier, ingredient);

                StringBuilder scriptBuilder = new StringBuilder();
                scriptBuilder.Append("cresults['");
                scriptBuilder.Append(identifier);
                scriptBuilder.Append("'] ={ 'keys':function(){ return _.keys(this); },'Item':function(k){ return this[k]; },");
                scriptBuilder.Append("'add':function(k, v){ if (!this[k]) { this[k] =[]; } this[k].push(v); } }; ResolveWorker.builder().list(");
                scriptBuilder.Append(JSTools.MakeSearchString(queries.ToArray()));
                scriptBuilder.Append(").fetchers(");
                scriptBuilder.Append(JSTools.MakeSearchString(resolvers.ToArray()));
                scriptBuilder.Append(").consumer(function(row){ cresults['");
                scriptBuilder.Append(identifier);
                scriptBuilder.Append("'].add(row.split('	')[0], row); }).finisher(function(){ sendMessageBackToCSharp('");
                scriptBuilder.Append(identifier);
                scriptBuilder.Append("'); }).resolve(true);");
                string script = scriptBuilder.ToString();
                ScriptExecutor.ExecuteScript(script);
            }
        }

        public void MarkLookupReceived(string key, string message)
        {
            Dictionary<string, string[]> returnedValue = JSTools.getDictionaryFromString(message);
            ApplicationEntity ingredient = _lookups[key];

            foreach (var field in ingredient.EntityFields.Where(f => !string.IsNullOrEmpty(f.Lookup) && f.FieldValue != null))
            {
                string[] values = returnedValue[field.FieldValue.ToString()][0].Split('\t');
                if (values.Length > 1) field.ResolvedValue = values[1];
                field.WasResolved = true;//resolves means we searched; may or may not have found a value
            }
            //check if resolution is complete for all ingredients
            if (_ingredients.All(i =>
            {
                return i.EntityFields.All(f => (string.IsNullOrEmpty(f.Lookup)) || f.WasResolved);
            }))
            {
                StartVocabularyRetrievals();
            }
        }


        public void ProcessIngredientsFromExcel(Worksheet ingredientSheet)
        {
            List<ApplicationField> ingredientFields = ApplicationMetadata.GetFields(ApplicationField.Level.AddIngredient).ToList();
            foreach (ApplicationField field in ingredientFields)
            {
                field.Column = SheetUtils.FindColumn(ingredientSheet.Range["A1", "Z1"], field.FieldName, 1);
                if (field.Column <= 0)
                {
                    log.DebugFormat("Field {0} not found on sheet", field.FieldName);
                }
            }
            List<ApplicationField> fieldsToProcess = ingredientFields.Where(f => f.Column > 0).ToList();
            int statusCol = ingredientFields.First(f => f.FieldName.Equals("IMPORT STATUS", StringComparison.CurrentCultureIgnoreCase)).Column;
            if (statusCol == 0)
            {
                string errorMessage = "Unable to locate status column";
                log.Error(errorMessage);
                UIUtils.ShowMessageToUser(errorMessage);
                return;
            }
            log.DebugFormat("found status column: {0}", statusCol);

            foreach (Range currentRowObject in ((Range)ExcelWindow.Application.Selection).Rows)
            {
                List<ApplicationField> fieldsForRow = new List<ApplicationField>(fieldsToProcess);
                int currentRow = currentRowObject.Row;
                Range statusCell = ingredientSheet.Range[SheetUtils.GetColumnName(statusCol) + currentRow.ToString()];
                if (statusCell.Value2 != null)
                {
                    log.InfoFormat("Skipping row {0} because there is a value in the status column", currentRow);
                    continue;
                }
                log.DebugFormat("looking at row {0}", currentRow);
                fieldsForRow.ForEach(f =>
                {
                    f.FieldValue = SheetUtils.GetValueForRowAndColumn(ingredientSheet, currentRow, f.Column);
                });

                string provenance = ingredientFields.First(f => f.FieldName.Equals("Provenance", StringComparison.CurrentCultureIgnoreCase)).GetValue();
                if (String.IsNullOrEmpty(provenance)) provenance = DEFAULT_PROVENANCE;

                string appType = ingredientFields.First(f => f.FieldName.Equals("Application Type", StringComparison.CurrentCultureIgnoreCase)).GetValue();
                string appNumber = ingredientFields.First(f => f.FieldName.Equals("Application Number", StringComparison.CurrentCultureIgnoreCase)).GetValue();
                string center = ingredientFields.First(f => f.FieldName.Equals("Center", StringComparison.CurrentCultureIgnoreCase)).GetValue();
                log.DebugFormat("retrieved params for row {0}: appType={1}, appNumber={2}, center={3}. ",
                    currentRow, appType, appNumber, center);
                //string fullUrl = GinasConfiguration.SelectedServer.ServerUrl.Replace("/ginas/app","") + config.AppSettings.Settings["applicationLookupUrl"].Value;
                string fullUrl = GinasConfiguration.SelectedServer.ServerUrl + config.AppSettings.Settings["applicationLookupUrl"].Value;
                string fullUrlWithParameters = string.Format(fullUrl, appType, appNumber, center, provenance);
                log.DebugFormat("looking up application using URL {0}", fullUrlWithParameters);
                StringBuilder stringBuilder = new StringBuilder();
                UpdateCallback callback = CreateIngredientUpdateCallback(statusCell, fullUrlWithParameters, fieldsForRow, stringBuilder);
                Callbacks.Add(callback.getKey(), callback);
                log.DebugFormat("completed row {0}", currentRow);
            }
            log.DebugFormat("going to call StartFirstUpdateCallback");
            StartFirstUpdateCallback();
        }

        private void StartVocabularyRetrievals()
        {
            log.Debug("Look-ups complete; now vocabularies");
            List<string> vocabularyNames = ApplicationMetadata.Metadata.Where(f =>
                !string.IsNullOrEmpty(f.VocabularyName)).Select(i => i.VocabularyName).ToList().ToList();
            vocabularyNames.ForEach(vn => log.Debug(vn));
            scriptUtils.ScriptExecutor = ScriptExecutor;
            scriptUtils.StartVocabularyRetrievals(vocabularyNames);
        }

        public override void CompleteSheet()//unfortunate name
        {
            log.Debug("Calling StartResolution");
            StartResolution(false);
        }

        /// <summary>
        /// Go through the entity
        /// </summary>
        /// <param name="entity">A holder for a set of fields -- name-value pairs</param>
        private void TranslateVocabularies(ApplicationEntity entity)
        {
            log.DebugFormat("Starting in {0} for item at level {1}", MethodBase.GetCurrentMethod().Name,
                (entity == null ? "[null]" : entity.ItemLevel.ToString()));
            foreach (ApplicationField field in entity.EntityFields)
            {
                if (!string.IsNullOrEmpty(field.VocabularyName))
                {
                    Vocab fieldVocab = scriptUtils.Vocabularies[field.VocabularyName];
                    //look for the vocabulary term whose Display property corresponds to what we found in the
                    // spreadsheet.  Then take the Value property as what we use in the JSON
                    VocabTerm term = fieldVocab.Content[0].Terms.FirstOrDefault(
                        t => t.Display.Equals(field.FieldValue.ToString(), StringComparison.CurrentCultureIgnoreCase));
                    if (term != null)
                    {
                        field.FieldValue = term.Value;
                        log.DebugFormat("Used vocab to translate {0} to {0}", term.Display, term.Value);
                    }
                }
            }
            entity.LowerLevelEntities.ForEach(e => TranslateVocabularies(e));
        }

        public UpdateCallback CreateIngredientUpdateCallback(Range messageCell, string url, List<ApplicationField> fields,
                StringBuilder messageBuilder)
        {
            log.Debug("starting in CreateIngredientUpdateCallback");
            string applicationsUrlEnd = "api/v1/applications";
            if (GinasConfiguration == null)
            {
                log.Debug("configuration was null");
                GinasConfiguration = FileUtils.GetGinasConfiguration();
            }
            UpdateCallback updateCallback = null;
            try
            {
                updateCallback = CallbackFactory.CreateUpdateCallback(messageCell);
                updateCallback.RunnerNumber = ++_scriptNumber;
                log.DebugFormat("in {0}, processing URL {1}", MethodBase.GetCurrentMethod().Name, url);

                string defaultValue = string.Empty;
                updateCallback.ParameterValues = new Dictionary<string, string>();
                updateCallback.ParameterValues.Add("getUrl", url);
                log.DebugFormat("In {0}, setting parm {1} to {2}",
                      MethodBase.GetCurrentMethod().Name,
                      "url", url);
                //string postUrl = GinasConfiguration.SelectedServer.ServerUrl.Replace("ginas/app/", "api/v1/applications");
                string postUrl = GinasConfiguration.SelectedServer.ServerUrl + applicationsUrlEnd;
                updateCallback.ParameterValues.Add("postUrl", postUrl);
                log.DebugFormat("In {0}, setting parm {1} to {2}, from ServerUrl {3} and end part {4}",
                      MethodBase.GetCurrentMethod().Name,
                      "postUrl", postUrl, GinasConfiguration.SelectedServer.ServerUrl, applicationsUrlEnd);
                foreach (ApplicationField field in fields)
                {
                    if (!string.IsNullOrEmpty(field.JsonFieldName) && field.FieldValue != null && field.FieldValue.ToString().Length > 0)
                    {
                        log.DebugFormat("In {0}, setting parm {1} to {2}",
                            MethodBase.GetCurrentMethod().Name,
                            field.FieldName, field.FieldValue);
                        updateCallback.ParameterValues.Add(field.JsonFieldName, field.FieldValue.ToString());
                    }
                }
                string callbackKey = JSTools.RandomIdentifier();
                DateTime newExpirationDate = DateTime.Now.AddSeconds(GinasConfiguration.ExpirationOffset +
                    (Callbacks.Count * Callbacks.Count * _secondsPerScript));//trying a quadratic term
                updateCallback.SetExpiration(newExpirationDate);
                updateCallback.SetKey(callbackKey);
                updateCallback.LoadScriptName = ADD_INGREDIENT_SCRIPT_NAME;
                string script = "tmpRunner.execute().get(function(b){cresults['"
                    + callbackKey + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})";
                updateCallback.SetScript(script);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error creating update callback: {0}", ex.Message);
                log.Debug(ex.StackTrace);
                messageBuilder.Append(ex.Message);
            }
            return updateCallback;
        }


        protected override void StartFirstUpdateCallback()
        {
            log.DebugFormat("Starting in {0}", MethodBase.GetCurrentMethod().Name);
            if (Callbacks.Count == 0) return;
            if (Callbacks.Values.First() is UpdateCallback)
            {
                //scriptUtils.AssignVocabularies();
                UpdateCallback updateCallback = Callbacks.Values.First() as UpdateCallback;
                if ((GinasConfiguration.DebugMode || StatusUpdater.GetDebugSetting())
                    && updateCallback.RunnerNumber % CONSOLE_CLEARANCE_INTERVAL == 0)
                {
                    SaveAndClearDebugInfo();
                }
                DateTime newExpirationDate = DateTime.Now.AddSeconds(GinasConfiguration.ExpirationOffset +
                    updateCallback.RunnerNumber * _secondsPerScript);
                updateCallback.SetExpiration(newExpirationDate);
                scriptUtils.ScriptExecutor = this.ScriptExecutor;
                scriptUtils.ScriptName = ADD_INGREDIENT_SCRIPT_NAME;
                scriptUtils.RunPreliminaries();
                RunUpdateCallback(updateCallback);
                updateCallback.Start();
            }
        }

        private void RunUpdateCallback(UpdateCallback updateCallback)
        {
            log.DebugFormat("RunUpdateCallback handling key {0}", updateCallback.getKey());
            if (updateCallback.ParameterValues == null)
            {
                log.Fatal("Error! ParameterValues is null");
                return;
            }
            log.DebugFormat("total parameters: {0}", updateCallback.ParameterValues.Count);
            scriptUtils.StartOneLoad(updateCallback.ParameterValues, updateCallback.getKey(), this.GinasConfiguration);
        }

    }
}