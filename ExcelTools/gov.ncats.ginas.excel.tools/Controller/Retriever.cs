using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;

using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.UI;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Providers;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public class Retriever : ControllerBase, IController
    {
        public Retriever()
        {
            ItemsPerBatch = GetBatchSize();
        }

        public void SetScriptExecutor(IScriptExecutor scriptExecutor)
        {
            _scriptExecutor = scriptExecutor;
        }

        private IScriptExecutor _scriptExecutor;

        
        
        private int _totalBatches;
        
        private bool _notified = false;

        public void StartOperation(Excel.Window window)
        {
            Callbacks = new Dictionary<string, Callback>();
            ScriptQueue = new Queue<string>();

            CurrentOperationType = OperationType.Resolution;

            CallbackFactory factory = new CallbackFactory();
            Excel.Worksheet activeWorksheet = ((Excel.Worksheet)window.Application.ActiveSheet);
            _selection = window.Application.Selection;
            List<SearchValue> searchValues = GetSearchValues(_selection);
            string callbackKey = JSTools.RandomIdentifier();

            if (searchValues.Any(v => !string.IsNullOrWhiteSpace(v.Value)))
            {
                string searchScript = MakeImageSearch(callbackKey, searchValues.Select(sv => sv.Value).ToList());
                ImgCallback imgCallback = new ImgCallback(_selection);
                RetrievalForm form = new RetrievalForm();
                form.Controller = this;
                form.Show();
                StatusUpdater = form;
                _scriptExecutor = form;
                form.ScriptToExecute = searchScript;
            }
            else
            {
                UIUtils.ShowMessageToUser("Please select a chemical name or ID");
                return;
            }

            if( StatusUpdater != null)
            {
                StatusUpdater.UpdateStatus("Getting user selections");
            }
        }

        public void HandleResults(string resultsKey, string message)
        {
            Debug.WriteLine(string.Format("HandleResults received message {0} for key {1}",
                message, resultsKey));

            Dictionary<string, string[]> returnedValue = JSTools.getDictionaryFromString(message);
            foreach (string key in returnedValue.Keys)
            {
                string[] messageParts = returnedValue[key][0].Split('\t');
                int currentRow = _selection.Row;

                int currentColumn = _selection.Column;
                int dataRow = SheetUtils.FindRow(_selection, key, currentColumn);
                for (int part = 1; part < messageParts.Length; part++)
                {
                    int column = currentColumn + part;
                    string cellId = SheetUtils.GetColumnName(column) + dataRow;
                    string result = messageParts[part];
                    if (result.Equals("[object Object]")) continue;
                    if (ImageOps.IsImageUrl(result))
                    {
                        ImageOps imageOps = new ImageOps();
                        cellId = SheetUtils.GetColumnName(column - 1) + dataRow;
                        Excel.Range currentCell = _selection.Worksheet.Range[cellId];
                        imageOps.AddImageCaption(currentCell, result, 240);
                    }
                    else
                    {
                        Excel.Range currentCell = _selection.Worksheet.Range[cellId];
                        currentCell.Value = result;
                    }

                }
            }
            Callbacks.Remove(resultsKey);
            
            string statusMessage = string.Format("{0} items to go", ScriptQueue.Count);
            if (ScriptQueue.Count == 0) statusMessage = "Processing complete!";
            StatusUpdater.UpdateStatus(statusMessage);
        }

        public bool StartResolution(bool newSheet)
        {
            float secondsPerItem = 0.4f;
            Callbacks = new Dictionary<string, Callback>();
            ScriptQueue = new Queue<string>();
            Excel.Range r = null;
            try
            {
                r = _window.RangeSelection;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);

            }
            if( r== null)
            {
                return false;
            }
            _selection = r;
            BatchCallback cb = CallbackFactory.CreateBatchCallback();
            RangeWrapper wrapped = null;
            /* todo: handle newSheet!
             if( newSheet)
            {
                wrapped = getNewSheetResolverCursor();
            }*/

            int currItem = 0;
            int currItemWithinBatch = 0;
            List<string> preSubmit = new List<string>();
            foreach (Excel.Range cell in r.Cells)
            {
                if (cell.Text != null && (!string.IsNullOrWhiteSpace(cell.Text)))
                {
                    currItemWithinBatch++;
                    currItem++;
                    string cellText = cell.Text;
                    preSubmit.Add(cellText.Replace("'", "\'"));
                    Callback rcb;
                    if (newSheet)
                    {
                        rcb = CallbackFactory.CreateCursorBasedResolverCallback(wrapped);
                    }
                    else
                    {
                        rcb = CallbackFactory.CreateResolverCallback(cell);
                    }
                    rcb.setKey(cellText);
                    DateTime newExpirationDate = DateTime.Now.AddSeconds((currItem * secondsPerItem));
                    rcb.setExpiration(newExpirationDate);
                    cb.addCallback(rcb);

                    if ((currItemWithinBatch % ItemsPerBatch) == 0)
                    {
                        QueueOneBatch(cb, preSubmit);
                        cb = CallbackFactory.CreateBatchCallback();
                        currItemWithinBatch = 0;
                        Debug.Print("Prepared batch containing " + ItemsPerBatch + " items");
                        preSubmit.Clear();
                    }
                }
            }
            if (currItemWithinBatch > 0)// process any leftovers
            {
                QueueOneBatch(cb, preSubmit);
            }

            if (ScriptQueue.Count > 0)
            {
                KeepCheckingCallbacks = true;
                LaunchLastScript();
                LaunchCheckJob();
                _totalBatches = ScriptQueue.Count;
                StatusUpdater.UpdateStatus("Starting...");
            }
            return true;
        }

        //        private RangeWrapper getNewSheetResolverCursor()
        //        {
        //            string[] headers, theaders;
        //            theaders = Split(getScriptPrimitive("_.map($('div.checkop input:checked'), 'name').join('___');"), "___");
        //            headers = new string[1, Information.UBound(theaders) - Information.LBound(theaders) + 1];
        //            int i;

        //            for (i = Information.LBound(theaders); i <= Information.UBound(theaders); i++)
        //                headers[0, i] = theaders[i];

        //            Worksheet nsheet;
        //            ;/* Cannot convert EmptyStatementSyntax, CONVERSION ERROR: Conversion for EmptyStatement not implemented, please report this issue in '' at character 446
        //   at ICSharpCode.CodeConverter.CSharp.VisualBasicConverter.MethodBodyVisitor.DefaultVisit(SyntaxNode node)
        //   at Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxVisitor`1.VisitEmptyStatement(EmptyStatementSyntax node)
        //   at Microsoft.CodeAnalysis.VisualBasic.Syntax.EmptyStatementSyntax.Accept[TResult](VisualBasicSyntaxVisitor`1 visitor)
        //   at Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxVisitor`1.Visit(SyntaxNode node)
        //   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.ConvertWithTrivia(SyntaxNode node)
        //   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.DefaultVisit(SyntaxNode node)

        //Input: 
        //    Set nsheet = ActiveWorkbook.Sheets.Add

        // */
        //            nsheet.Name = ExcelTools.getNewSheetName("Resolved");
        //            nsheet.Range("A1").FormulaR1C1 = "Query";
        //            nsheet.Range("B1").Resize(1, Information.UBound(theaders) - Information.LBound(theaders) + 1) = headers;
        //            RangeWrapper wrapped;
        //            ;/* Cannot convert EmptyStatementSyntax, CONVERSION ERROR: Conversion for EmptyStatement not implemented, please report this issue in '' at character 707
        //   at ICSharpCode.CodeConverter.CSharp.VisualBasicConverter.MethodBodyVisitor.DefaultVisit(SyntaxNode node)
        //   at Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxVisitor`1.VisitEmptyStatement(EmptyStatementSyntax node)
        //   at Microsoft.CodeAnalysis.VisualBasic.Syntax.EmptyStatementSyntax.Accept[TResult](VisualBasicSyntaxVisitor`1 visitor)
        //   at Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxVisitor`1.Visit(SyntaxNode node)
        //   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.ConvertWithTrivia(SyntaxNode node)
        //   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.DefaultVisit(SyntaxNode node)

        //Input: 
        //    Set wrapped = RangeWrapperFactory.createRangeWrapper(nsheet.Range("A2"))

        // */
        //            ;/* Cannot convert EmptyStatementSyntax, CONVERSION ERROR: Conversion for EmptyStatement not implemented, please report this issue in '' at character 784
        //   at ICSharpCode.CodeConverter.CSharp.VisualBasicConverter.MethodBodyVisitor.DefaultVisit(SyntaxNode node)
        //   at Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxVisitor`1.VisitEmptyStatement(EmptyStatementSyntax node)
        //   at Microsoft.CodeAnalysis.VisualBasic.Syntax.EmptyStatementSyntax.Accept[TResult](VisualBasicSyntaxVisitor`1 visitor)
        //   at Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxVisitor`1.Visit(SyntaxNode node)
        //   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.ConvertWithTrivia(SyntaxNode node)
        //   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.DefaultVisit(SyntaxNode node)

        //Input: 
        //    Set getNewSheetResolverCursor = wrapped

        // */
        //        }

        public void LaunchLastScript()
        {
            Stopwatch sw = new Stopwatch();
            if (ScriptQueue.Count > 0)
            {
                sw.Start();
                Debug.Print("About to run script from queue. Script queue count: "
                    + ScriptQueue.Count + " at " + DateTime.Now);
                StartCorrespondingCallback(ScriptQueue.Peek());
                _scriptExecutor.ExecuteScript(ScriptQueue.Dequeue());
                StatusUpdater.UpdateStatus("Processing batch " + (_totalBatches - ScriptQueue.Count)
            + " of " + _totalBatches);
                Debug.Print("launching script took: " + sw.ElapsedMilliseconds + " milliseconds");
            }
            else
            {
                StatusUpdater.UpdateStatus("All batches have been processed");
            }
        }

        private void StartCorrespondingCallback(string script)
        {
            // locate the key
            int pos1;
            int pos2;
            string key;
            Callback cb;
            pos1 = script.IndexOf("'");
            pos2 = script.IndexOf("'", pos1 + 1);
            key = script.Substring(pos1 + 1, (pos2 - pos1 -1));
            if (Callbacks.ContainsKey(key))
            {
                cb = Callbacks[key];
                cb.start();
                Debug.Print(" ... found callback and marked it as started");

            }
        }

        private void QueueOneBatch(Callback cb, List<string> submittable)
        {
            cb.setKey(JSTools.RandomIdentifier());
            Callbacks.Add(cb.getKey(), cb);
            Debug.Print("preparing callback with key " + cb.getKey() + " at " + DateTime.Now);
            string script = MakeSearch(cb.getKey(), submittable);
            Debug.Print("script: " + script);

            // executeScript script
            ScriptQueue.Enqueue(script);
        }

        private List<string> GetSimpleSearchValues(Excel.Range selection)
        {
            List<string> searchValues = new List<string>();
            foreach (Excel.Range row in selection.Rows)
            {
                string cellName = SheetUtils.GetColumnName(row.Column) + row.Row;
                string cellValue = selection.Worksheet.get_Range(cellName).Value;
                Debug.WriteLine(string.Format("cell {0} = value: {1}",
                    cellName, cellValue));
                searchValues.Add(cellValue);
            }
            return searchValues;
        }

        private List<SearchValue> GetSearchValues(Excel.Range selection)
        {
            List<SearchValue> searchValues = new List<SearchValue>();
            foreach (Excel.Range row in selection.Rows)
            {
                string cellName = SheetUtils.GetColumnName(row.Column) + row.Row;
                string cellValue = selection.Worksheet.get_Range(cellName).Value;
                Debug.WriteLine(string.Format("cell {0} = value: {1}",
                    cellName, cellValue));
                searchValues.Add(new SearchValue(cellValue, row.Row));
            }
            return searchValues;
        }

        private string MakeImageSearch(string key, List<string> names)
        {
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.Append("cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("']={'keys':function(){return _.keys(this);},'Item':function(k){return this[k];}, 'add':function(k,v){if(!this[k]){this[k]=[];}this[k].push(v);}};ResolveWorker.builder()");
            scriptBuilder.Append(".list(");
            string arrayedNames = JSTools.MakeSearchString(names.ToArray());
            scriptBuilder.Append(arrayedNames);
            //scriptBuilder.Append("'.split('\n'))");
            scriptBuilder.Append(")");
            scriptBuilder.Append(".fetchers(['Image URL'])");
            scriptBuilder.Append(".consumer(function(row){cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("'].add(row.split('\t')[0],row);})");
            scriptBuilder.Append(".finisher(function(){window.external.Notify('");
            scriptBuilder.Append(key);
            scriptBuilder.Append("');})");
            scriptBuilder.Append(".resolve();");
            return scriptBuilder.ToString();
        }

        private string MakeSearch(string key, List<string> names)
        {
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.Append("cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("']={'keys':function(){return _.keys(this);},'Item':function(k){return this[k];},");
            scriptBuilder.Append("'add':function(k,v){if(!this[k]){this[k]=[];}this[k].push(v);}};");
            scriptBuilder.Append("ResolveWorker.builder()");
            string arrayedNames = JSTools.MakeSearchString(names.ToArray());
            scriptBuilder.Append(".list(");
            scriptBuilder.Append(arrayedNames);
            scriptBuilder.Append(")");
            //scriptBuilder.Append(".list('$NAMES$'.split('\n'))");
            scriptBuilder.Append(".fetchers(_.map($('div.checkop input:checked'), 'name'))");
            scriptBuilder.Append(".consumer(function(row){cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("'].add(row.split('\t')[0],row);})");
            scriptBuilder.Append(".finisher(function(){window.external.Notify('");
            scriptBuilder.Append(key);
            scriptBuilder.Append("');})");
            scriptBuilder.Append(".resolve();");
            return scriptBuilder.ToString();
        }

        private void DecremementTotalScripts()
        {
            if (_totalScripts > 0)
            {
                _totalScripts--;
            }
        }

        private void EndProcessNotification()
        {
            //dialog itself will handle saving of debug info.
            StatusUpdater.UpdateStatus("Completed");
            this._notified = true;
        }

        public void CheckAllCallbacks(Object source, ElapsedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string message;
            bool haveActive = false;
            Debug.Print("Starting in checkAllCallbacks");
            if (Callbacks == null || Callbacks.Count == 0)
            {
                message = "callbacks null or empty";
                Debug.Print(message);
                _timer.Stop();
                return;
            }
            message = "callback total: " + Callbacks.Count;
            List<string> callbackKeysToRemove = new List<string>();
            //'go through individual callbacks
            foreach (string cbKey in this.Callbacks.Keys)
            {
                Callback cb = Callbacks[cbKey];
                if (cb.hasStarted())
                {
                    if (cb is BatchCallback)
                    {
                        BatchCallback batchCb = cb as BatchCallback;
                        //'look for started items 
                        if (batchCb.ContainsActiveCallback())
                        {
                            haveActive = true;
                            break;
                        }
                        else
                        {
                            callbackKeysToRemove.Add(cbKey);
                        }
                    }
                }
            }

            callbackKeysToRemove.ForEach(k =>
            {
                this.Callbacks.Remove(k);
                Debug.Print("just removed batchcallback with key " + k);
            });
            KeepCheckingCallbacks = haveActive;
            if (!haveActive)
            {
                message = "No active callbacks detected";
            }

            Debug.Print(message);

            if (!KeepCheckingCallbacks && ((ScriptQueue == null) || ScriptQueue.Count == 0))
            {
                _timer.Stop();
                _timer.Close();
                _timer = null;
                Debug.Print("_timer closed");
                if (!haveActive)
                {
                    Debug.Print("about to clear callbacks");
                    this.Callbacks.Clear();
                }

            }
            else if (ScriptQueue != null && ScriptQueue.Count > 0)
            {
                LaunchLastScript();
            }
            Debug.Print("end of checkAllCallbacks which took " + sw.Elapsed);
            sw.Stop();
        }
        public void LaunchCheckJob()
        {
            double secondsToMilliseconds = 1000;
            string interval = "00:00:" + String.Format("{0:00}", _checkInterval);
            //'assume interval is less than 60 seconds!

            Debug.Print("LaunchCheckJob using interval " + interval);
            _timer = new Timer(_checkInterval * secondsToMilliseconds);
            _timer.AutoReset = true;
            _timer.Elapsed += CheckAllCallbacks;
            Debug.Print("(checkAllCallbacks)");
            _timer.Start();
        }

        public void ContinueSetup()
        {

        }
    }
}