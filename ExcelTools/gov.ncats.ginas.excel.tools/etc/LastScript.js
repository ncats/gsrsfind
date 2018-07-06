var GSRSAPI_consoleStack = [];

var madeReady = false;
$(document).ready(function () {
    handleReady();    
});

function handleReady() {
    var htmltemplate = $("#fetcherTemplate").html();
    var test = $("#fetcherTemplate");

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
    madeReady = true;
}

function showPreview(runner) {
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
    _.forEach(Scripts.all(), function (script) {
        $("#scriptlist").append("<option value='" + script.name + "'>" + script.name + "</option>");
    });
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
    var result = eval(stuffToRun);
    if (result) {
        if (typeof result === 'string') {
            if (debugOutput) {
                console.log('result was string: ' + result);
            }
            return result;
        }
        console.log('result: ' + JSON.stringify(result));
        return JSON.stringify(result);
    }
    else return result;
}

function checkReady() {
    if (!madeReady) {
        handleReady();
    }
}

setTimeout(checkReady(), 3000);