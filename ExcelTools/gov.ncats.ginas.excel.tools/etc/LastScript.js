function buildMeta() {
    var meta = document.createElement('meta')
    meta.setAttribute('http-equiv', 'X-UA-Compatible');
    meta.setAttribute('content', "IE=Edge");
    meta.outerHTML = '<meta http-equiv="X-UA-Compatible" content="IE=Edge">';
    document.getElementsByTagName('head')[0].appendChild(meta);
}
//buildMeta();

var madeReady = false;
$(document).ready(function () {
    handleReady();
});

function handleReady() {
    
    console.log('handleReady madeReady: ' + madeReady);
    if (madeReady) return;

    //For some reason, the template no longer comes back correctly from the DOM so we're putting it here:
    //     17 July 2019 MAM
    var htmltemplate = id = '<DIV class=checkop><INPUT id="$name$" name="$name$" type=checkbox><LABEL for="$name$">$name$</LABEL></DIV>';
        //$("#fetcherTemplate").html();
    
    _.chain(FetcherRegistry.getFetcherTags())
        .filter(function (t) { return t !== "Tests"; })
        .forEach(function (tag) {
            var n = 1;
            var fetchers = FetcherRegistry.getFetchersWithTag(tag);
            var nhtml = _.chain(fetchers)
                .map("name")
                .map(function (n) {
                    var completedTemplate = htmltemplate.replace(/\$name\$/g, n);
                    return completedTemplate;
                })
                .value()
                .join("\n");
            $("#outputSettings").append("<div><h5>" + tag + "</h5>" + nhtml + "</div>");
        }).value();

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
    /*if (stuffToRun.indexOf("auth") >= 0) {
        debugOutput = false;
    }*/
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
    console.log('Sending message back to c sharp: ' + message);
    window.external.Notify(message);
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

//setTimeout(checkReady, 500);

