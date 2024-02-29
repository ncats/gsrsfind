
var headElems = document.getElementsByTagName('head');
if (headElems && headElems.length > 0) {
    var headElement = headElems[0];
}
else {
    var headElement = document.createElement('head');
}

var metaElement = document.createElement('meta');
metaElement.setAttribute('http-equiv', 'content-type');
metaElement.setAttribute('content', 'text/html; charset=UTF-8');
headElement.appendChild(metaElement);

metaElement = document.createElement('meta');
metaElement.setAttribute('http-equiv', 'X-UA-Compatible');
metaElement.setAttribute('content', 'IE=Edge');
headElement.appendChild(metaElement);

var titleElement = document.createElement('title');
titleElement.innerText = 'ginas Tools';
headElement.appendChild(titleElement);

var scriptElement = document.createElement('script');
scriptElement.setAttribute('type', 'text/javascript');
scriptElement.setAttribute('src', 'https://cdnjs.cloudflare.com/ajax/libs/lodash.js/3.0.0/lodash.min.js');
headElement.appendChild(scriptElement);

var scriptElement2 = document.createElement('script');
scriptElement2.setAttribute('type', 'text/javascript');
scriptElement2.setAttribute('src', 'https://cdnjs.cloudflare.com/ajax/libs/fast-json-patch/1.0.1/json-patch.min.js');
headElement.appendChild(scriptElement2);

/*var scriptElement3 = document.createElement('script');
scriptElement3.setAttribute('type', 'text/javascript');
scriptElement3.setAttribute('src', 'https://code.jquery.com/jquery-1.12.4.min.js');
headElement.appendChild(scriptElement3);*/

var styleElement = document.createElement('style');
styleElement.setAttribute('type', 'text/css');
styleElement.innerHTML = "    body { font-family: sans-serif; } .scriptlist { padding: 10px;   width: 40%;            display: inline-block;        }            .scriptlist select {                width: 100%;            }        .scriptdetails {            padding: 10px;            width: 40%;            display: inline-block;            vertical-align: top; /* here */        }        .manual,        .advanced {            display: none;        }        .required {            color: #ff0000;        }        .template {            display: none;        }        .argument textarea {            width: 100%;        }    .argument {            display: inline-block;            width: 33%;            padding: 10px;        }        #console {            width: 100%;            height: 50px;            /*display:none;*/        }        .consolehead {            /*display: none;*/        }        .data {            width: 40%;            display: inline-block;            padding: 10px;        }        .checkop {            width: 30%;            display: inline-block;        }        .data textarea {            width: 100%;            min-height: 300px;            white-space: nowrap;        }        .buttons {            width: 8%;            display: inline-block;}";
headElement.appendChild(styleElement);

scriptElement = document.createElement('script');
scriptElement.setAttribute('type', 'text/javascript');
scriptElement.innerHTML = 'if (!Array.prototype.getItem) { Array.prototype.getItem = function (i) { return this[i]; }; };Object.prototype.gGet = function (k) { return this[k]; }; Object.prototype.gKeys = function () { return _.keys(this); }; var cresults = { "getItem": function (v) { return this[v]; }, "popItem": function (v) { var ret = this[v]; delete this[v]; return ret; } }; window["console"] = {log: function (r){var currValue = document.getElementById("console").value; document.getElementById("console").value = currValue + "\\r\\n" +r;}}';
headElement.appendChild(scriptElement);

if (!headElems || headElems.length === 0) {
    document.getElementsByTagName("html")[0].appendChild(headElement);
}

/*var bodyElement = document.createElement('body');

var divElement = document.createElement('div');
divElement.setAttribute('id', 'showScripts');
var innerDiv = document.createElement('div');
innerDiv.setAttribute('class', 'scriptlist');
var selectElement = document.createElement('select');
selectElement.setAttribute('id', 'scriptlist');
selectElement.setAttribute('size', '10');
innerDiv.appendChild(selectElement);
divElement.appendChild(innerDiv);

innerDiv = document.createElement('div');
innerDiv.setAttribute('class', 'scriptdetails');
innerDiv.setAttribute('id', 'scriptdetails');
divElement.appendChild(innerDiv);
bodyElement.appendChild(divElement);

divElement = document.createElement('div');
var formElement = document.createElement('form');
formElement.setAttribute('id', 'scriptArguments');
divElement.appendChild(formElement);
bodyElement.appendChild(divElement);

divElement = document.createElement('div');
divElement.setAttribute('id', 'argTemplate');
divElement.setAttribute('class', 'template');

innerDiv = document.createElement('div');
innerDiv.setAttribute('class', 'argument');

var inner2Div = document.createElement('div');
var labelElement = document.createElement('label');
labelElement.setAttribute('for', '$arg$');
labelElement.innerText = '$arg$';
var spanElement = document.createElement('span');
spanElement.setAttribute('class', 'required');
spanElement.setAttribute('title', 'required');
spanElement.innerText = '$req$';
inner2Div.appendChild(labelElement);
inner2Div.appendChild(spanElement);
innerDiv.appendChild(inner2Div);

inner2Div = document.createElement('div');
var textAreaElement = document.createElement('textarea');
textAreaElement.setAttribute('id', '$arg$');
textAreaElement.setAttribute('name', '$arg$');
textAreaElement.innerText = '$value$';
inner2Div.appendChild(textAreaElement);
innerDiv.appendChild(inner2Div);
divElement.appendChild(innerDiv);

divElement = document.createElement('div');
divElement.setAttribute('id', 'fetcherTemplate');
divElement.setAttribute('class', 'template');
innerDiv = document.createElement('div');
innerDiv.setAttribute('class', 'checkop');
var inputElement = document.createElement('input');
inputElement.setAttribute('type', 'checkbox');
inputElement.setAttribute('name', '$name$');
inputElement.setAttribute('id', '$name$');
labelElement = document.createElement('label');
labelElement.setAttribute('for', '$name$');
labelElement.innerText = '$name$';
innerDiv.appendChild(inputElement);
innerDiv.appendChild(labelElement);

divElement.appendChild(innerDiv);
bodyElement.appendChild(divElement);

var brElement = document.createElement('br');
bodyElement.appendChild(brElement);
brElement = document.createElement('br');
bodyElement.appendChild(brElement);

var formElement = document.createElement('form');
var h3Element = document.createElement('h3');
h3Element.setAttribute('class', 'consolehead');
h3Element.setAttribute('id', 'consoleHeadWebOutput');
h3Element.innerText = 'Web output:';
formElement.appendChild(h3Element);

brElement = document.createElement('br');
formElement.appendChild(brElement);
textAreaElement = document.createElement('textarea');
textAreaElement.setAttribute('id', 'console');
formElement.appendChild(textAreaElement);

brElement = document.createElement('br');
formElement.appendChild(brElement);

spanElement = document.createElement('span');
spanElement.innerText = 'Command:';
formElement.appendChild(spanElement);

var textInputElement = document.createElement('input');
textInputElement.setAttribute('type', 'text');
textInputElement.setAttribute('name', 'commandInput');
textInputElement.setAttribute('id', 'commandInput');
textInputElement.setAttribute('placeholder', '[command to run]');
textInputElement.setAttribute('size', '50');
formElement.appendChild(textInputElement);

inputElement = document.createElement('input');
inputElement.setAttribute('type', 'button');
inputElement.setAttribute('value', 'Run');
inputElement.setAttribute('onclick', "runCommandForCSharp(document.getElementById('commandInput').value)");
formElement.appendChild(inputElement);

bodyElement.appendChild(formElement);

document.getElementsByTagName("html")[0].appendChild(bodyElement);*/