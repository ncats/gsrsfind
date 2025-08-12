function buildMeta() {
    var meta = document.createElement('meta')
    meta.setAttribute('http-equiv', 'X-UA-Compatible');
    meta.setAttribute('content', "IE=Edge");
    meta.outerHTML = '<meta http-equiv="X-UA-Compatible" content="IE=Edge">';
    document.getElementsByTagName('head')[0].appendChild(meta);
}
/*buildMeta();*/

var madeReady = false;
$(document).ready(function () {
    handleReady();
});

window.idsForResize = [];

function handleParameterizedTemplate(fetcher, template) {
    console.log('handling fetcher with parms');
    var parameterTag = _.find(fetcher.tags, function (t) { return _.startsWith(t, 'parameters:') });
    var pos = parameterTag.indexOf(":");
    if (pos > -1) {
        /*expected format: parm1=value1,value2,value3;parm2=value1.....*/
        try {
            var paramOptions = getParmsFromString(fetcher.id, parameterTag.split(':')[1], window.idsForResize);
            var additional = createCommentsCheckbox(fetcher.id);
            paramOptions = paramOptions.concat(additional);
            return template.replace('$PARAMETERS$', paramOptions.join(' '));
        }
        catch (ex) {
            console.log('Error: ' + JSON.stringify(ex));
        }
    }
}

function getPrettyName(initialName) {
    if (!initialName || initialName == null || initialName.length === 0) {
        return initialName;
    }
    var result = [];
    var i = 0;
    result.push(initialName.charAt(0).toUpperCase());
    for (i = 1; i< initialName.length; i++) {
        if (isUpper(initialName.charAt(i))) {
            result.push(' ');
        }
        result.push(initialName.charAt(i));
    }
    return result.join('').trim();
}

/*based on https://www.reddit.com/r/javascript/comments/6a6lo7/how_can_i_check_if_a_character_is_lower_case_or/*/
function isUpper(testChar) {
    return (testChar === testChar.toUpperCase()) && (testChar !== testChar.toLowerCase());
}

function getParmsFromString(resolverId, parmString, idSet) {
    var parmObject = [];
    if (parmString == null || parmString.length == 0) {
        return;
    }
    var substrings = parmString.split(';');
    for (var i = 0; i < substrings.length; i++) {
        var subsubstrings = substrings[0].split('=');
        var parmName = subsubstrings[0];
        console.log('parmName: ' + parmName);
        var optionNames = subsubstrings[1].split(',');
        if ((typeof idSet) === 'undefined') {
            idSet = [];
        }
        if (parmName.length > 0 && optionNames.length > 0) {
            var select = document.createElement("SELECT");
            select.name = parmName;
            var selectId = resolverId + '_param_' + parmName.replace(/[\s\(\)]+/g, '_');
            /*console.log('creating select with ID ' + selectId);*/
            select.setAttribute('id', selectId);
            idSet.push(selectId);
            var label = document.createElement("LABEL");
            label.setAttribute("for", parmName);
            label.setAttribute("value", parmName);
            var labelText = document.createTextNode(getPrettyName(parmName));
            label.appendChild(labelText);
            var breakElement = document.createElement("BR");
            for (var o = 0; o < optionNames.length; o++) {
                var optionName = optionNames[o];
                var option = new Option(optionName, optionName);
                select.options.length = o + 1;
                select.options[o] =option;
            }
            parmObject.push(breakElement.outerHTML);
            parmObject.push(label.outerHTML);
            parmObject.push(breakElement.outerHTML);
            parmObject.push(select.outerHTML);
        }
    }
    return parmObject;
}

function createCommentsCheckbox(resolverId) {
    var checkbox = document.createElement('input');
    checkbox.setAttribute('type', 'checkbox');
    checkbox.setAttribute('class', 'parameterCheckbox');
    checkbox.id = resolverId + '_include_comments';
    var label = document.createElement("LABEL");
    label.setAttribute("for", resolverId + '_include_comments');
    var parmName = 'Include comments?';
    var labelText = document.createTextNode(parmName);
    label.appendChild(labelText);
    var parmObjects = [];
    parmObjects.push(checkbox.outerHTML);
    parmObjects.push(label.outerHTML);
    return parmObjects;
}

