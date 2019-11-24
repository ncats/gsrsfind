using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Utils;

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
        internal const string APPLICATION_ID_PROPERTY = "Application ID";

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
            for (int p = 0; p < application.LowerLevelEntities.Count; p++)
            {
                stringBuilder.Append("{");
                AppendEntityFieldsToJson(application.LowerLevelEntities[p], stringBuilder);

                stringBuilder.Append(", \"applicationIngredientList\": [");
                for (int i = 0; i < application.LowerLevelEntities[p].LowerLevelEntities.Count; i++)
                {
                    stringBuilder.Append("{");
                    AppendEntityFieldsToJson(application.LowerLevelEntities[p].LowerLevelEntities[i], stringBuilder);
                    stringBuilder.Append("}");
                    if (i < application.LowerLevelEntities[p].LowerLevelEntities.Count - 1) stringBuilder.Append(",");
                }
                stringBuilder.Append(" ] ");
                stringBuilder.Append("}");
                if (p < application.LowerLevelEntities.Count - 1) stringBuilder.Append(",");
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

            for (int f = 0; f < entity.EntityFields.Count; f++)
            {
                ApplicationField field = entity.EntityFields[f];
                if (string.IsNullOrEmpty(field.JsonFieldName)) continue;

                if (!string.IsNullOrEmpty(field.ParentEntityName))
                {
                    json.Append("\"");
                    json.Append(field.ParentEntityName);
                    json.Append("\": [{");
                }
                if (string.IsNullOrEmpty(field.VocabularyName))
                {
                    json.AppendFormat("\"{0}\" : \"{1}\"", field.JsonFieldName, field.GetValue());
                }
                else
                {
                    json.AppendFormat("\"{0}\" : ", field.JsonFieldName);
                    json.Append("{ ");
                    json.AppendFormat("\"value\": \"{0}\"", field.GetValue());
                    json.Append(" } ");
                }

                if (!string.IsNullOrEmpty(field.ParentEntityName))
                {
                    json.Append(" } ]");
                }
                if (f < entity.EntityFields.Count - 1) json.Append(",");
            }
            return true;
        }

        public ApplicationEntity GetApplication(Worksheet worksheet)
        {
            _worksheet = worksheet;

            List<ApplicationEntity> ingredients = ParseEntity(worksheet, ApplicationField.Level.Ingredient);
            if (ScriptExecutor != null) ResolveIngredients(ingredients);

            List<ApplicationEntity> products = ParseEntity(worksheet, ApplicationField.Level.Product);
            products.First().LowerLevelEntities = ingredients;
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
                    string fieldValue = headerCell.Offset[rowOffset, 0].Value2.ToString();
                    ApplicationField baseField = fields.First(f => f.FieldLevel == level
                        && f.FieldName.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase));
                    ApplicationField entityField = baseField.Clone();
                    entityField.FieldValue = fieldValue;
                    entity.EntityFields.Add(entityField);
                }
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
                if (message.Contains("\"valid\":true") && message.Contains(appIdInfo))
                {
                    int pos = message.IndexOf(appIdInfo) + appIdInfo.Length;
                    int pos2 = message.IndexOf("\"", pos + 1);
                    string appId = message.Substring(pos + 1, (pos2 - pos - 1));
                    UIUtils.ShowMessageToUser("Your application has been created. The ID is: "
                        + appId);
                    SheetUtils.SetSheetPropertyValue(_worksheet, APPLICATION_ID_PROPERTY, appId);
                }
                else
                {
                    UIUtils.ShowMessageToUser(message);
                }

                log.Debug("calling StatusUpdater.Complete");
                StatusUpdater.Complete();
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
            string url = GinasConfiguration.SelectedServer.ServerUrl + _urlEndPoint;
            if (_urlEndPoint.Contains("updateApplication"))
            {
                object AppId = SheetUtils.GetSheetPropertyValue(_worksheet, APPLICATION_ID_PROPERTY);
                url += "?applicationId=" + AppId;
            }
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
            scriptUtils.StartOneLoad(scriptParameters, key, this.GinasConfiguration);
            log.Debug("completed scriptUtils.StartOneLoad");
            return true;
        }


        public void ContinueSetup()
        {
            log.Debug("ContinueSetup");
            Authenticate();
            ApplicationEntity application = GetApplication((Worksheet)ExcelWindow.SelectedSheets.Item[1]);
            if (application == null)
            {
                log.Debug("Application creation aborted because of missing fields");
                return;
            }
            _application = application;

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
                scriptBuilder.Append("'); }).resolve();");
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

        private void StartVocabularyRetrievals()
        {
            log.Debug("Look-ups complete; now vocabularies");
            List<string> vocabularyNames = ApplicationMetadata.Metadata.Where(f =>
                !string.IsNullOrEmpty(f.VocabularyName)).Select(i => i.VocabularyName).ToList();
            scriptUtils.ScriptExecutor = ScriptExecutor;
            scriptUtils.StartVocabularyRetrievals(vocabularyNames);
        }

        public override void CompleteSheet()
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
                entity.ItemLevel);
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
    }
}