function handleReady() {
    
    console.log('handleReady madeReady: ' + madeReady);
    if (madeReady) return;

    /*For some reason, the template no longer comes back correctly from the DOM so we're putting it here:
    //     17 July 2019 MAM*/
    var htmltemplate = id = '<DIV class=checkop><INPUT id="$name$" name="$name$" type=checkbox><LABEL for="$name$">$name$</LABEL></DIV>';
    /*new stuff 9 Sept*/
    var htmltemplateOptions = id = '<DIV class="checkopparent"><span class=checkop><INPUT id="$name$" name="$name$" type=checkbox><LABEL for="$name$">$name$</LABEL></span><br/> <span class="checkopparam"> <input id="$name$_comments" name="$name$_comments" type="checkbox"><LABEL for="$name$_comments">include comments?</LABEL></span><OPTIONS></DIV>';
    /*var htmlTemplateParameters = '<DIV class=checkopwithopts><INPUT id="$name$" name="$name$" type=checkbox><LABEL for="$name$">$name$</LABEL>$PARAMETERS$</DIV>';*/
    var htmlTemplateParameters = '<DIV class="checkop checkopwithopts"><INPUT id="$name$" name="$name$" type=checkbox><LABEL for="$name$">$name$</LABEL>$PARAMETERS$</DIV>';
    console.log('_: ' + (typeof _));
    var fetchersSoFar = [];
    _.chain(FetcherRegistry.getFetcherTags())
        .filter(function (t) { return !FetcherRegistry.isNonOrganizingTag(t); })
        .forEach(function (tag) {
            var n = 1;
            var fetchers = FetcherRegistry.getFetchersWithTag(tag);
            var nhtml = _.chain(fetchers)
                /*.map("name")*/
                .filter(function (fetcher) { return !_.includes(fetchersSoFar, fetcher.name); })
                .map(function (fetcher) {
                    var fetcherName = fetcher.name;
                    var tags = fetcher.tags;

                    var completedTemplate = htmltemplate.replace(/\$name\$/g, fetcherName);
                    if (_.some(tags, function (t) {
                        return t != null && t.toUpperCase().indexOf("PARAMETERS:") > -1;
                    })) {
                        completedTemplate = handleParameterizedTemplate(fetcher, htmlTemplateParameters);
                        completedTemplate = completedTemplate.replace(/\$name\$/g, fetcherName);
                    }
                    else if (_.includes(tags, "optioned")) {
                        completedTemplate = htmltemplateOptions.replace(/\$name\$/g, fetcherName);
                    }
					/*else {
						if( _.some(tags, function(t) {
							console.log('evaulating tag t ' + t);
							return  t!=null && t.toUpperCase().indexOf("PARAMETERS:") > -1;
						})) {
							console.log('some -- true');
							completedTemplate =handleParameterizedTemplate(fetcher, htmlTemplateParameters);
							completedTemplate = completedTemplate.replace(/\$name\$/g, fetcherName);
						}

					}*/
                    fetchersSoFar.push(fetcherName);
                    return completedTemplate;
                })
                .value()
                .join("\n");
            $("#outputSettings").append("<div><h5>" + tag + "</h5>" + nhtml + "</div>");
        }).value();
    /*Size bounding boxes appropriately*/
    _.forEach(window.idsForResize, function (id) { $('#' + id).parent().width($('#' + id).width()+10)});
    var nhtml = _.chain(FetcherRegistry.getFetchersWithNoTag())
        .map("name")
        .map(function (n) {
            return htmltemplate.replace(/\$name\$/g, n);
        }).value().join("\n");
    $("#outputSettings").append("<div><h5>Others</h5>" + nhtml + "</div>");
    $('#FormDiv').css('margin-left', '10px');
    $('label').css({ 'font-weight': 'normal', 'margin-left': '4px' });
    $('.checkop').css('margin-left', '5px');
    $('h5').css('font-weight', 'bold');
    madeReady = true;
}

function showPreview(runner) {
    console.log('showPreview');
    $("#scriptArguments").html("");
    _.forEach(runner.getArguments(), function (a) {
        var val = a.getValue();
        if (!val) {
            val = "";
        }
        var html = $("#argTemplate").html()
            .replace(/\$arg\$/g, a.name)
            .replace(/\$value\$/g, val);
        if (a.isRequired()) {
            html = html.replace(/\$req\$/g, "*");
        } else {
            html = html.replace(/\$req\$/g, "");
        }
        $('#' + a.name + 'Value').show();
        $("#scriptArguments").append(html);
    });
    $("#scriptArguments").show();
    $(".paramArgValue").show();
    $(".paramArgValue").parent().show();
}

function showScripts() {
    $("#scriptlist").html("");
    _.chain(Scripts.all())
        .filter(function (s) { return s.validForSheetCreation; })
        .forEach(function (script) {
            $("#scriptlist").append("<option value='" + script.name + "'>" + script.name + "</option>");
        }).value();
    $("#scriptlist").change(function () {
        var script = Scripts.get($("#scriptlist").val());
        $("#scriptdetails").html(script.description);
    });
}

function setMode(m) {
    $("#outputSettings").hide();
    $("#scriptArguments").hide();
    $("#showScripts").hide();

    if (m === "resolver") {
        $("#outputSettings").show();
    } else if (m === "showScripts") {
        $("#showScripts").show();
        showScripts();
    } else { /*resolver*/
        $("#scriptArguments").show();
    }
}

function runCommandForCSharp(stuffToRun) {
    if (!stuffToRun || stuffToRun === null || stuffToRun.length === 0) {
        console.log('empty arg to runCommandForCSharp');
        return null;
    }
    var debugOutput = true;
    if (stuffToRun.indexOf("auth") >= 0) {
        debugOutput = false;
    }
    if (debugOutput) {
        console.log('runCommandForCSharp about to eval ' + stuffToRun);
    }
    try {
        /*assume C# validates */
        var result = eval(stuffToRun);
    }
    catch (ex) {
        console.log('error running script: ' + JSON.stringify(ex));
        console.log('script was: ' + stuffToRun + " at " + (new Date()));
        return 'error running script: ' + JSON.stringify(ex);
    }
    if (result) {
        if (typeof result === 'string') {
            return result;
        }
        /*console.log('result: ' + JSON.stringify(result));*/
        return JSON.stringify(result);
    }
    /*takes care of falsey returns*/
    return result;
}

function sendMessageBackToCSharp(message) {
    let msg = 'Sending message back to c sharp: ' + message
    console.log(msg);
    window.chrome.webview.postMessage(message);
}

function checkReady() {
    if (!madeReady) {
        console.log('calling handleReady')
        handleReady();
    }
    else {
        console.log('already called handleReady')
    }
}

/*setTimeout(checkReady, 500);*/

