var GSRSAPI_consoleStack = [];

var console = {
    log: function (msg) {
        //if (window['console']) window.console.log(msg);
        GSRSAPI_consoleStack.push(msg);
    }
}
$.support.cors = true;
var CALLBACK_NUMBER = 0;
var GSRSAPI = {
    builder: function () {
        var g_api = {};
        g_api.GlobalSettings = {
            _url: "https://ginas.ncats.nih.gov/ginas/app/api/v1/",
            _status: "OK", /*set to ERROR when there's a problem*/
            _errorMessage: "",
            getBaseURL: function () {
                return g_api.GlobalSettings._url;
            },
            setBaseURL: function (url) {
                g_api.GlobalSettings._url = url;
                return g_api.GlobalSettings;
            },
            getHomeURL: function () {
                return g_api.GlobalSettings.getBaseURL().replace(/api.v1.*/g, "");
            },
            httpType: function () {
                /*return "jsonp"; get only*/
                return "json"; /*CORS needed, updates possible */
            },
            authToken: null,
            authUsername: null,
            authKey: null,
            authenticate: function (req) {
                req.headers = {};
                /*figure out authentication*/
                if (g_api.GlobalSettings.authUsername
                    && g_api.GlobalSettings.authUsername.length > 0
                    && g_api.GlobalSettings.authKey
                    && g_api.GlobalSettings.authKey.length > 0) {
                    req.headers["auth-username"] = g_api.GlobalSettings.authUsername;
                    req.headers["auth-key"] = g_api.GlobalSettings.authKey;
                    console.log("using name/key authentication");
                }
                else if (g_api.GlobalSettings.authToken
                    && g_api.GlobalSettings.authToken.length > 0) {
                    req.headers["auth-token"] = g_api.GlobalSettings.authToken;
                    console.log("using token authentication");
                }
                else {
                    console.log("no authentication configured");
                }
            },
            getStatus: function () {
                return GlobalSettings._status;
            },
            setStatus: function (newStatus) {
                GlobalSettings._status = newStatus;
                console.log('Setting status to ' + newStatus);
            },
            getErrorMessage: function () {
                return GlobalSettings._errorMessage;
            }
        },
            g_api.isJson = function (str) {
                try {
                    JSON.parse(str);
                }
                catch (e) {
                    console.log('error in isJson: ' + e);
                    return false;
                }
                return true;
            },

            /*TODO: should be its own service*/
            g_api.httpProcess = function (req) {
                return g_api.JPromise.of(function (cb) {
                    var b = req._b;
                    var contentType = 'application/json';

                    console.log('in httpProcess, req.skipJson: ' + req.skipJson);
                    if (b && !req.skipJson) {
                        b = JSON.stringify(b);
                    } else {
                        b = b ? b : req._q;
                        contentType = 'text/plain';
                    }
                    if (req._url.match(/.*[?]/)) {
                        req._url = req._url + "&cache=" + g_api.UUID.randomUUID();
                    } else {
                        req._url = req._url + "?cache=" + g_api.UUID.randomUUID();
                    }

                    g_api.GlobalSettings.authenticate(req);

                    console.log("(g_api.httpProcess) going to call url: " + req._url);
                    if (req._q && req._q.q) {
                        console.log("   with query: " + req._q.q);
                    }
                    var cbackname = 'jsoncallback' + ++CALLBACK_NUMBER;
                    window[cbackname] = function (response) {
                        console.log('ajax call success (1)');
                        console.log(' at ' + _.now());
                        cb(response);
                    };
                    console.log('b: ' + JSON.stringify(b));
                    $.ajax({
                        url: req._url,
                        /*jsonp: cbackname,*/
                        dataType: GlobalSettings.httpType(),
                        contentType: contentType,
                        type: req._method,
                        data: b,
                        beforeSend: function (request) {
                            if (req.headers) {
                                _.forEach(_.keys(req.headers), function (k) {
                                    request.setRequestHeader(k, req.headers[k]);
                                });
                            }
                        },
                        success: function (response) {
                            console.log('ajax call success ');
                            console.log('	at ' + _.now());
                            /*console.log('	with response ' + (typeof (response) == 'string') ? response
                                : JSON.stringify(response));*/
                            cb(response);
                        },
                        error: function (response, error, t) {
                            console.log('ajax call error ');
                            var msg = 'Error from server. response: '
                                + JSON.stringify(response) + '; url: '
                                + this.url;
                            console.log(msg);
                            if ((response.status >= 400 && response.status <= 600) || response.status === 0) {
                                if (response.status === 500 && response.responseText === "java.lang.reflect.InvocationTargetException"
                                    && response.readyState === 4) {
                                    /*not necessarily an error.
                                     This message occurs when we attempt to retrieve a section from a substance 
                                     that does not have that type of section (e.g., a protein does not have a molfile)*/
                                    console.log('500 error');
                                } else {
                                    GlobalSettings.setStatus("ERROR " + response.status);
                                }
                            }
                            GlobalSettings._errorMessage = error;
                            /*figure out the message that will be displayed to the user in Excel*/
                            if (response.responseText) {
                                console.log('Noting error. ');
                                GlobalSettings._errorMessage = response.responseText;
                                console.log('	just set errorMessage');
                                var retMsg = { valid: false };
                                console.log('	initialized retMsg');
                                /*detect a complex, nested error message*/
                                if (typeof response.responseText === 'string' && g_api.isJson(response.responseText)) {
                                    var responseRestored = JSON.parse(response.responseText);
                                    console.log(' parsed JSON');
                                    if (responseRestored.validationMessages && responseRestored.validationMessages.length > 0) {
                                        console.log('	error msg: ' + responseRestored.validationMessages[0].message);
                                        retMsg.message = responseRestored.validationMessages[0].message;
                                        GlobalSettings._errorMessage = responseRestored.validationMessages[0].message;
                                        if (responseRestored.validationMessages.length > 1) {
                                            retMsg.message = retMsg.message + ' + more';
                                            GlobalSettings._errorMessage = GlobalSettings._errorMessage + '...';
                                        }
                                    }
                                    else if (responseRestored.message) {
                                        retMsg.message = responseRestored.message;
                                        GlobalSettings._errorMessage = responseRestored.message;
                                    }
                                    else retMsg.message = "unparsed error";
                                }
                                else if (typeof response.responseText === 'object' && response.responseText.message) {
                                    console.log(' object');
                                    retMsg.message = response.responseText.message;
                                    if (response.status === 502) {
                                        console.log('502; proxy error');
                                        retMsg.message = 'proxy error on server. Please report this to your administrator!';
                                    }
                                }
                                else {
                                    console.log(' simple message. response.status: ' + response.status);
                                    /*simple message*/
                                    retMsg.message = response.responseText;
                                    if (response.status === 502) {
                                        console.log('502; proxy error');
                                        retMsg.message = 'proxy error on server. Please report this to your administrator!';
                                    }
                                };
                                console.log('Calling cb with retMsg. cb: ' + JSON.stringify(cb));

                                if (retMsg) {
                                    cb(retMsg);
                                }
                                else {
                                    cb("[no data]");
                                }

                            }
                            else if (response.statusText) {
                                console.log('statusText: ' + response.statusText);
                            }
                            else {
                                console.log('Error missing');
                                cb(response);
                            };
                        }
                    });
                });
            };
        /*Returns an object which will call
        the supplied callback after {{total}}
        number of calls to {{decrement}}*/
        g_api.getListener = function (total, cb) {
            return {
                total: total,
                current: 0,
                callback: cb,
                decrement: function () {
                    this.current++;
                    if (this.current >= this.total) {
                        this.callback();
                    };
                }
            };
        };
        g_api.JPromise = {
            ofScalar: function (s) {
                return g_api.JPromise.of(function (cb) {
                    cb(s);
                });
            },
            of: function (calc) {
                var ret = {
                    get: function (cb) {
                        calc(cb);
                    },
                    andThen: function (lam) {
                        return g_api.JPromise.of(function (cb) {
                            ret.get(function (orig) {
                                var ret = lam(orig);
                                if (ret && ret._promise) {
                                    ret.get(cb);
                                } else if (typeof ret === "undefined") {
                                    cb(orig);
                                } else {
                                    cb(ret);
                                };
                            });
                        });
                    },
                    _promise: true
                };
                return ret;
            },
            join: function (listo) {
                var list = [];
                if (arguments.length > 1) {
                    list = arguments;
                } else {
                    list = listo;
                }
                return g_api.JPromise.of(function (cb) {
                    var toRet = {};
                    var retFun = function () {
                        var retList = [];
                        for (var j = 0; j < list.length; j++) {
                            retList.push(toRet[j]);
                        }
                        return retList;
                    };
                    var listener = g_api.getListener(list.length, function () {
                        cb(retFun());
                    });
                    var proc = function (pFetch, key) {
                        pFetch.get(function (ret) {
                            toRet[key] = ret;
                            listener.decrement();
                        });
                    };
                    for (var i = 0; i < list.length; i++) {
                        var pFetch = list[i];
                        proc(pFetch, i);
                    };
                });
            }
        };
        g_api.gUtil = {
            empty: {},

            deepIterate: function (o, path, cb) {
                if (_.isFunction(o)) {
                    return g_api.gUtil.empty;
                } else if (_.isObject(o)) {
                    if (_.isArray(o)) {
                        var ks = _.keys(o);
                        _.forEach(ks, function (k) {
                            g_api.gUtil.deepIterate(o[k], path + "[" + k + "]", cb);
                        });
                    } else {
                        var ks2 = _.keys(o);
                        _.forEach(ks2, function (k) {
                            g_api.gUtil.deepIterate(o[k], path + "/" + k, cb);
                        });
                    }
                } else {
                    cb(path, o);
                };
            },
            forEachDeep: function (o, path, cb) {
                var node = function (path, key, value, parent) {
                    return {
                        path: path,
                        key: key,
                        value: value,
                        parent: parent
                    };
                };
                if (_.isFunction(o)) {
                    return g_api.gUtil.empty;
                } else if (_.isObject(o)) {
                    if (_.isArray(o)) {
                        var ks = _.keys(o);
                        var mod = false;
                        _.forEach(ks, function (k) {
                            var rep = cb(node(path, k, o[k], o));
                            if (rep === g_api.gUtil.empty) {
                                o[k] = g_api.gUtil.empty;
                                mod = true;
                            } else {
                                if (typeof rep !== "undefined") {
                                    o[k] = rep;
                                }
                                g_api.gUtil.forEachDeep(o[k], path + "/" + k, cb);
                            };
                        });
                        if (mod) {
                            var newArray = _.filter(o, function (e) {
                                if (e === g_api.gUtil.empty)
                                    return false;
                                return true
                            });
                            o.splice(0, o.length);
                            _.forEach(newArray, function (a) {
                                o.push(a);
                            });
                        };
                    } else {
                        var ks2 = _.keys(o);
                        _.forEach(ks2, function (k) {
                            var rep = cb(node(path, k, o[k], o));
                            if (rep === gUtil.empty) {
                                delete o[k];
                            } else {
                                if (typeof rep !== "undefined") {
                                    o[k] = rep;
                                }
                                g_api.gUtil.forEachDeep(o[k], path + "/" + k, cb);
                            }
                        });
                    }
                }
            },
            removeDeep: function (o, test) {
                g_api.gUtil.forEachDeep(o, "", function (node) {
                    if (test(node)) {
                        return gUtil.empty;
                    };
                });
            },
            removeKeysLike: function (o, regex) {
                g_api.gUtil.removeDeep(o, function (node) {
                    return node.key.match(regex);
                });
            },
            toDate: function (d) {
                return new Date(d);
            }
        };
        g_api.ResourceFinder = {
            builder: function () {
                var finder = {};
                finder.resource = function (resource) {
                    finder.resource = resource;
                    return finder;
                };
                finder.searcher = function () {
                    return g_api.SearchRequest.builder()
                        .resource(finder.resource);
                };
                finder.search = function (q) {
                    return finder.searcher()
                        .query(q)
                        .execute();
                };
                finder.get = function (uuid) {
                    var url = g_api.GlobalSettings.getBaseURL() + finder.resource + "(" + uuid + ")";
                    var req = g_api.Request.builder()
                        .url(url);

                    return g_api.httpProcess(req).andThen(function (sim) {
                        /*TODO: make generic*/
                        return g_api.SubstanceBuilder.fromSimple(sim);
                    });
                };

                finder.extend = function (f) {
                    var nfinder = f(finder);
                    if (typeof nfinder !== "undefined") {
                        return nfinder;
                    } else {
                        return finder;
                    }
                };
                return finder;
            }
        };
        g_api.SubstanceFinder = g_api.ResourceFinder.builder()
            .resource("substances")
            .extend(function (sfinder) {
                sfinder.searchByExactNameOrCode = function (q) {
                    if (UUID.isUUID(q)) {
                        return sfinder.get(q).andThen(function (s) {
                            return { "content": [s] };
                        });
                    }
                    return sfinder.search("root_names_name:\"^" + q + "$\" OR " +
                        "root_approvalID:\"^" + q + "$\" OR " +
                        "root_codes_code:\"^" + q + "$\"");
                };
                sfinder.getExactStructureMatches = function (smi) {
                    /*substances/structureSearch?q=CCOC(N)=O&type=exact*/
                    var req = g_api.Request.builder()
                        .url(g_api.GlobalSettings.getBaseURL() + "substances/structureSearch")
                        .queryStringData({
                            q: smi,
                            type: "exact",
                            sync: "true" /*shouldn't be sync*/
                        });
                    return g_api.httpProcess(req).andThen(function (tmp) {
                        return tmp;
                    });
                };
                sfinder.saveTemporaryStructure = function (smi) {
                    var url = g_api.GlobalSettings.getBaseURL();
                    var pos = url.indexOf("api");
                    url = url.substring(0, pos) + "structure";
                    var req = g_api.Request.builder()
                        .url(url)
                        .method("POST")
                        .setSkipJson(true)
                        .body(smi)
                        .setContents({ "body": smi });
                    return g_api.httpProcess(req)
                        .andThen(function (tmp) {
                            console.log('saveTemporaryStructure tmp:' + JSON.stringify(tmp));
                            return tmp;
                        });
                };

                sfinder.searchByExactName = function (q) {
                    return sfinder.search("root_names_name:\"^" + q + "$\"");
                };
            });
        g_api.ReferenceFinder = g_api.ResourceFinder.builder()
            .resource("references")
            .extend(function (rfinder) {
                rfinder.searchByLastEdited = function (q) {
                    return rfinder.search("root_lastEditedBy:\"^" + q + "$\"");
                };
            });

        g_api.CVFinder = g_api.ResourceFinder.builder()
            .resource("vocabularies")
            .extend(function (cvfinder) {
                cvfinder.searchByDomain = function (q) {
                    console.log("going to run cvfinder: " + "root_domain:\"^" + q + "$\"");
                    return cvfinder.search("root_domain:\"^" + q + "$\"");
                };
            });
        g_api.SearchRequest = {
            builder: function () {
                var request = {
                    _limit: 10,
                    _skip: 0,
                    _resource: "resource",
                    _query: ""
                };
                request.limit = function (limit) {
                    request._limit = limit;
                    return request;
                };
                request.skip = function (skip) {
                    request._skip = skip;
                    return request;
                };
                request.top = function (top) {
                    return request.limit(top);
                };
                request.resource = function (resource) {
                    request._resource = resource;
                    return request;
                };
                request.query = function (q) {
                    request._query = q;
                    return request;
                };
                request.asRequest = function () {
                    return g_api.Request.builder()
                        .url(g_api.GlobalSettings.getBaseURL() + request._resource + "/search")
                        .queryStringData({
                            q: request._query,
                            top: request._limit,
                            skip: request._skip
                        });
                };
                request.execute = function () {
                    var httpreq = request.asRequest();
                    return g_api.httpProcess(httpreq);
                };
                return request;
            }
        };


        /*TODO*/
        g_api.SearchResponse = {
            builder: function () {
                var resp = {};
                resp.mix = function (raw) {
                    _.merge(resp, raw);
                    return resp;
                };
                return resp;
            }
        };

        g_api.SubstanceBuilder = {
            fromSimple: function (simple) {
                simple._cache = {};
                simple.getBestID = function () {
                    if (simple._approvalIDDisplay) {
                        return simple._approvalIDDisplay;
                    } else {
                        return simple.uuid;
                    }
                };
                simple.full = function () {
                    /*if this is a new record, return self*/
                    if (!simple.uuid) {
                        return g_api.JPromise.ofScalar(simple);
                    };
                    var req = Request.builder()
                        .url(g_api.GlobalSettings.getBaseURL() + "substances(" + simple.uuid + ")")
                        .queryStringData({
                            view: "full"
                        });
                    return g_api.httpProcess(req);
                };
                simple.fetch = function (field, lambda) {
                    var ret = simple._cache[field];
                    var p = null;
                    if (!ret) {
                        var url = g_api.GlobalSettings.getBaseURL() + "substances(" + simple.uuid + ")/";
                        if (field) {
                            url += field;
                        }
                        var req = g_api.Request.builder()
                            .url(url);
                        p = g_api.httpProcess(req);
                    } else {
                        p = g_api.JPromise.ofScalar(ret);
                    }
                    if (lambda) {
                        return p.andThen(lambda);
                    }
                    return p;
                };
                simple.patch = function () {
                    var p = Patch.builder();

                    if (!simple.uuid) {
                        p = p.setMethod("POST");
                    }

                    /*patch overrides but calls the base method*/
                    p._oldApply = p.apply;
                    p._oldCompute = p.compute;
                    p._oldValidate = p.validate;
                    p.apply = function () {
                        return p._oldApply(simple);
                    };
                    p.compute = function () {
                        return p._oldCompute(simple);
                    };
                    p.validate = function () {
                        return p._oldValidate(simple);
                    };
                    return p;
                };
                return simple;
            }
        };

        g_api.Patch = {
            builder: function () {
                var b = {
                    ops: []
                };

                b.change = function (op) {
                    b.ops.push(op);
                    return b;
                };

                b.replace = function (path, n) {
                    return b.change({
                        op: "replace",
                        path: path,
                        value: n
                    });
                };

                b.remove = function (path) {
                    return b.change({
                        op: "remove",
                        path: path
                    });
                };

                b._method = "PUT";

                /*change the method type*/
                b.setMethod = function (meth) {
                    b._method = meth;
                    return b;
                };

                /*Method below is a shot in the dark. TODO: verify!*/
                /*Note: method not in use as of 18 January 2019*/
                b.update = function (path) {
                    console.log('b.update!');
                    return b.change({
                        op: "update",
                        path: path
                    });
                };
                b.add = function (path, n) {
                    return b.change({
                        op: "add",
                        path: path,
                        value: n
                    });
                };
                b.addData = function (data) {
                    return data.addToPatch(b);
                };
                /*should return a promise*/
                b.apply = function (simpleSub) {
                    return simpleSub.full()
                        .andThen(function (ret) {
                            var rr = ret;
                            jsonpatch.apply(rr, b.ops);
                            var methodText = (b._method) ? b._method : "PUT";
                            console.log('methodText: ' + methodText);
                            var req = g_api.Request.builder()
                                .url(g_api.GlobalSettings.getBaseURL() + "substances")
                                .method(methodText)
                                .body(rr);
                            return g_api.httpProcess(req)
                                /*new lines 30 June 2017*/
                                .andThen(function (r) {
                                    if (r === "") {
                                        return { valid: false, message: "Unexpected error from server" };
                                    } else {
                                        return r;
                                    }
                                });
                        });
                };
                /*Calculates the new record, does not submit it*/
                b.compute = function (simpleSub) {
                    return simpleSub.full()
                        .andThen(function (ret) {
                            var rr = ret;
                            jsonpatch.apply(rr, b.ops);
                            return rr;
                        });
                };
                /*Calculates the new record, does not submit it*/
                b.validate = function (simpleSub) {
                    return simpleSub.full()
                        .andThen(function (ret) {
                            var rr = ret;
                            jsonpatch.apply(rr, b.ops);
                            var req = g_api.Request.builder()
                                .url(g_api.GlobalSettings.getBaseURL() + "substances/@validate")
                                .method("POST")
                                .body(rr);
                            return g_api.httpProcess(req);
                        });
                };
                return b;
            }
        };

        g_api.ResolveWorker = {
            builder: function () {
                var worker = {
                    _list: [],
                    _fetchers: [],
                    _consumer: function (r) { },
                    _finisher: function () { },
                    consumer: function (c) {
                        worker._consumer = c;
                        return worker;
                    },
                    list: function (l) {
                        worker._list = l;
                        return worker;
                    },
                    fetchers: function (f) {
                        worker._fetchers = f;
                        return worker;
                    },
                    finisher: function (f) {
                        worker._finisher = f;
                        return worker;
                    },
                    resolve: function () {
                        var psubs = _.chain(worker._list)
                            .filter(function (r) {
                                return (r.length > 0);
                            })
                            .map(function (r) {
                                var pSub = g_api.SubstanceFinder.searchByExactNameOrCode(r);
                                pSub._q = r;
                                return pSub;
                            })
                            .value();

                        var listener = getListener(psubs.length, function () {
                            worker._finisher();
                        });

                        _.forEach(psubs, function (pSub) {

                            worker.process(pSub, worker._fetchers).get(function (rows) {
                                _.forEach(rows, function (row) {
                                    worker._consumer(row);
                                });
                                listener.decrement();
                            });
                        });
                    },
                    process: function (pSub, fetchNames) {
                        var row = pSub._q;
                        return pSub.andThen(function (ret) {
                            return ret["content"];
                        })
                            .andThen(function (content) {
                                if (content && content.length > 0) {
                                    var promises = _.map(content, function (c) {
                                        return worker.outputAll(g_api.SubstanceBuilder.fromSimple(c), fetchNames);
                                    });
                                    return g_api.JPromise.join(promises).andThen(function (all) {
                                        return _.map(all, function (q) {
                                            return row + "\t" + q;
                                        });
                                    });
                                } else {
                                    return g_api.JPromise.ofScalar([row]);
                                }
                            });
                    },
                    outputAll: function (simpleSub, fetchNames) {
                        return g_api.JPromise.of(function (cb) {
                            g_api.FetcherRegistry.getFetchers(fetchNames)
                                .fetcher(simpleSub)
                                .get(function (g) {
                                    cb(g.join("\t"));
                                });
                        });
                    }
                };
                return worker;
            }
        };
        /*TODO: convert to builder pattern*/
        g_api.FetcherMaker = {
            make: function (name, maker) {
                var fetcher = {
                    name: name,
                    tags: [],
                    fetcher: function (simp) {
                        return g_api.JPromise.of(function (cb) {
                            maker(simp).get(function (ret) {
                                cb(ret, name);
                            });
                        });
                    },
                    andThen: function (after) {
                        return g_api.FetcherMaker.make(name, function (simp) {
                            return fetcher.fetcher(simp).andThen(after);
                        });
                    }
                };
                fetcher.addTag = function (tag) {
                    fetcher.tags.push(tag);
                    return fetcher;
                };
                fetcher.setDescription = function (desc) {
                    fetcher.description = desc;
                    return fetcher;
                };
                return fetcher;
            },
            makeAPIFetcher: function (property, name) {
                var nm = name;
                if (!nm) {
                    nm = property;
                }
                return g_api.FetcherMaker.make(nm, function (simpleSub) {
                    return simpleSub.fetch(property);
                });
            },
            makeScalarFetcher: function (property, name) {
                var nm = name;
                if (!nm) {
                    nm = property;
                }
                return g_api.FetcherMaker.make(nm, function (simpleSub) {
                    return g_api.JPromise.ofScalar(simpleSub[property]);
                });
            },
            makeCodeFetcher: function (codeSystem, name) {
                var nm = name;
                if (!nm) {
                    nm = codeSystem + "[CODE]";
                }
                return g_api.FetcherMaker.make(nm, function (simpleSub) {
                    return simpleSub.fetch("codes(codeSystem:" + codeSystem + ")")
                        .andThen(function (cds) {
                            console.log('cds: ' + JSON.stringify(cds));
                            return _.chain(cds)
                                .sort(function (a, b) {
                                    if (a.type === "PRIMARY" && b.type !== "PRIMARY") {
                                        return -1;
                                    } else if (a.type !== "PRIMARY" && b.type === "PRIMARY") {
                                        return 1;
                                    } else {
                                        return 0;
                                    }
                                })
                                .map(function (cd) {
                                    if (cd.type !== "PRIMARY") {
                                        return cd.code + " [" + cd.type + "]";
                                    } else {
                                        return cd.code;
                                    }
                                })
                                .value()
                                .join("; ");
                        });
                });
            }
        };

        g_api.FetcherRegistry = {
            fetchMap: {},
            getFetcher: function (name) {
                var ret = g_api.FetcherRegistry.fetchMap[name];
                return ret;
            },
            addFetcher: function (fetcher) {
                g_api.FetcherRegistry.fetchMap[fetcher.name] = fetcher;
                g_api.FetcherRegistry.fetchers.push(fetcher);
                return g_api.FetcherRegistry;
            },
            fetchers: [],
            /*Actually accumulates into a master fetcher */
            getFetchers: function (list) {
                var retlist = _.map(list, function (f) {
                    return g_api.FetcherRegistry.getFetcher(f);
                });
                return g_api.FetcherRegistry.joinFetchers(retlist);
            },
            joinFetchers: function (retlist) {
                return g_api.FetcherMaker.make("Custom", function (simpleSub) {
                    var proms = _.map(retlist, function (r) {
                        return r.fetcher(simpleSub);
                    });
                    var promNames = _.map(retlist, function (r) {
                        return r.name;
                    });

                    return g_api.JPromise.of(function (callback) {
                        g_api.JPromise.join(proms)
                            .get(function (array) {
                                callback(array, promNames);
                            });
                    });
                });
            },
            getFetcherTags: function () {
                var allTags = [];
                _.chain(g_api.FetcherRegistry.fetchers)
                    .map(function (f) {
                        return f.tags;
                    })
                    .forEach(function (tgs) {
                        _.forEach(tgs, function (t) {
                            allTags.push(t);
                        });
                    }).value();
                return _.uniq(allTags);
            },
            getFetchersWithTag: function (tag) {
                return _.chain(g_api.FetcherRegistry.fetchers)
                    .filter(function (f) {
                        return _.indexOf(f.tags, tag) >= 0;
                    })
                    .value();
            },
            getFetchersWithNoTag: function () {
                return _.chain(g_api.FetcherRegistry.fetchers)
                    .filter(function (f) {
                        return f.tags.length === 0;
                    })
                    .value();
            }
        };

        var UUID = {
            randomUUID: function () {
                return UUID.s4() + UUID.s4() + '-' + UUID.s4() + '-' + UUID.s4() + '-' +
                    UUID.s4() + '-' + UUID.s4() + UUID.s4() + UUID.s4();
            },
            s4: function () {
                return Math.floor((1 + Math.random()) * 0x10000)
                    .toString(16)
                    .substring(1);
            },
            isUUID: function (uuid) {
                if ((uuid + "").match(/^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$/)) {
                    return true;
                }
                return false;
            }
        };
        g_api.UUID = UUID;
        g_api.Request = {
            builder: function () {
                var rq = {
                    _method: "GET",
                    skipJson: false
                };
                rq.url = function (url) {
                    rq._url = url;
                    return rq;
                },
                    rq.method = function (method) {
                        rq._method = method;
                        return rq;
                    },
                    rq.queryStringData = function (q) {
                        rq._q = q;
                        return rq;
                    },
                    rq.body = function (b) {
                        rq._b = b;
                        return rq;
                    },
                    rq.setSkipJson = function (a) {
                        rq.skipJson = a;
                        return rq;
                    },
                    rq.setContents = function (c) {
                        rq.contents = c;
                        return rq;
                    },
                    rq.setContentType = function (ct) {
                        rq.contentType = ct;
                        return rq;
                    };

                return rq;
            }
        };

        g_api.RequestProcessor = {
            SimpleProcess: function (req) {
                return g_api.JPromise.of(function (cb) {
                    var b = req._b;
                    var contentType = req.contentType;
                    console.log('in SimpleProcess, req.skipJson: ' + req.skipJson);
                    if (b && !req.skipJson) {
                        b = JSON.stringify(b);
                    } else {
                        b = b ? b : req._q;
                        contentType = 'text/plain';
                    }

                    g_api.GlobalSettings.authenticate(req);

                    console.log("(SimpleProcess going to call url: " + req._url);
                    if (req._q && req._q.q) {
                        console.log("   with query: " + req._q.q);
                    }
                    console.log('b: ' + JSON.stringify(b));
                    $.ajax({
                        url: req._url,
                        /*jsonp: cbackname,*/
                        dataType: GlobalSettings.httpType(),
                        contentType: contentType,
                        type: req._method,
                        data: b,
                        beforeSend: function (request) {
                            if (req.headers) {
                                _.forEach(_.keys(req.headers), function (k) {
                                    request.setRequestHeader(k, req.headers[k]);
                                });
                            }
                        },
                        success: function (response) {
                            console.log('ajax call success ');
                            console.log('	at ' + _.now());
                            /*console.log('	with response ' + (typeof (response) == 'string') ? response
                                : JSON.stringify(response));*/
                            cb(response);
                        },
                        error: function (response, error, t) {
                            console.log('ajax call error ');
                            console.log('	at ' + _.now());
                            if (typeof response === 'object') {
                                console.log(JSON.stringify(response));
                            }
                            else {
                                console.log(response);
                            }
                            cb(response);
                        }
                    });
                });
            }
        };

        g_api.StructureFinder = g_api.ResourceFinder.builder()
            .resource("structure")
            .extend(function (sfinder) {
                sfinder.postSmiles = function (smi) {
                    var url = g_api.GlobalSettings.getBaseURL();
                    var pos = url.lastIndexOf("app/");
                    url = url.substring(0, pos + 4) + "structure";
                    console.log("postSmiles using URL " + url);
                    var req = g_api.Request.builder()
                        .url(url)
                        .method("POST")
                        .queryStringData({
                            "body": smi
                        });
                    return g_api.httpProcess(req).andThen(function (tmp) {
                        if (g_api.isJson(tmp)) {
                            var obj = JSON.parse(tmp);
                            console.log("Parsed object out of JSON");
                            console.log(" going to return id " + obj.structure.id);
                            return obj.structure.id;
                        }
                        return tmp;
                    });
                };

                sfinder.searchByExactName = function (q) {
                    return sfinder.search("root_names_name:\"^" + q + "$\"");
                };
            });

        /********************************
        Models
        ********************************/
        var CommonData = {
            builder: function () {
                var data = {};

                /*should be set*/
                data._path = "";
                data._type = "";

                /*default values*/
                data.uuid = UUID.randomUUID();
                data.references = [];
                data.access = [];
                data._references = [];

                data.build = function () {
                    var d = JSON.parse(JSON.stringify(data));
                    g_api.gUtil.removeKeysLike(d, /^_/);
                    return d;
                },
                    data.setAccess = function (list) {
                        data.access = list;
                        return data;
                    },
                    data.setProtected = function () {
                        console.log('setProtected called');
                        data.access = ["protected"];
                        return data;
                    },
                    data.setPublic = function (pub) {
                        if (pub) {
                            return data;
                        }
                        return data.setProtected();
                    },
                    data.setPreferred = function (preferred) {
                        data.preferred = true;
                        return data;
                    },
                    data.setDeprecated = function (d) {
                        if (d) {
                            data.deprecated = true;
                        } else {
                            data.deprecated = false;
                        }
                        return data;
                    },
                    data.addReference = function (r) {
                        if (UUID.isUUID(r)) {
                            data.addReferenceUUID(r);
                        } else {
                            if (r._type === "reference") {
                                data._references.push(r);
                                data.addReferenceUUID(r.uuid);
                            } else {
                                var ref = _.merge(Reference.builder(), r);
                                data._references.push(ref);
                                data.addReferenceUUID(ref.uuid);
                            }
                        }
                        return data;
                    },
                    data.addReferenceUUID = function (ruuid) {
                        data.references.push(ruuid);
                        return data;
                    },
                    data.addToPatch = function (patch) {
                        var builtData = data.build();
                        /*console.log('builtData: ' + JSON.stringify(builtData));*/
                        patch = patch.add(data._path, builtData);

                        if (data._references && data._references.length > 0) {
                            _.forEach(data._references, function (r) {
                                console.log('adding one reference, r ' + r);
                                patch = patch.add("/references/-", r.build());
                            });
                        }
                        return patch;
                    },
                    data.mix = function (source) {
                        _.merge(data, source);
                        return data;
                    };
                return data;
            }
        };

        var Name = {
            builder: function () {
                var name = CommonData.builder();
                name._type = "name";
                name._path = "/names/-";

                name.type = "cn";
                name.setName = function (nm) {
                    name.name = nm;
                    return name;
                };
                name.setType = function (type) {
                    name.type = type;
                    return name;
                };
                name.setLanguages = function (lng) {
                    name.languages = lng;
                    return name;
                };
                name.setDomains = function (dmns) {
                    name.domains = dmns;
                    return name;
                };
                name.setNameOrgs = function (orgs) {
                    name.nameOrgs = orgs;
                    return name;
                };
                return name;
            }
        };
        var Code = {
            builder: function () {
                var code = CommonData.builder();
                code._type = "code";
                code._path = "/codes/-";

                code.type = "cn";
                code.setCode = function (cd) {
                    code.code = cd;
                    return code;
                };
                code.setType = function (type) {
                    code.type = type;
                    return code;
                };
                code.setCodeSystem = function (sys) {
                    code.codeSystem = sys;
                    return code;
                };
                code.setCodeComments = function (cmt) {
                    code.comments = cmt;
                    return code;
                };
                code.setCodeText = function (txt) {
                    code.codeText = txt;
                    console.log('setCodeText processing codeText ' + txt);
                    return code;
                };
                code.setUrl = function (url) {
                    code.url = url;
                    return code;
                };

                return code;
            }
        };

        var Property = {
            builder: function () {
                var prop = CommonData.builder();
                prop.value = {};
                prop._type = "property";
                prop._path = "/properties/-";

                prop.propertyType = "PHYSICAL";

                prop.setName = function (newName) {
                    prop.name = newName;
                    return prop;
                };
                prop.setType = function (type) {
                    prop.propertyType = type;
                    return prop;
                };
                prop.setPropertyStringValue = function (txt) {
                    prop.value.nonNumericValue = txt;
                    console.log('setPropertyStringValue ' + txt);
                    return prop;
                };
                prop.setAverage = function (avg) {
                    prop.value.average = avg;
                    return prop;
                };
                prop.setHigh = function (val) {
                    prop.value.high = val;
                    return prop;
                };
                prop.setLow = function (val) {
                    prop.value.low = val;
                    return prop;
                };
                prop.setUnits = function (units) {
                    prop.value.units = units;
                    return prop;
                };
                return prop;
            }
        };

        var Reference = {
            builder: function () {
                var ref = CommonData.builder();
                ref._type = "reference";
                ref._path = "/references/-";
                ref._fileData = null;
                ref._uploadFileUrl = null;

                ref.setCitation = function (cit) {
                    ref.citation = cit;
                    return ref;
                };
                ref.setUrl = function (url) {
                    ref.url = url;
                    return ref;
                };
                ref.setDocType = function (typ) {
                    ref.docType = typ;
                    return ref;
                };
                ref.setPublicDomain = function (pd) {
                    ref.publicDomain = pd;
                    return ref;
                };
                ref.setUploadFileUrl = function (u) {
                    ref._uploadFileUrl = u;
                    return ref;
                };
                ref.setFileData = function (fd, ft) {
                    var formData = new FormData();
                    formData.append('file-name', fd);
                    //formData.append('file-type', ft);
                    _fileData = formData;
                    return ref;
                }
                ref.setUploadedFile = function (fileUrl) {
                    ref.uploadedFile = fileUrl;
                    return ref;
                }
                ref.processFileData = function () {
                    var url = g_api.GlobalSettings.getBaseURL();
                    var pos = url.indexOf('api');
                    url = url.substring(0, pos - 1) + "/upload";
                    console.log('url for file upload: ' + url);
                    var req = g_api.Request.builder()
                        .url(url);
                    req.b = _fileData;
                    return g_api.httpProcessFile(req).andThen(function (ret) {
                        var uploadInfo = JSON.parse(ret);
                        console.log('upload info: ' + ret);
                        console.log('  url: ' + uploadInfo.url);
                        return setUploadFileUrl(uploadInfo.url);
                    });
                }
                /*@Override*/
                var oldBuild = ref.build;
                ref.build = function () {
                    var d = oldBuild();
                    delete d.references;
                    return d;
                };
                return ref;
            },
            isDuplicate: function (existingRef, newReferenceType, newReferenceCitation, newReferenceUrl) {
                if (existingRef.docType === newReferenceType && existingRef.citation === newReferenceCitation
                    && existingRef.url === newReferenceUrl) {
                    return true;
                }
                return false;
            }
        };
        var Relationship = {
            builder: function () {
                var relationship = CommonData.builder();
                relationship._type = "relationship";
                relationship._path = "/relationships/-";
                relationship.relatedSubstance = {};
                relationship.type = "";
                relationship.setType = function (type) {
                    relationship.type = type;
                    return relationship;
                };
                relationship.setRelatedSubstance = function (relatedSubstance) {

                    relationship.relatedSubstance = {
                        refuuid: relatedSubstance.uuid,
                        refPname: relatedSubstance._name
                    }
                    return relationship;
                };

                return relationship;
            }
        };

        var Note = {
            builder: function () {
                var note = CommonData.builder();
                note._type = "note";
                note._path = "/notes/-";

                note.setNote = function (nt) {
                    note.note = nt;
                    return note;
                };
                return note;
            }
        };

        g_api.CommonData = CommonData;
        g_api.Name = Name;
        g_api.Code = Code;
        g_api.Reference = Reference;
        g_api.Relationship = Relationship;
        g_api.Property = Property;
        g_api.Note = Note;

        var Scripts = {
            scriptMap: {},
            addScript: function (s) {
                Scripts.scriptMap[s.name] = s;
                return Scripts;
            },
            get: function (nm) {
                return Scripts.scriptMap[nm];
            },
            all: function () {
                return _.chain(_.keys(Scripts.scriptMap))
                    .map(function (s) {
                        return Scripts.scriptMap[s];
                    })
                    .value();
            }
        };
        var Script = {
            builder: function () {
                var scr = {};
                scr.argMap = {};
                scr.arguments = [];
                scr.validators = [];
                scr.validatorParms = [];
                scr.addArgument = function (arg) {
                    if (arg._type !== "argument") {
                        arg = Argument.builder().mix(arg);
                    }
                    scr.arguments.push(arg);
                    scr.argMap[arg.getKey()] = arg;
                    return scr;
                };
                scr.validForSheetCreation = true;
                scr.setKey = function (key) {
                    scr.key = key;
                    return scr;
                };
                scr.setName = function (name) {
                    scr.name = name;
                    return scr;
                };
                scr.setDescription = function (desc) {
                    scr.description = desc;
                    return scr;
                };
                scr.mix = function (sc) {
                    _.merge(scr, sc);
                    _.forEach(scr.arguments, function (a) {
                        scr.argMap[a.getKey()] = a;
                    });
                    return scr;
                };
                scr.getArgument = function (narg) {
                    return scr.argMap[narg];
                };

                scr.getArgumentByName = function (narg) {
                    var l = _.filter(scr.arguments, function (a) {
                        return a.name === narg;
                    });
                    if (l.length === 0)
                        return undefined;
                    return l[0];
                };
                scr.hasArgumentByName = function (narg) {
                    return !(typeof scr.getArgumentByName(narg) === "undefined");
                };
                scr.hasArgument = function (narg) {
                    return !(typeof scr.getArgument(narg) === "undefined");
                };

                scr.setExecutor = function (exec) {
                    scr.executor = exec;
                    return scr;
                };
                scr.addValidator = function (valid, parms) {
                    scr.validators.push(valid);
                    scr.validatorParms.push(parms);
                    return scr;
                };
                scr.useFor = function (cb) {
                    if (!_.find(scr.arguments, { key: 'FORCED' })) {
                        /*Automatically include this one:*/
                        scr.addArgument(
                            {
                                "key": "FORCED", name: "FORCED",
                                description: "Override of normal validation", required: false,
                                defaultValue: "FALSE", type: "boolean"
                            });
                    }

                    cb(scr);
                };

                /*should return a promise*/
                scr.execute = function (vals) {
                    return g_api.JPromise.of(function (cb) {
                        var ret = scr.executor(vals);
                        if (ret && ret._promise) {
                            ret.get(cb);
                        } else {
                            cb(ret);
                        }
                    });

                };
                scr.runner = function () {
                    var cargs = {
                        args: {}
                    };
                    cargs.clearValues = function () {
                        argSet = this.args;
                        _.forEach(this.args, function (val, key) {
                            argSet[key].value = argSet[key].defaultValue;
                        });
                        this.args = argSet;
                    };
                    cargs.setValue = function (key, value) {
                        var darg = scr.getArgument(key);
                        if (!darg) {
                            throw "No such argument '" + key + "' in script '" + scr.name + "'";
                        }
                        cargs.args[key] = Argument.builder().mix(scr.getArgument(key)).setValue(value);
                        return cargs;
                    };
                    cargs.setValues = function (kvpairs) {
                        _.forEach(_.keys(kvpairs), function (k) {
                            cargs.setValue(k, kvpairs[k]);
                        });
                        return cargs;
                    };
                    cargs.getArguments = function () {
                        var ret = [];
                        _.forEach(_.keys(cargs.args), function (k) {
                            ret.push(cargs.args[k]);
                        });
                        return ret;
                    };
                    _.forEach(scr.arguments, function (a) {
                        cargs.args[a.getKey()] = a;
                    });

                    cargs.validate = function () {
                        var proms = _.chain(cargs.getArguments())
                            .map(function (a) {
                                return a.validate(cargs);
                            })
                            .value();
                        for (var i = 0; i < scr.validators.length; i++) {
                            proms.push(scr.validators[i](cargs.args, scr.validatorParms[i]));
                        }
                        return g_api.JPromise.join(proms)
                            .andThen(function (plist) {
                                var invalid = _.chain(plist)
                                    .filter(function (v) {
                                        return !v.valid;
                                    })
                                    .value();
                                console.log('cargs.validate about to return ' + JSON.stringify(invalid));
                                return invalid;
                            });
                    };
                    cargs.execute = function () {
                        return cargs.validate()
                            .andThen(function (v) {

                                if (v.length === 0 ||
                                    (cargs.forced() &&
                                        !_.filter(v, function (item) { return item.overall }).length > 0)) {
                                    return scr.execute(cargs.args)
                                        .andThen(function (r) {
                                            if (typeof r.valid === "undefined") {
                                                return { "valid": true, "message": "Success", "returned": r };
                                            } else {
                                                return r;
                                            }
                                        });
                                } else {
                                    var msg = _.chain(v)
                                        .map(function (mv) {
                                            return mv.message;
                                        })
                                        .value()
                                        .join(";");
                                    return { "valid": false, "message": msg };
                                }
                            });
                    };
                    cargs.forced = function () {
                        console.log("cargs.args['FORCED'] " + JSON.stringify(cargs.args['FORCED']));
                        if (cargs.args['FORCED'] && cargs.args['FORCED'].value &&
                            (cargs.args['FORCED'].value.charAt(0).toUpperCase() === 'T'
                                || cargs.args['FORCED'].value.charAt(0).toUpperCase() === 'Y')) {
                            console.log('forced returning true');
                            return true;
                        }
                        console.log('forced returning false');
                        return false;
                    };
                    return cargs;
                };
                return scr;
            }
        };

        var Argument = {
            builder: function () {
                var arg = {};
                arg._type = "argument";
                arg.opPromise = JPromise.ofScalar([]);
                arg.cvType = "";
                arg.usedForLookup = false;
                arg.allowCVOverride = false;
                /*name of the argument*/
                arg.setName = function (nm) {
                    arg.name = nm;
                    return arg;
                };

                arg.getName = function () {
                    return arg.name;
                };
                arg.mix = function (ar) {
                    return _.merge(arg, ar);
                };

                arg.validator = function (v) {
                    return g_api.JPromise.ofScalar({ valid: true });
                };

                arg.setRequired = function (r) {
                    arg.required = r;
                    return arg;
                };
                arg.setAllowCVOverride = function (a) {
                    arg.allowCVOverride = a;
                    return arg;
                }
                arg.setOptions = function (opPromise) {
                    if (opPromise._promise) {
                        arg.opPromise = opPromise;
                    } else {
                        arg.opPromise = JPromise.ofScalar(opPromise);
                    }
                    return arg;
                };
                arg.getOptions = function () {
                    return arg.opPromise;
                };
                arg.getKey = function () {
                    if (arg.key) {
                        return arg.key;
                    }
                    return arg.name;
                };

                arg.validate = function (cargs) {
                    var validFunction = arg.validator(arg.getValue(), cargs);

                    if (arg.required) {
                        if (_.isUndefined(arg.getValue()) || arg.getValue() === "") {
                            return g_api.JPromise.ofScalar({
                                "valid": false,
                                "message": "Argument '" + arg.getName() + "' must be specified"
                            });
                        }
                    }

                    if (arg.type === "cv" && !arg.allowCVOverride) {
                        return arg.opPromise.andThen(function (o) {
                            if (!_.includes(o, arg.getValue())) {
                                console.log('cv: ' + o);
                                return {
                                    "valid": false,
                                    "message": "Argument '" + arg.getName() + "' has value '"
                                        + arg.getValue() + "' which is not in the CV"
                                };
                            }
                            return validFunction;
                        });
                    }
                    return validFunction;
                }
                arg.isRequired = function () {
                    if (arg.required) {
                        return true;
                    }
                    var typeVar = typeof arg.defaultValue;
                    return (typeVar === "undefined");
                }
                arg.setDescription = function (des) {
                    arg.description = des;
                    return arg;
                }
                arg.setType = function (type) {
                    arg.type = type;
                    return arg;
                }
                arg.setValue = function (value) {
                    arg.value = value;
                    return arg;
                }
                arg.setDefault = function (def) {
                    arg.defaultValue = def;
                    return arg;
                };
                arg.getValue = function () {
                    if (!_.isUndefined(arg.value)) {
                        return arg.value;
                    } else {
                        return arg.defaultValue;
                    }
                };
                arg.isYessy = function () {
                    if (_.isUndefined(arg.value)) {
                        return false;
                    } else if (typeof arg.value === 'boolean') {
                        return arg.value;
                    } else if (typeof arg.value === 'string') {
                        var upperValue = arg.value.toUpperCase();
                        return (upperValue === 'YES' || upperValue === 'Y'
                            || upperValue === 'TRUE' || upperValue === 'T');
                    } else if (typeof arg.value === 'number') {
                        return arg.value > 0;
                    }
                    return false;
                };

                arg.setUsedForLookup = function (newValue) {
                    arg.usedForLookup = newValue;
                    return arg;
                }
                return arg;
            }
        };
        g_api.Scripts = Scripts;
        g_api.Script = Script;
        g_api.Argument = Argument;

        GSRSAPI.initialize(g_api);
        return g_api;
    },
    initialize: function (g_api) {
        _.chain(GSRSAPI.extensions)
            .forEach(function (ex) {
                ex.init(g_api);
            });
    },
    addExtension: function (ext) {
        GSRSAPI.extensions.push(ext);
    },
    extensions: [],
    consoleStack: []
};
/*Global Helpers
For use in legacy code (should refactor)
*/
var GGlob = GSRSAPI.builder();
var GlobalSettings = GGlob.GlobalSettings;
var getListener = GGlob.getListener;
var JPromise = GGlob.JPromise;
var gUtil = GGlob.gUtil;
var ResourceFinder = GGlob.ResourceFinder;
var SubstanceFinder = GGlob.SubstanceFinder;
var ReferenceFinder = GGlob.ReferenceFinder;
var SearchRequest = GGlob.SearchRequest;
var SubstanceBuilder = GGlob.SubstanceBuilder;
var Patch = GGlob.Patch;
var ResolveWorker = GGlob.ResolveWorker;
var FetcherMaker = GGlob.FetcherMaker;
var FetcherRegistry = GGlob.FetcherRegistry;
var UUID = GGlob.UUID;
var Request = GGlob.Request;
var StructureFinder = GGlob.StructureFinder;
var RequestProcessor = GGlob.RequestProcessor;

/*TODO: Finish this*/
var Validation = {
    builder: function () {
        var v = {};
    }
};

/*********************************
Models
********************************/
var CommonData = GGlob.CommonData;
var Name = GGlob.Name;
var Code = GGlob.Code;
var Property = GGlob.Property;
var Reference = GGlob.Reference;
var Relationship = GGlob.Relationship;
var Note = GGlob.Note;

var Debug = {};

/*This requires some more work
it's here as a quick and dirty way to make
VBA have a simple recipe for doing predefined things*/
var Scripts = GGlob.Scripts;
var Script = GGlob.Script;
var Argument = GGlob.Argument;
/********************************
Fetchers
********************************/
FetcherRegistry.addFetcher(
    FetcherMaker.make("Active Moiety PT", function (simpleSub) {
        return simpleSub.fetch("relationships")
            .andThen(function (r) {
                return _.chain(r)
                    .filter({ type: "ACTIVE MOIETY" })
                    .map(function (ro) {
                        return ro.relatedSubstance.refPname;
                    })
                    .value()
                    .join("|");
            });
    }).addTag("Substance")
);


FetcherRegistry.addFetcher(
    FetcherMaker.make("Active Moiety ID", function (simpleSub) {
        return simpleSub.fetch("relationships")
            .andThen(function (r) {
                return _.chain(r)
                    .filter({ type: "ACTIVE MOIETY" })
                    .map(function (ro) {
                        return ro.relatedSubstance.approvalID;
                    })
                    .value()
                    .join("|");
            });
    }).addTag("Substance")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("SMILES", function (simpleSub) {
        return simpleSub.fetch("structure/smiles");
    }).addTag("Chemical")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("InChIKey", function (simpleSub) {
        return simpleSub.fetch("structure!$inchikey()")
            .andThen(function (ik) {
                console.log('in InChIKey resolver, id: ' + ik);
                if (!ik) return null;
                if (typeof ik === 'object') {
                    if (ik.retMsg) {
                        return ik.retMsg;
                    }
                    else return "";
                }
                if (ik.indexOf("=") > -1) {
                    var iks = ik.split("=");
                    if (iks.length > 1) {
                        return iks[1];
                    } else {
                        return null;
                    }
                }
                else {
                    return ik;
                }
            });
    }).addTag("Chemical")
);


FetcherRegistry.addFetcher(
    FetcherMaker.make("Exact Test", function (simpleSub) {
        return simpleSub.fetch("structure/smiles")
            .andThen(function (smi) {
                return SubstanceFinder.getExactStructureMatches(smi)
                    .andThen(function (s) {
                        return _.chain(s.content)
                            .map(function (o) {
                                return o._name;
                            })
                            .value().join("|");
                    });
            });

    }).addTag("Tests")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Image URL", function (simpleSub) {
        return simpleSub.fetch("structure/smiles")
            .andThen(function (s) {
                if (s && s.valid === false) {
                    console.log("No structure found!");
                    return "";
                }
                var base = GlobalSettings.getBaseURL().replace(/api.*/g, "");
                var imgurl = base + "img/" + simpleSub.uuid + ".$IMGFORMAT$?size=300";

                return imgurl;
            });
    }
    ));

FetcherRegistry.addFetcher(
    FetcherMaker.make("Protein Sequence", function (simpleSub) {
        return simpleSub.fetch("protein/subunits!(sequence)!join(;)");
    }).addTag("Protein")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("UUID", function (simpleSub) {

        return JPromise.ofScalar(simpleSub.uuid);
    }).addTag("Identifiers")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Lychi L1", function (simpleSub) {
        return simpleSub.fetch("structure/properties(label:LyChI_L1)($0)/term");
    }).addTag("Chemical")
);


FetcherRegistry.addFetcher(
    FetcherMaker.make("Full Lychi", function (simpleSub) {
        return simpleSub.fetch("structure/properties").andThen(function (r) {
            return _.chain(r)
                .filter(function (r1) { return r1.label && r1.label.indexOf("LyChI_L") >= 0; })
                .sortBy("label")
                .map("term")
                .value().join("-");
        });
    }).addTag("Chemical")
);


FetcherRegistry.addFetcher(
    FetcherMaker.make("Substance Class", function (simpleSub) {
        return JPromise.ofScalar(simpleSub.substanceClass);
    }).addTag("Substance")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Record Access", function (simpleSub) {
        return JPromise.ofScalar(simpleSub.access.join(";"));
    }).addTag("Record")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Status", function (simpleSub) {
        console.log('simpleSub: ' + JSON.stringify(simpleSub));
        return JPromise.of(function (cb) {
            var returnValue = simpleSub.status;
            if (simpleSub.status === 'approved') {
                returnValue = 'Validated (UNII)';
            }
            cb(returnValue);
        });
    }).addTag("Record")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("All Names", function (simpleSub) {
        return simpleSub.fetch("names!(name)!join(|)").andThen(function (n) {
            return n.replace(/%7C/g, "|");
        });
    }).addTag("Substance")
);

FetcherRegistry.addFetcher(FetcherMaker.makeCodeFetcher("BDNUM", "BDNUM Code").addTag("Identifiers"))
    .addFetcher(FetcherMaker.makeCodeFetcher("WHO-ATC", "ATC Code").addTag("Substance"))
    .addFetcher(FetcherMaker.makeCodeFetcher("CAS", "CAS Numbers").addTag("Identifiers"))
    .addFetcher(FetcherMaker.makeCodeFetcher("EVMPD", "EVMPD Code").addTag("Identifiers"));

FetcherRegistry.addFetcher(FetcherMaker.makeScalarFetcher("_name", "Preferred Term").addTag("Substance"))
    .addFetcher(FetcherMaker.makeScalarFetcher("_approvalIDDisplay", "Approval ID (UNII)").addTag("Identifiers"))
    .addFetcher(FetcherMaker.makeScalarFetcher("createdBy", "Created By").addTag("Record"))
    .addFetcher(FetcherMaker.makeScalarFetcher("created", "Created Date").andThen(gUtil.toDate).addTag("Record"))
    .addFetcher(FetcherMaker.makeScalarFetcher("lastEditedBy", "Last Edited By").addTag("Record"))
    .addFetcher(FetcherMaker.makeScalarFetcher("lastEdited", "Last Edited Date").andThen(gUtil.toDate).addTag("Record"))
    .addFetcher(FetcherMaker.makeScalarFetcher("version", "Version").addTag("Record"))
    .addFetcher(FetcherMaker.makeAPIFetcher("structure/formula", "Molecular Formula").addTag("Chemical"))
    .addFetcher(FetcherMaker.makeAPIFetcher("structure/molfile", "Molfile").addTag("Chemical"))
    .addFetcher(FetcherMaker.makeAPIFetcher("structure/mwt", "Molecular Weight").addTag("Chemical"));

/*FetcherRegistry.addFetcher(
    FetcherMaker.make("Structural Modifications", function (simpleSub) {
        return simpleSub.fetch("protein/subunits").andThen(function (subs) {

            return simpleSub.fetch("modifications/structuralModifications").andThen(function (n) {
                return _.chain(n)
                    .map(function (sm) {
                        var type = sm.structuralModificationType;
                        var ext = sm.extent;
                        var mfrag = sm.molecularFragment;
                        var mfragUUID = "";
                        var mfragApprovalID = "";
                        var mfragName = "";
                        var residue = sm.residueModified;
                        var aasites = "";

                        if (mfrag) {
                            mfragUUID = mfrag.refuuid;
                            mfragApprovalID = mfrag.approvalID;
                            mfragName = mfrag.refPname;
                        }
                        if (sm.sites) {
                            aasites = _.chain(sm.sites)
                                .map(function (s) {
                                    var sunit = _.chain(subs)
                                        .find(function (sq) {
                                            return sq.subunitIndex === s.subunitIndex;
                                        })
                                        .value();
                                    var aa = sunit.sequence[s.residueIndex - 1];
                                    return aa;
                                })
                                .uniq()
                                .value()
                                .join(";");
                        }
                        return [type, ext, residue, mfragUUID, mfragApprovalID, mfragName, aasites].join("~");
                    })
                    .value()
                    .join("|");
            });
        });
    }).addTag("Protein")
);*/

/*corrected spelling of 'Equivalence' 24 July 2018 MAM*/
FetcherRegistry.addFetcher(
    FetcherMaker.make("Equivalence Factor", function (simpleSub) {
        return simpleSub.fetch("structure/mwt").andThen(function (mwt) {
            return simpleSub.fetch("relationships")
                .andThen(function (r) {
                    var amuuid = _.chain(r)
                        .filter({ type: "ACTIVE MOIETY" })
                        .map(function (ro) {
                            return ro.relatedSubstance.refuuid;
                        })
                        .value()[0];
                    return SubstanceFinder.get(amuuid)
                        .andThen(function (amsub) {
                            return amsub.fetch("structure/mwt").andThen(function (mwt2) {
                                if (mwt && !isNaN(mwt) && mwt2 && !isNaN(mwt2)) {
                                    return mwt2 / mwt;
                                }
                                return "";
                            });
                        });
                });
        });
    }).addTag("Chemical")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Latin Binomial", function (simpleSub) {
        return simpleSub.fetch("structurallyDiverse!$select(organismGenus,organismSpecies)!join(%20)").andThen(function (n) {
            if (n && n.length > 0 && n !== 'null%20null') {
                return n.replace(/%20/g, " ");
            }
            return "";
        });
    }).addTag("Structurally Diverse")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Author", function (simpleSub) {
        return simpleSub.fetch("structurallyDiverse/organismAuthor");
    }).addTag("Structurally Diverse")
);


FetcherRegistry.addFetcher(
    FetcherMaker.make("Part", function (simpleSub) {
        return simpleSub.fetch("structurallyDiverse/part!(term)!join(|)").andThen(function (n) {
            if (n && n.length > 0) {
                return n.replace(/%7C/g, "|");
            }
            return "";
        });
    }).addTag("Structurally Diverse")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Stereo Type", function (simpleSub) {
        return simpleSub.fetch("structure/stereoChemistry");
    }).addTag("Chemical")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Record URL", function (simpleSub) {
        return JPromise.ofScalar(GlobalSettings.getHomeURL() + "substance/" + simpleSub.uuid);
    }).addTag("Record")
);


/*If these names are directly registered*/
FetcherRegistry.addFetcher(
    FetcherMaker.make("Bracket Terms", function (simpleSub) {
        return simpleSub.fetch("names!(name)").andThen(function (n) {
            return _.chain(n)
                .filter(function (n1) {
                    return n1.match(/\[.*\]/g);
                })
                .value().join("|");
        });
    }).addTag("Substance")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Component Report", function (simpleSub) {
        var proms = [];
        proms.push(simpleSub.fetch("relationships"));
        proms.push(simpleSub.fetch("mixture/components"));
        return GGlob.JPromise.join(proms)
            .andThen(function (r) {
                console.log('in Components andThen');
                for (var i in r) {
                    if ((r[i].hasOwnProperty('length') && r[i].length === 0 || r[i].length === 1 && r[i]) ||
                        (r[i].hasOwnProperty('valid') && !r[i].valid)) {
                        continue;
                    }
                    if (typeof r[i] === 'object' && r[i].length) {
                        //we have an array
                        var mapped = _.map(r[i], function (mc) {
                            var answerParts = [];
                            if (mc.substance) {
                                var subId = mc.substance.approvalID ?
                                    mc.substance.approvalID : mc.substance.refuuid;
                                answerParts.push('MIXTURE COMPONENT');
                                answerParts.push(subId);
                                answerParts.push(mc.substance.name);
                            }
                            else if (mc.relatedSubstance && mc.type.toUpperCase().indexOf("CONSTITUENT") > -1) {
                                console.log("constituent");
                                var subId2 = mc.relatedSubstance.approvalID ?
                                    mc.relatedSubstance.approvalID : mc.relatedSubstance.refuuid;
                                answerParts.push(mc.type);
                                answerParts.push(subId2);
                                answerParts.push(mc.relatedSubstance.name);
                            }
                            if (answerParts.length > 0) return answerParts.join("^");
                        });
                        if (mapped && mapped.length && mapped.length > 0) {
                            return _.filter(mapped, function (a) {
                                return a && a.length && a.length > 0;
                            })
                                .join('|');
                        }
                    }
                }
                return ('');
            });
    }).addTag("Substance")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Vapor Pressure", function (simpleSub) {
        return simpleSub.fetch("properties")
            .andThen(function (r) {
                return _.chain(r)
                    .filter({ name: "Vapor pressure" })
                    .map(function (ro) {
                        return ro.value.average + ro.value.units;
                    })
                    .value();
            });
    }).addTag("Properties")
);

FetcherRegistry.addFetcher(
    FetcherMaker.make("Volume of Distribution", function (simpleSub) {
        return simpleSub.fetch("properties")
            .andThen(function (r) {
                return _.chain(r)
                    .filter({ name: "Volume of Distribution" })
                    .map(function (ro) {
                        var returnText = [];
                        if (ro.value.low) returnText.push("Low: " + ro.value.low);
                        if (ro.value.high) returnText.push("High: " + ro.value.high);
                        if (ro.value.average) returnText.push("Average: " + ro.value.average);
                        if (ro.value.units) returnText.push("Units: " + ro.value.units);
                        return returnText.join('; ');
                    })
                    .value();
            });
    }).addTag("Properties")
);

/*******************
CV helper (TODO:move to main library)
*******************/


var CVHelper = {
    getTermList: function (domain) {
        return JPromise.of(function (cb) {
            GGlob.CVFinder.searchByDomain(domain).andThen(function (r) {
                return _.map(r.content[0].terms, function (o) {
                    return o.value;
                });
            }).get(cb);
        });
    },
    getDictionary: function (domain) {
        /*console.log('getDictionary called with domain: ' + domain);*/
        return GGlob.CVFinder.searchByDomain(domain).andThen(function (r) {
            /*console.log('getDictionary andThen, r: '+ JSON.stringify(r));*/
            return "vocabulary:" + domain + ":" + JSON.stringify(r);
        });

    }
};


function validate4Params(args, params) {
    var requireCrossValidation = false;
    console.log('Starting in validate4Params');
    if (params && params.RequireCrossValidation !== 'undefined' && params.RequireCrossValidation) {
        requireCrossValidation = true;
    }
    console.log('requireCrossValidation: ' + requireCrossValidation);
    var twoParameterMessage = "At least two of these arguments must have values: UUID, PT and BDNUM";
    /*Look at up to 3 parameters: UUID, PT, and BDNUM
     * (We used to consider a fourth parameter: FORCED)
     * when requireCrossValidation is true, at least 2 of the above must have valid
     * values.
     * without requireCrossValidation, any one is sufficient.
     * When more than one is present, those present must point to the same Substance
     */
    if (!args.uuid.getValue() && !args.pt.getValue() && !args.bdnum.getValue()) {
        console.log('missing parm(s)');
        return GGlob.JPromise.of(function (cb) {
            var errorMessage = "At least one of these arguments must have values: UUID, PT and BDNUM";
            if (requireCrossValidation) {
                errorMessage = twoParameterMessage;
            }
            cb({
                "valid": false, "message": errorMessage,
                "overall": true
            });
        });
    }
    if (args.uuid.getValue()) {
        console.log('has UUID');
        if (!args.pt.getValue() && !args.bdnum.getValue()) {
            console.log('   and no other arg');
            /*we do have a UUID but PT and BDNUM are empty and FORCED is on
             can forego any further checking, unless we require more than one!*/
            if (requireCrossValidation) {
                return GGlob.JPromise.of(function (cb) {
                    cb({"valid": false, "message": twoParameterMessage, "overall": true});
                });
            }

            return GGlob.JPromise.of(function (cb) {
                cb({ "valid": true, "overall": true });
            });
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(args.uuid.getValue())
            .andThen(function (resp) {
                if (resp.content && resp.content.length >= 1) {
                    console.log('looked up substance by UUID');
                    var rec = resp.content[0];
                    var uuid = rec.uuid;
                    if (uuid !== args.uuid.getValue()) {
                        /*is this even possible?*/
                        return {
                            "valid": false,
                            "message": "The UUID for this record does not match the one provided",
                            "overall": true
                        };
                    }
                    var pt = rec._name;
                    if (args.pt.getValue() && pt !== args.pt.getValue()) {
                        console.log('pt: ' + pt + '; pt from args: ' + args.pt.getValue());
                        return {
                            "valid": false, "message": "The PT does not match the value for this record",
                            "overall": true
                        };
                    }

                    if (args.bdnum.getValue()) {
                        var substance = GGlob.SubstanceBuilder.fromSimple(rec);
                        return substance.fetch("codes(codeSystem:BDNUM)")
                            .andThen(function (cds) {
                                console.log('after fetching bdnum, result: ' + rec + '; cds: ' + cds);
                                var hasBdNumMatch = false;
                                _.forEach(cds, function (cd) {
                                    if (cd.code === args.bdnum.getValue()) {
                                        console.log('looking at bdnum from db: ' + cd.code
                                            + ' and from input: ' + args.bdnum.getValue());
                                        hasBdNumMatch = true;
                                        return false;
                                    }
                                });
                                if (!hasBdNumMatch) {
                                    return {
                                        "valid": false,
                                        "message": "BDNUM does not match value in database",
                                        "overall": true
                                    }
                                }
                                return { valid: true };
                            });
                    }
                    else {
                        return { valid: true };
                    }
                } else {
                    return {
                        "valid": false, "message": "Could not find record with that UUID",
                        "overall": true
                    };
                }
            });
    }
    else if (args.pt.getValue()) {
        if (requireCrossValidation && !args.bdnum.getValue()) {
            return GGlob.JPromise.of(function (cb) {
                cb({
                    "valid": false,
                    "message": twoParameterMessage,
                    "overall": true
                });
            });
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(args.pt.getValue())
            .andThen(function (resp) {
                console.log('resp: ' + JSON.stringify(resp));
                if (resp.content && resp.content.length >= 1) {
                    var rec = resp.content[0];
                    var pt = rec._name;
                    if (pt.toUpperCase() !== args.pt.getValue().toUpperCase()) {
                        return {
                            "valid": false,
                            "message": "The PT of the record does not match the value provided",
                            "overall": true
                        };
                    }

                    if (args.bdnum.getValue()) {
                        console.log('going to look up BDNum...');
                        var substance = GGlob.SubstanceBuilder.fromSimple(rec);
                        return substance.fetch("codes(codeSystem:BDNUM)")
                            .andThen(function (cds) {
                                var hasBdNumMatch = false;
                                _.forEach(cds, function (cd) {
                                    console.log('looking at bdnum from db: ' + cd.code
                                        + ' and from input: ' + args.bdnum.getValue());

                                    if (cd.code === args.bdnum.getValue()) {
                                        hasBdNumMatch = true;
                                        return false;
                                    }
                                });
                                if (!hasBdNumMatch) {
                                    return {
                                        "valid": false,
                                        "message": "BDNUM does not match value in database",
                                        "overall": true
                                    }
                                }
                                return { valid: true };
                            });
                    }
                    else {
                        console.log('Skipping BDNum look up ...');
                        return { valid: true };
                    }
                }
                else {
                    console.log('no results found for search!');
                    return {
                        "valid": false,
                        "message": "No substance found with preferred term '" + args.pt.getValue()
                            + "'",
                        "overall": true
                    }
                }
            });

    }
    if (args.bdnum.getValue()) {
        console.log('bdnum only');
        return GGlob.JPromise.of(function (cb) {
            if (requireCrossValidation) {
                return cb({
                    "valid": false,
                    "message": twoParameterMessage,
                    "overall": true
                });
            }
            cb({ valid: true });
        });

    }
    return GGlob.JPromise.of(function (cb) {
        cb({ valid: false, message: 'Unexpected result in multiple parameter validator!' });
    });

}

function validate3Params(args, params) {
    console.log('Starting in validate3Params');
    var requireCrossValidation = false;
    if (params && params.RequireCrossValidation !== 'undefined' && params.RequireCrossValidation) {
        requireCrossValidation = true;
    }
    console.log('requireCrossValidation: ' + requireCrossValidation);
    var twoParameterMessage = "At least two of these arguments must have values: UUID, PT and BDNUM";
    /*Look at up to 4 parameters: UUID, PT, and FORCED.
     When !FORCED, all of the first 3 must be present.
     Otherwise, any one is sufficient.
     When more than one is present, those present must agree
     */
    if ((!args.uuid.getValue() && !args.pt.getValue())) {
        console.log('missing parm(s)');
        var errorMessage = "At least one of these arguments must have values: UUID, PT ";
        if (requireCrossValidation) errorMessage = twoParameterMessage;
        return GGlob.JPromise.of(function (cb) {
            cb({
                "valid": false,
                "message": errorMessage,
                "overall": true
            });
        });
    }
    if (args.uuid.getValue()) {
        console.log('has UUID');
        if (!args.pt.getValue()) {
            console.log('   and no other arg');
            /*we do have a UUID but PT is empty and FORCED is on
             can forego any further checking!*/
            if (requireCrossValidation) {
                return GGlob.JPromise.of(function (cb) {
                    cb({ "valid": false, message: twoParameterMessage, "overall": true });
                });
            }
            return GGlob.JPromise.of(function (cb) {
                cb({ "valid": true, "overall": true });
            });
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(args.uuid.getValue())
            .andThen(function (resp) {
                if (resp.content && resp.content.length >= 1) {
                    console.log('looked up substance by UUID');
                    var rec = resp.content[0];
                    var uuid = rec.uuid;
                    if (uuid !== args.uuid.getValue()) {
                        /*is this even possible?*/
                        return {
                            "valid": false,
                            "message": "The UUID for this record does not match the one provided",
                            "overall": true
                        };
                    }
                    var pt = rec._name;
                    if (args.pt && args.pt.getValue() && pt !== args.pt.getValue()) {
                        console.log('pt: ' + pt + '; pt from args: ' + args.pt.getValue());
                        return {
                            "valid": false,
                            "message": "The PT does not match the value for this record",
                            "overall": true
                        };
                    }
                    console.log(' about to return simple true');
                    return { valid: true };
                } else {
                    return {
                        "valid": false,
                        "message": "Could not find record with that UUID",
                        "overall": true
                    };
                }
            });
    }
    else if (args.pt.getValue()) {
        console.log('has PT');
        if (requireCrossValidation) {
            return GGlob.JPromise.of(function (cb) {
                cb({ "valid": false, message: twoParameterMessage, "overall": true });
            });
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(args.pt.getValue())
            .andThen(function (resp) {
                if (resp.content && resp.content.length >= 1) {
                    var rec = resp.content[0];
                    var pt = rec._name;
                    if (pt !== args.pt.getValue()) {
                        return {
                            "valid": false,
                            "message": "The PT of the record does not match the value provided",
                            "overall": true
                        };
                    }
                    console.log(' about to return simple true');
                    return { "valid": true, "overall": true };
                } else {
                    console.log(' about to return simple false');
                    return {
                        "valid": false,
                        "message": "Could not find record with that PT",
                        "overall": true
                    };
                }
            });

    }

    console.log('neither UUID nor PT');
    errorMessage = "One or both of these arguments must have a value: UUID, PT";
    if (requireCrossValidation) errorMessage = twoParameterMessage;
    return GGlob.JPromise.of(function (cb) {
        cb({
            "valid": false,
            "message": errorMessage,
            "overall": true
        });
    });
}


function validateOneSubstance(subUUIDArg, subNameArg) {
    console.log('Starting in validateOneSubstance. ');
    if (subUUIDArg && subUUIDArg.getValue()) {
        console.log('has UUID');
        if (!subNameArg || !subNameArg.getValue()) {
            console.log('   and no other arg');
            /*we do have a UUID but PT is empty and FORCED is on
             can forego any further checking!*/
            return GGlob.JPromise.of(function (cb) {
                cb({ "valid": true, "overall": true });
            });
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(subUUIDArg.getValue())
            .andThen(function (resp) {
                if (resp.content && resp.content.length >= 1) {
                    console.log('looked up substance by UUID');
                    var rec = resp.content[0];
                    var uuid = rec.uuid;
                    if (uuid !== subUUIDArg.getValue()) {
                        /*is this even possible?*/
                        return {
                            "valid": false,
                            "message": "The UUID for this record does not match the one provided",
                            "overall": true
                        };
                    }
                    var pt = rec._name;
                    if (subNameArg && subNameArg.getValue() && pt !== subNameArg.getValue()) {
                        console.log('pt: ' + pt + '; pt from args: ' + subNameArg.getValue());
                        return {
                            "valid": false,
                            "message": "The PT does not match the value for this record",
                            "overall": true
                        };
                    }
                    console.log(' about to return simple true');
                    return { valid: true };
                } else {
                    console.log(' about to return simple false');
                    return {
                        "valid": false,
                        "message": "Could not find record with UUID " + subUUIDArg.getValue(),
                        "overall": true
                    };
                }
            });
    }
    else if (subNameArg.getValue()) {
        console.log('has PT');
        return GGlob.SubstanceFinder.searchByExactNameOrCode(subNameArg.getValue())
            .andThen(function (resp) {
                if (resp.content && resp.content.length >= 1) {
                    var rec = resp.content[0];
                    var pt = rec._name;
                    if (pt !== subNameArg.getValue()) {
                        return {
                            "valid": false,
                            "message": "The PT of the record does not match the value provided",
                            "overall": true
                        };
                    }
                    console.log(' about to return simple true');
                    return { "valid": true, "overall": true };
                } else {
                    console.log(' about to return simple false');
                    return {
                        "valid": false,
                        "message": "Could not find record PT " + subNameArg.getValue(),
                        "overall": true
                    };
                }
            });

    }
    return GGlob.JPromise.of(function (cb) {
        cb({
            "valid": false,
            "message": "One or both of these arguments must have a value: UUID, PT",
            "overall": true
        });
    });
}

function validate2Substances(args) {
    console.log('Starting in validate2Substances. ');

    var proms = [];
    proms.push(validateOneSubstance(args.uuid, args.pt));
    proms.push(validateOneSubstance(args.uuid2, args.pt2));
    return GGlob.JPromise.join(proms)
        .andThen(function (plist) {
            var valid = true;
            var messages = [];
            _.forEach(plist, function (p) {
                valid = valid && p.valid;
                if (!p.valid) {
                    console.log('adding message ' + p.message);
                    messages.push(p.message);
                }
            });
            console.log('validate2Substances about to return ' + valid);
            if (valid) return { "valid": true, "overall": true };
            return { "valid": false, message: messages.join(',') };
        });
}

/********************************
Scripts
********************************/
Script.builder().mix({ name: "Add Name", description: "Adds a name to a substance record" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)",
        required: false, usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM",
        description: "BDNUM of the record (used for validation)", required: false, usedForLookup: true
    })
    .addArgument({
        "key": "name", name: "NAME", description: "Name text of the new name", required: true
    })
    .addArgument({
        "key": "name type",
        name: "NAME TYPE",
        description: "Category of name",
        defaultValue: "cn",
        required: false,
        type: "cv",
        opPromise: CVHelper.getTermList("NAME_TYPE"),
        cvType: "NAME_TYPE"
    })
    .addArgument({
        "key": "language",
        name: "LANGUAGE",
        description: "Language of the new name",
        defaultValue: "English",
        required: false,
        opPromise: CVHelper.getTermList("LANGUAGE"),
        type: "cv",
        cvType: "LANGUAGE"
    })
    .addArgument({
        "key": "pd", name: "PD",
        description: "Public Domain status of the name (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: false,
        type: "cv",
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: true
    })
    .addArgument({
        "key": "reference file path", name: "REFERENCE FILE PATH",
        description: "A file to attach to the reference", required: false
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL",
        description: "URL for the reference", required: false
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Added Name",
        description: "Text for the record change", required: false
    })
    .addValidator(validate4Params, null)
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var bdnum = args.bdnum.getValue();
        var name = args.name.getValue();
        var substanceForPatch;

        var nameType = args["name type"].getValue();
        var dataPublic = args.pd.isYessy();
        var referenceType = args["reference type"].getValue();
        var referenceCitation = args["reference citation"].getValue();
        var referenceUrl = args['reference url'].getValue();
        var nameLanguage = args.language.getValue();
        var referenceFilePath = args['reference file path'].getValue();

        var reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
        console.log('referenceUrl: ' + referenceUrl);
        if (referenceUrl && referenceUrl.length > 0) {
            reference = reference.setUrl(referenceUrl);
        }

        console.log('referenceFilePath: ' + referenceFilePath);
        if (referenceFilePath && referenceFilePath.length > 0) {
            reference.setUploadedFile(referenceFilePath);
            console.log('adding uploaded file to reference');
        }
        console.log('dataPublic: ' + dataPublic);
        if (dataPublic) {
            console.log('perceived public reference');
            reference.setPublic(true);
            reference.setPublicDomain(true);
        } else {
            console.log('perceived NON public reference');
            reference.setPublic(false);
            reference.setPublicDomain(false);
        }

        var langs = [];
        langs.push(nameLanguage);

        var nameObject = Name.builder().setName(name)
            .setType(nameType)
            .setPublic(dataPublic)
            .setLanguages(langs);

        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                lookupCriterion = pt;
            }
            else {
                console.log('using bdnum ' + bdnum);
                lookupCriterion = bdnum;
            }
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (s) {
                console.log('in add name script, search returned s: ' + s);
                var substance;
                var rec = s.content[0]; /*can be undefined... todo: handle*/
                substance = GGlob.SubstanceBuilder.fromSimple(rec);

                substanceForPatch = substance;

                return substance.fetch("references")
                    .andThen(function (refs) {
                        _.forEach(refs, function (ref) {
                            if (Reference.isDuplicate(ref, referenceType, referenceCitation, referenceUrl)) {
                                console.log('Duplicate reference found! Will skip creation of new one.');
                                reference = ref;
                                return false;
                            }
                        });
                        nameObject.addReference(reference);
                        return substance;
                    })
                    .andThen(function (s2) {
                        /*var substInner = GGlob.SubstanceBuilder.fromSimple(s);*/
                        return substance.patch()
                            .addData(nameObject)
                            .add("/changeReason", args['change reason'].getValue())
                            .apply()
                            .andThen(_.identity);
                    });
            });

    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });


Script.builder().mix({ name: "Add Code", description: "Adds a code to a substance record" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)",
        required: false, usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM",
        description: "BDNUM of the record (used for validation)", required: false, usedForLookup: true
    })
    .addArgument({
        "key": "code", name: "CODE", description: "Actual code for the new item", required: true
    })
    .addArgument({
        "key": "code system", name: "CODE SYSTEM", description: "Code system of the new code",
        required: true,
        opPromise: CVHelper.getTermList("CODE_SYSTEM"),
        type: "cv",
        cvType: "CODE_SYSTEM"
    })
    .addArgument({
        "key": "code type", name: "CODE TYPE",
        description: "Code type of code. For instance, whether it's a primary code",
        defaultValue: "PRIMARY", required: false,
        opPromise: CVHelper.getTermList("CODE_TYPE"),
        type: "cv",
        cvType: "CODE_TYPE"
    })
    .addArgument({
        "key": "code text", name: "CODE TEXT",
        description: "Free text", required: false
    })
    .addArgument({
        "key": "comments", name: "COMMENTS",
        description: "Description for the new code (free text)", required: false
    })
    .addArgument({
        "key": "code url", name: "CODE URL",
        description: "URL to evaluate this code (this is distinct from the reference URL)",
        required: false
    })
    .addArgument({
        "key": "allow multiples", name: "ALLOW MULTIPLES",
        description: "Allow the entry of multiple codes within the same code system",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "pd", name: "PD",
        description: "Public Domain status of the code (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: false,
        type: "cv",
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: false
    })
    .addArgument({
        "key": "reference file path", name: "REFERENCE FILE PATH",
        description: "A file to attach to the reference", required: false
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL",
        description: "URL for the reference", required: false
    })
    .addArgument({
        "key": "replace existing", name: "REPLACE EXISTING",
        description: "when codes are found from the same system, delete what was there before adding this",
        defaultValue: false, required: false,
        type: "boolean"
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON",
        defaultValue: "Added Code",
        description: "Text for the record change", required: false
    })
    .addValidator(validate4Params, null)
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var bdnum = args.bdnum.getValue();
        var codeInput = args.code.getValue();
        var codeType = args['code type'].getValue();
        var codeSystem = args['code system'].getValue();
        var codeComments = args['comments'].getValue();
        var codeText = args['code text'].getValue();
        var allowMultiple = args['allow multiples'].isYessy();
        var url = args['code url'].getValue();
        var dataPublic = args.pd.isYessy();
        var referenceType = args['reference type'].getValue();
        var referenceCitation = args['reference citation'].getValue();
        var referenceUrl = args['reference url'].getValue();
        var replaceExisting = args['replace existing'].isYessy();
        var referenceFilePath = args['reference file path'].getValue();
        console.log('referenceFilePath: ' + referenceFilePath);

        var codesIndicesToRemove = [];

        var reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
        if (referenceUrl && referenceUrl.length > 0) {
            reference = reference.setUrl(referenceUrl);
        }
        if (dataPublic) {
            reference.setPublic(true);
            reference.setPublicDomain(true);
        } else {
            reference.setPublic(false);
            reference.setPublicDomain(false);
        }
        if (referenceFilePath && referenceFilePath.length > 0) {
            reference.setUploadedFile(referenceFilePath);
            console.log('adding uploaded file to reference');
        }

        console.log('Creating code using codeInput ' + codeInput
            + '; codeSystem ' + codeSystem
            + '; codeText ' + codeText + '; and comments: ' + codeComments);
        var code = Code.builder().setCode(codeInput)
            .setType(codeType)
            .setCodeSystem(codeSystem)
            .setPublic(dataPublic);
        if (url) {
            code.setUrl(url);
        }
        if (codeComments) {
            code.setCodeText(codeComments);
        }
        if (codeText) {
            code.setCodeComments(codeText);
        }

        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                console.log('using pt for lookup');
                lookupCriterion = pt;
            }
            else {
                lookupCriterion = bdnum;
            }
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (s) {
                if (!s || !s.content || s.content.length === 0) {
                    console.log('no results found for query of ' + lookupCriterion);
                    return { valid: false, message: 'Error looking up record for ' + lookupCriterion };
                }
                var rec = s.content[0]; /*can be undefined... todo: handle*/
                var substance = GGlob.SubstanceBuilder.fromSimple(rec);
                return substance.fetch("references")
                    .andThen(function (refs) {
                        console.log('retrieved refs');
                        _.forEach(refs, function (ref) {
                            if (Reference.isDuplicate(ref, referenceType, referenceCitation, referenceUrl)) {
                                console.log('Duplicate reference found! Will skip creation of new one.');
                                reference = ref;
                                return false;
                            }
                        });
                        code.addReference(reference);
                    })
                    .andThen(function (s2) {
                        return substance.fetch("codes")
                            .andThen(function (codes) {
                                var valuesOK = true;
                                var valuesError = '';
                                if (replaceExisting) {
                                    //iterate backwards over the collection to avoid issue 22 August 2019
                                    codes = _.forEachRight(codes, function (code, codeIndex) {
                                        if (code.codeSystem === codeSystem && code.code === codeInput) {
                                            console.log('adding code at index ' + codeIndex + ' to list');
                                            codesIndicesToRemove.push(codeIndex);
                                        }
                                    });
                                }
                                if (codesIndicesToRemove.length > 1) {
                                    console.log(' multiple codes that match input detected');
                                    valuesOK = false;
                                    valuesError =
                                        'This substance already has more than one code that match code "'
                                        + codeInput + '" for system ' + cd.codeSystem;
                                    return false;
                                }
                                _.forEach(codes, function (cd) {
                                    if (cd.codeSystem === codeSystem) {
                                        if (allowMultiple && !replaceExisting) {
                                            /*use the double equal to allow coercion of values*/
                                            if (cd.code == codeInput) {
                                                console.log(' duplicate code detected');
                                                valuesOK = false;
                                                valuesError = 'This substance already has the code "'
                                                    + codeInput + '" for system ' + cd.codeSystem;
                                                return false;
                                            }
                                        }
                                        else if (!replaceExisting) {
                                            console.log('detected duplicate');
                                            valuesOK = false;
                                            valuesError = 'This substance already has a code for system '
                                                + cd.codeSystem;
                                            return false;
                                        }
                                    }
                                });
                                if (valuesOK) {
                                    console.log('Add Code is going to return patch ');
                                    var codePatch = rec.patch();
                                    _.forEach(codesIndicesToRemove, function (index) {
                                        console.log('removing index ' + index);
                                        codePatch = codePatch.remove("/codes/" + index);
                                    });

                                    console.log('codePatch: ' + JSON.stringify(codePatch));
                                    return codePatch.addData(code)
                                        .add("/changeReason", args['change reason'].getValue())
                                        .apply()
                                        .andThen(_.identity);
                                } else {
                                    console.log('Add Code is going to return message ' + valuesError);
                                    return { "message": valuesError, "valid": false };
                                }
                            });
                    });

            });
    })
    .useFor(Scripts.addScript);

/*Add relationship by MAM 14 June 2017*/
Script.builder().mix({ name: "Add Relationship", description: "Adds a relationship to a substance record" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the (primary) substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the primary record (used for validation)",
        required: false, usedForLookup: true
    })
    .addArgument({
        "key": "uuid2", name: "UUID2", description: "UUID of the (secondary) substance record",
        required: false, usedForLookup: true
    })
    .addArgument({
        "key": "pt2", name: "PT2",
        description: "Preferred Term of the secondary record (used for validation)", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "relationship type", name: "RELATIONSHIP TYPE",
        description: "Type of the new relationship",
        "type": "cv", required: true,
        opPromise: CVHelper.getTermList("RELATIONSHIP_TYPE"),
        cvType: "RELATIONSHIP_TYPE"
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: false,
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        type: "cv",
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: false
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL",
        description: "URL for the reference", required: false
    })
    .addArgument({
        "key": "reference tags", name: "REFERENCE TAGS",
        description: "pipe-delimited set of tags for the reference", required: false
    })
    .addArgument({
        "key": "pd", name: "PD",
        description: "Public Domain status of the relationship (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON",
        defaultValue: "Added Code",
        description: "Text for the record change", required: false
    })
    .addValidator(validate2Substances, null)
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var uuid2 = args.uuid2.getValue();
        var pt = args.pt.getValue();
        var pt2 = args.pt2.getValue();
        var relationshiptype = args['relationship type'].getValue();
        console.log('got relationshiptype: ' + relationshiptype);
        var dataPublic = args.pd.isYessy();
        var referenceType = args['reference type'].getValue();
        var referenceCitation = args['reference citation'].getValue();
        var referenceUrl = args['reference url'].getValue();
        var referenceTags = args['reference tags'].getValue();
        console.log('got remaining parms ');
        var reference = null;
        if (referenceType && referenceCitation) {
            reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
            if (referenceUrl && referenceUrl.length > 0) {
                reference = reference.setUrl(referenceUrl);
            }
            if (dataPublic) {
                reference.setPublic(true);
                reference.setPublicDomain(true);
            } else {
                reference.setPublic(false);
                reference.setPublicDomain(false);
            }
            if (referenceTags && referenceTags.length > 0) {
                var tags = referenceTags.split("|");
                var tagSet = [];
                _.forEach(tags, function (tag) {
                    tagSet.push(tag);
                });
                reference.tags = tagSet;
            }
        }

        var searchCrit = (uuid) ? uuid : pt;
        var substanceObject;
        return GGlob.SubstanceFinder.searchByExactNameOrCode(searchCrit)
            .andThen(function (s) {
                var rec = s.content[0]; /*can be undefined... todo: handle*/
                substanceObject = GGlob.SubstanceBuilder.fromSimple(rec);
                console.log('going to check references');
                var substance = GGlob.SubstanceBuilder.fromSimple(s);
                return substanceObject.fetch("references")
                    .andThen(function (refs) {
                        console.log('retrieved refs: ' + JSON.stringify(refs));
                        _.forEach(refs, function (ref) {
                            if (Reference.isDuplicate(ref, referenceType, referenceCitation, referenceUrl)) {
                                console.log('Duplicate reference found! Will skip creation of new one.');
                                reference = ref;
                                return false;
                            }
                        });
                    })
            })
            .andThen(function (s1) {
                console.log('in andThen 2');
                var searchCrit2 = (uuid2) ? uuid2 : pt2;
                return GGlob.SubstanceFinder.searchByExactNameOrCode(searchCrit2)
                    .andThen(function (s2) {
                        console.log('in andThen 2 inner ');
                        var rec2 = s2.content[0]; /*can be undefined... todo: handle*/
                        var substanceObject2 = GGlob.SubstanceBuilder.fromSimple(rec2);

                        /*construct the relationship object*/
                        var relationship = Relationship.builder()
                            .setRelatedSubstance(substanceObject2) /*make sure this works!*/
                            .setType(relationshiptype);
                        if (reference) {
                            relationship.addReference(reference);
                        }

                        return substanceObject.patch().addData(relationship)
                            .add("/changeReason", args['change reason'].getValue())
                            .apply()
                            .andThen(_.identity);
                    });
            });
    })
    .useFor(Scripts.addScript);

Script.builder().mix({
    name: "Replace Code",
    description: "Replaces one code with another of the same type for a substance record identified by preferred term. Matches code ONLY by code system!"
})
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record",
        required: false, usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM",
        description: "BDNUM of the record (used for validation)", required: false, usedForLookup: true
    })
    .addArgument({
        "key": "code", name: "CODE", description: "Actual code for the new item", required: true
    })
    .addArgument({
        "key": "code system", name: "CODE SYSTEM",
        description: "Code system for the old and new codes", required: true,
        opPromise: CVHelper.getTermList("CODE_SYSTEM"),
        type: "cv",
        cvType: "CODE_SYSTEM"
    })
    .addArgument({
        "key": "code type", name: "CODE TYPE",
        description: "Code type of code. For instance, primary", defaultValue: "PRIMARY",
        required: false,
        opPromise: CVHelper.getTermList("CODE_TYPE"),
        type: "cv",
        cvType: "CODE_TYPE"
    })
    .addArgument({
        "key": "code text", name: "CODE TEXT",
        description: "Free text", required: false
    })
    .addArgument({
        "key": "comments", name: "COMMENTS", description: "Description new/replacement code",
        required: false
    })
    .addArgument({
        "key": "code url", name: "CODE URL",
        description: "URL to evaluate this code (this is distinct from the reference URL)",
        required: false
    })
    .addArgument({
        "key": "pd", name: "PD",
        description: "Public Domain status of the code (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: true,
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        type: "cv",
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: true
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL", description: "URL for the reference",
        required: false
    })
    .addArgument({
        "key": "reference tags", name: "REFERENCE TAGS",
        description: "pipe-delimited set of tags for the reference", required: false
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Added Code",
        description: "Text for the record change", required: false
    })
    .addValidator(validate4Params, { RequireCrossValidation: true })
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var bdnum = args.bdnum.getValue();
        var pt = args.pt.getValue();
        var codeValue = args.code.getValue();
        var codeType = args['code type'].getValue();
        var codeSystem = args['code system'].getValue();
        var codeComments = args.comments.getValue();
        var codeText = args['code text'].getValue();
        var url = args['code url'].getValue();
        var dataPublic = args.pd.isYessy();
        var referenceType = args['reference type'].getValue();
        var referenceCitation = args['reference citation'].getValue();
        var referenceUrl = args['reference url'].getValue();
        var reference = null;
        var referenceTags = args['reference tags'].getValue();

        if (referenceType && referenceType.length > 0 && referenceCitation && referenceCitation.length > 0) {
            reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
            reference = reference.setUrl(referenceUrl);
            if (dataPublic) {
                reference.setPublic(true);
                reference.setPublicDomain(true);
            } else {
                reference.setPublic(false);
                reference.setPublicDomain(false);
            }
            if (referenceTags && referenceTags.length > 0) {
                var tags = referenceTags.split("|");
                var tagSet = [];
                _.forEach(tags, function (tag) {
                    tagSet.push(tag);
                });
                reference.tags = tagSet;
            }
        }

        var code = Code.builder()
            .setCode(codeValue)
            .setType(codeType)
            .setCodeSystem(codeSystem)
            .setPublic(dataPublic);
        if (codeText) code.setCodeComments(codeText);
        if (codeComments) code.setCodeText(codeComments);
        console.log('code object: ' + JSON.stringify(code));

        if (url) {
            code.setUrl(url);
        }

        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                console.log('using pt');
                lookupCriterion = pt;
            }
            else {
                console.log('using bdnum ' + bdnum);
                lookupCriterion = bdnum;
            }
        }
        console.log('lookupCriterion: ' + lookupCriterion);
        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (resp) {
                if (resp.content && resp.content.length >= 1) {
                    var rec = resp.content[0];
                    var substance = GGlob.SubstanceBuilder.fromSimple(rec);
                    console.log('Found a substance with PT: ' + pt);
                    return substance.fetch("codes")
                        .andThen(function (codeCollection) {
                            return substance.fetch("references")
                                .andThen(function (refs) {
                                    var indexCodeToRemove = -1;
                                    var indexReferenceToRemove = -1;
                                    var indexReferencesToRemove = [];
                                    for (var i = 0; i < codeCollection.length; i++) {
                                        if (codeCollection[i].codeSystem === codeSystem) {
                                            indexCodeToRemove = i;
                                            break;
                                        }
                                    }

                                    _.forEach(refs, function (ref) {
                                        if (Reference.isDuplicate(ref, referenceType, referenceCitation, referenceUrl)) {
                                            console.log('Duplicate reference found! Will skip creation of new one.');
                                            reference = ref;
                                            return false;
                                        }
                                    });
                                    if (reference) {
                                        code.addReference(reference);
                                    }
                                    if (indexCodeToRemove > -1) {
                                        return rec.patch()
                                            .remove("/codes/" + indexCodeToRemove)
                                            .addData(code)
                                            .add("/changeReason", args['change reason'].getValue())
                                            .apply()
                                            .andThen(_.identity);
                                    } else {
                                        return { "message": "Error locating code to replace", "valid": false };
                                    }
                                })
                        })
                } else {
                    console.log('Did not locate substance based on ' + pt);
                    return { "message": "Did not locate substance based on " + pt, "valid": false };
                }
            });
    })
    .useFor(Scripts.addScript);

Script.builder().mix({ name: "Replace Code Text", description: "Replaces the text (comment) of one code for a substance record identified by preferred term" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record",
        required: false, usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM",
        description: "BDNUM of the record (used for validation)", required: false, usedForLookup: true
    })
    .addArgument({
        "key": "code", name: "CODE", description: "Existing code to match", required: true
    })
    .addArgument({
        "key": "code system", name: "CODE SYSTEM",
        description: "Code system to match",
        opPromise: CVHelper.getTermList("CODE_SYSTEM"),
        type: "cv",
        cvType: "CODE_SYSTEM"
    })
    .addArgument({
        "key": "code type", name: "CODE TYPE",
        description: "Code type of code. For instance, primary", defaultValue: "PRIMARY",
        required: false,
        opPromise: CVHelper.getTermList("CODE_TYPE"),
        type: "cv",
        cvType: "CODE_TYPE"
    })
    .addArgument({
        "key": "comments", name: "COMMENTS", description: "Updated description/comments for the code",
        required: false
    })
    .addArgument({
        "key": "code url", name: "CODE URL",
        description: "URL to evaluate this code (this is distinct from the reference URL)",
        required: false
    })
    .addArgument({
        "key": "pd", name: "PD",
        description: "Public Domain status of the code (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: false,
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        type: "cv",
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: false
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL", description: "URL for the reference",
        required: false
    })
    .addArgument({
        "key": "reference tags", name: "REFERENCE TAGS",
        description: "pipe-delimited set of tags for the reference", required: false
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Updated Code",
        description: "Text for the record change", required: false
    })
    .addValidator(validate4Params, {RequireCrossValidation: true})
    .setExecutor(function (args) {
        var pt = args.pt.getValue();
        var uuid = args.uuid.getValue();
        var bdnum = args.bdnum.getValue();
        var codeValue = args.code.getValue();
        var codeType = args['code type'].getValue();
        var codeSystem = args['code system'].getValue();
        var codeComments = args['comments'].getValue();
        var url = args['code url'].getValue();
        var dataPublic = args.pd.isYessy();
        var referenceType = args['reference type'].getValue();
        var referenceCitation = args['reference citation'].getValue();
        var referenceUrl = args['reference url'].getValue();
        var reference = null;
        var referenceTags = args['reference tags'].getValue();

        if (referenceType && referenceType.length > 0 && referenceCitation && referenceCitation.length > 0) {
            reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
            reference = reference.setUrl(referenceUrl);
            if (dataPublic) {
                reference.setPublic(true);
                reference.setPublicDomain(true);
            } else {
                reference.setPublic(false);
                reference.setPublicDomain(false);
            }
            if (referenceTags && referenceTags.length > 0) {
                var tags = referenceTags.split("|");
                var tagSet = [];
                _.forEach(tags, function (tag) {
                    tagSet.push(tag);
                });
                reference.tags = tagSet;
            }
        }

        var code = Code.builder()
            .setCode(codeValue)
            .setType(codeType)
            .setCodeSystem(codeSystem)
            .setCodeComments(codeComments)
            .setPublic(dataPublic);
        if (url) {
            code.setUrl(url);
        }
        if (codeComments) {
            code.setCodeText(codeComments);
        }

        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                lookupCriterion = pt;
            }
            else {
                console.log('using bdnum ' + bdnum);
                lookupCriterion = bdnum;
            }
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (resp) {
                if (resp.content && resp.content.length >= 1) {
                    var rec = resp.content[0];
                    var substance = GGlob.SubstanceBuilder.fromSimple(rec);
                    console.log('Found a substance with PT: ' + pt);
                    return substance.fetch("codes")
                        .andThen(function (codeCollection) {
                            return substance.fetch("references")
                                .andThen(function (refs) {
                                    var indexCodeToRemove = -1;
                                    var indexReferenceToRemove = -1;
                                    var indexReferencesToRemove = [];
                                    _.forEach(refs, function (ref) {
                                        if (Reference.isDuplicate(ref, referenceType, referenceCitation, referenceUrl)) {
                                            console.log('Duplicate reference found! Will skip creation of new one.');
                                            reference = null;
                                            return false;
                                        }
                                    });
                                    if (reference) {
                                        code.addReference(reference);
                                    }

                                    for (var i = 0; i < codeCollection.length; i++) {
                                        if (codeCollection[i].codeSystem === codeSystem
                                            && codeCollection[i].code === codeValue) {
                                            /*see if sufficient reference input was not provided*/
                                            if (!referenceCitation || referenceCitation.length === 0) {
                                                console.log('Copying ref ' + JSON.stringify(codeCollection[i].references));
                                                code.references = codeCollection[i].references;
                                            }
                                            indexCodeToRemove = i;
                                            break;
                                        }
                                    }

                                    if (indexCodeToRemove > -1) {
                                        console.log('creating code with ref ' + code.references);
                                        var p = rec.patch()
                                            .remove("/codes/" + indexCodeToRemove)
                                            .addData(code);
                                        if (reference) {
                                            console.log('Adding reference to patch');
                                            p.addData(reference);
                                        }
                                        return p.add("/changeReason", args['change reason'].getValue())
                                            .apply()
                                            .andThen(_.identity);
                                    } else {
                                        return { "message": "Error locating code to replace", "valid": false };
                                    }
                                });
                        });
                } else {
                    console.log('Did not locate substance based on ' + pt);
                    return { "message": "Did not locate substance based on " + pt, "valid": false };
                }
            });
    })
    .useFor(Scripts.addScript);
/*Added 25 October 2019 MAM*/
Script.builder().mix({ name: "Replace Code Type", description: "Replaces the type ('PRIMARY,' 'ALTERNATIVE', 'GENERIC (FAMILY)'..) of a code for a substance record" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)",
        required: false, usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM",
        description: "BDNUM of the record (used for validation)", required: false, usedForLookup: true
    })
    .addArgument({
        "key": "code", name: "CODE", description: "Actual code for item to match", required: true,
        usedForLookup: false
    })
    .addArgument({
        "key": "code system", name: "CODE SYSTEM", description: "Code system of the existing code to match",
        required: true, opPromise: CVHelper.getTermList("CODE_SYSTEM"), type: "cv",
        cvType: "CODE_SYSTEM", usedForLookup: false
    })
    .addArgument({
        "key": "code type", name: "CODE TYPE", description: "New type for the code. For instance, GENERIC",
        required: true, opPromise: CVHelper.getTermList("CODE_TYPE"), type: "cv",
        cvType: "CODE_TYPE", usedForLookup: false
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Added Code",
        description: "Text for the record change", required: false, usedForLookup: false
    })
    .addValidator(validate4Params, { RequireCrossValidation: true })
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var bdnum = args.bdnum.getValue();
        var codeInput = args.code.getValue();
        var codeType = args['code type'].getValue();
        var codeSystem = args['code system'].getValue();

        var codesToUpdate = [];
        var codeIndicesToUpdate = [];

        console.log('Looking for code ' + codeInput
            + ' of codeSystem ' + codeSystem);
        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                console.log('using pt for lookup');
                lookupCriterion = pt;
            }
            else {
                lookupCriterion = bdnum;
            }
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (s) {
                if (!s || !s.content || s.content.length === 0) {
                    console.log('no results found for query of ' + lookupCriterion);
                    return { valid: false, message: 'Error looking up record for ' + lookupCriterion };
                }
                var rec = s.content[0]; /*can be undefined... todo: handle*/
                var substance = GGlob.SubstanceBuilder.fromSimple(rec);
                return substance.fetch("references")
                    .andThen(function (s2) {
                        return substance.fetch("codes")
                            .andThen(function (codes) {
                                var valuesError = '';
                                //iterate backwards over the collection to avoid issue 22 August 2019
                                codes = _.forEachRight(codes, function (cd, codeIndex) {
                                    if (cd.codeSystem === codeSystem && cd.code === codeInput) {
                                        console.log('located code at index ' + codeIndex + ' to change type '
                                            + codeType);

                                        var code = Code.builder()
                                            .setCode(codeInput)
                                            .setType(codeType)
                                            .setCodeSystem(codeSystem)
                                            .setCodeComments(cd.comments)
                                            .setPublic(cd.public)
                                            .setUrl(cd.url)
                                            .setAccess(cd.access);
                                        _.forEach(cd.references, function (r) {
                                            code.addReference(r);
                                        });

                                        //code.mix(code, cd);
                                        codeIndicesToUpdate.push(codeIndex);
                                        codesToUpdate.push(code);
                                    }
                                });

                                if (codeIndicesToUpdate.length > 0 || codesToUpdate.length > 0) {
                                    console.log('Replace Code Type is going to return patch ');
                                    var codePatch = rec.patch();
                                    _.forEach(codeIndicesToUpdate, function (c, i) {
                                        codePatch = codePatch.remove("/codes/" + codeIndicesToUpdate[i]);
                                    });
                                    _.forEach(codesToUpdate, function (c, i) {
                                        codePatch = codePatch.addData(c);
                                    })

                                    console.log('codePatch: ' + JSON.stringify(codePatch));
                                    return codePatch
                                        .add("/changeReason", args['change reason'].getValue())
                                        .apply()
                                        .andThen(_.identity);
                                } else {
                                    console.log('Replace Code Type is going to return message ' + valuesError);
                                    return { "message": valuesError, "valid": false };
                                }
                            });
                    });

            });
    })
    .useFor(Scripts.addScript);

/*Remove Name*/
Script.builder().mix({ name: "Remove Name", description: "Removes a name from a substance record" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)",
        usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM", description: "BDNUM of the record (used for validation)",
        usedForLookup: true
    })
    .addArgument({
        "key": "name", name: "NAME", description: "Text of the name to delete", required: true,
        "validator": function (val) {
            console.log('starting in validator for arg name');
            return GGlob.SubstanceFinder.searchByExactName(val)
                .andThen(function (resp) {
                    console.log('in andThen for validator');
                    if (resp.content && resp.content.length < 1) {
                        return { valid: false, message: "The name '" + val + "' was not found in the database. " };
                    } else {
                        return { valid: true };
                    }
                });
        }
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Delete Name",
        description: "Text for the record change log", required: false
    })
    .addValidator(validate4Params, { RequireCrossValidation: true })
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var bdnum = args.bdnum.getValue();
        var nameToRemove = args.name.getValue();

        var s0;
        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                lookupCriterion = pt;
            }
            else {
                lookupCriterion = bdnum;
            }
        }
        console.log('lookupCriterion = ' + lookupCriterion);
        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (resp) {
                if (resp.content && resp.content.length >= 1) {
                    console.log('looked up substance successfully');
                    var rec = resp.content[0];
                    substance = GGlob.SubstanceBuilder.fromSimple(rec);
                    s0 = substance;
                    return substance.full();
                }
                return { valid: false, message: 'Error looking up substance' };
            })
            .andThen(function (s) {
                var nameIndex = -1;
                for (var i = 0; i < s.names.length; i++) {
                    if (s.names[i].name === nameToRemove) {
                        nameIndex = i;
                        break;
                    }
                }

                if (nameIndex <= -1) {
                    return { valid: false, message: "Unable to locate name to delete: " + nameToRemove }
                }
                return s0.patch()
                    .remove("/names/" + nameIndex) /*, args['change reason'].getValue())*/
                    .add("/changeReason", args['change reason'].getValue())
                    .apply()
                    .andThen(function (s0) {
                        return s0;
                    });
            });
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });

/*Remove Code*/
Script.builder().mix({ name: "Remove Code", description: "Removes a single code from a substance record. Note: this method makes changes to existing records" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)",
        usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM", description: "BDNUM of the record (used for validation)",
        usedForLookup: true
    })
    .addArgument({
        "key": "code", name: "CODE", description: "Code value of the code to delete", required: true
    })
    .addArgument({
        "key": "code system", name: "CODE SYSTEM", description: "Code system of the code to delete"
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Delete Name",
        description: "Text for the record change log", required: false
    })
    .addValidator(validate4Params, { RequireCrossValidation: true })
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var bdnum = args.bdnum.getValue();
        var codeToRemove = args.code.getValue();
        var codeSystemToRemove = args['code system'].getValue();
        console.log('Looking for codeToRemove: ' + codeToRemove + '; codeSystemToRemove: '
            + codeSystemToRemove);

        var s0;
        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                lookupCriterion = pt;
            }
            else {
                lookupCriterion = bdnum;
            }
        }
        console.log('lookupCriterion = ' + lookupCriterion);
        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (resp) {
                if (resp.content && resp.content.length >= 1) {
                    console.log('looked up substance successfully');
                    var rec = resp.content[0];
                    substance = GGlob.SubstanceBuilder.fromSimple(rec);
                    s0 = substance;
                    return substance.full();
                }
                return { valid: false, message: 'Error looking up substance' };
            })
            .andThen(function (s) {
                var codeIndex = -1;
                console.log('total codes: ' + s.codes.length);
                for (var i = 0; i < s.codes.length; i++) {
                    if (s.codes[i].code === codeToRemove && s.codes[i].codeSystem === codeSystemToRemove) {
                        codeIndex = i;
                        break;
                    }
                }

                if (codeIndex <= -1) {
                    return {
                        valid: false, message: "Unable to locate code to delete: "
                            + codeSystemToRemove + '.' + codeToRemove
                    }
                }
                return s0.patch()
                    .remove("/codes/" + codeIndex)
                    .add("/changeReason", args['change reason'].getValue())
                    .apply()
                    .andThen(function (s0) {
                        return s0;
                    });
            });
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });


/*Update the URL for a given code via substance name MAM 6 July 2017*/
Script.builder().mix({
    name: "Fix Code URLS",
    description: "Replaces the URL associated with a code on a substance record when a code of that type already exists"
})
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        /*deliberately NOT making this a controlled vocabulary because we want to allow for handling
         of a code whose type might have been removed from the CV*/
        "key": "code system", name: "CODE SYSTEM", description: "Code system to modify",
        required: true, defaultValue: "CAS"
    })
    .addArgument({
        "key": "url base", name: "URL BASE",
        defaultValue: "Stem for the formation of URLs, with Code to be appended",
        description: "Text for the record change", required: true
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Fixing Code URLs",
        description: "Text for the record change", required: false
    })
    .addValidator(validate3Params, {RequireCrossValidation: true})
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var codeSystem = args['code system'].getValue();
        var pt = args.pt.getValue();
        var urlBase = args['url base'].getValue();

        var searchCrit = uuid ? uuid : pt;
        return GGlob.SubstanceFinder.searchByExactNameOrCode(searchCrit)
            .andThen(function (s) {
                if (!s || !s.content || s.content.length === 0) {
                    return { valid: false, message: 'search for ' + searchCrit + ' returned no records' };
                }
                var rec = s.content[0]; /*can be undefined... */

                s0 = GGlob.SubstanceBuilder.fromSimple(rec);
                if (s0)
                    return s0.full();
                else
                    return { valid: false, message: 'unexpected error' };
            })
            .andThen(function (s) {
                console.log('Starting in second andThen');
                if (s.valid === false)
                    return s;
                console.log('Looking at codes collection which has ' + s.codes.length);

                var codesToUpdate = [];
                var codeIndicesToUpdate = [];

                _.forEach(s.codes, function (c, i) {
                    if (c.codeSystem === codeSystem) {
                        /*replace the URL*/
                        c.url = urlBase + c.code;
                        codesToUpdate.push(c);
                        codeIndicesToUpdate.push(i);
                    }
                });

                var updatePatch = s0.patch();
                /* This code handles multiple items*/
                _.forEach(codesToUpdate, function (c, i) {
                    updatePatch = updatePatch.replace("/codes/" + codeIndicesToUpdate[i], c);
                });
                return updatePatch
                    .add("/changeReason", args['change reason'].getValue())
                    .apply()
                    .andThen(function (arg) {
                        return arg;
                    });
            });
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });

/*Set object MAM 5 July 2017*/
Script.builder().mix({ name: "Set Object JSON", description: "Replace an entire record based on JSON read in" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: true,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)",
        usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM", description: "BDNUM of the record (used for validation)",
        usedForLookup: true
    })
    .addArgument({
        "key": "json", name: "JSON", description: "JSON (string) version of record to replace",
        required: true
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON",
        description: "Text for the record change", required: false
    })
    .addValidator(validate4Params, {RequireCrossValidation: true})
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var jsonString = args.json.getValue();
        console.log('retrieved args');
        return SubstanceFinder.get(uuid)
            .andThen(function (s) {
                var updatePatch = s.patch();
                console.log('called .patch');
                jsonString = jsonString.replace(/ꬷ/g, "\\n");
                console.log('called .replace');
                var parsedJson = JSON.parse(jsonString);
                updatePatch = updatePatch.replace("", parsedJson);
                console.log('updated patch');
                return updatePatch
                    .add("/changeReason", args['change reason'].getValue())
                    .apply()
                    .andThen(function () {});
            })
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });

/*Update the visibility of a given code via UUID MAM 14 October 2017*/
Script.builder().mix({ name: "Set Code Access", description: "Sets the permission on a code for a given substance record" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM", description: "BDNUM of the record (used for validation)",
        usedForLookup: true
    })
    .addArgument({
        "key": "code system", name: "CODE SYSTEM",
        description: "Code system to modify", required: true, defaultValue: "CAS"
    })
    .addArgument({
        "key": "access", name: "ACCESS", defaultValue: "protected",
        description: "Text for the access value of the code",
        required: true
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Changing Code protection", description: "Text for the record change", required: false
    })
    .addValidator(validate4Params, { RequireCrossValidation: false })
    .setExecutor(function (args) {
        var ACCESS_NONE = '[NONE]';
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var codeSystem = args['code system'].getValue();
        var access = args['access'].getValue();
        var searchCrit = (uuid) ? uuid : pt;
        return GGlob.SubstanceFinder.searchByExactNameOrCode(searchCrit)
            .andThen(function (s) {
                if (!s || !s.content || s.content.length === 0) {
                    return { valid: false, message: 'search for ' + searchCrit + ' returned no records' };
                }
                var rec = s.content[0]; /*can be undefined... */

                s0 = GGlob.SubstanceBuilder.fromSimple(rec);
                if (s0)
                    return s0.full();
                else
                    return { valid: false, message: 'unexpected error' };

            })
            .andThen(function (s) {
                console.log('Starting in second andThen');
                if (s.valid === false)
                    return s;
                console.log('Looking at codes collection which has ' + s.codes.length);

                var codesToUpdate = [];
                var codeIndicesToUpdate = [];

                _.forEach(s.codes, function (c, i) {
                    if (c.codeSystem === codeSystem) {
                        /*replace the access*/
                        if (!(c.access && typeof c.access === 'object')) {
                            console.log('creating access array');
                            c.access = [];
                        }
                        console.log('Appending access: ' + access);
                        if (access === ACCESS_NONE) {
                            c.access = [];
                        } else {
                            c.access.push(access);
                        }
                        codesToUpdate.push(c);
                        codeIndicesToUpdate.push(i);
                    }
                });

                var updatePatch = s0.patch();
                /* This code handles multiple items*/
                _.forEach(codesToUpdate, function (c, i) {
                    updatePatch = updatePatch.replace("/codes/" + codeIndicesToUpdate[i], c);
                });
                return updatePatch
                    .add("/changeReason", args['change reason'].getValue())
                    .apply()
                    .andThen(function (arg) {
                        return arg;
                    });
            })
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });

Script.builder().mix({ name: "Create Substance", description: "Creates a brand new substance record" })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the new substance", required: true,
        "validator": function (val, cargs) {
            return GGlob.SubstanceFinder.searchByExactNameOrCode(val)
                .andThen(function (resp) {
                    if (resp.content && resp.content.length >= 1) {
                        return { valid: false, message: "The PT for this record already exists" };
                    } else {
                        return { valid: true };
                    }
                });
        }
    })
    .addArgument({
        "key": "pt language", name: "PT LANGUAGE",
        description: "language for Preferred Term",
        required: true, defaultValue: "English",
        opPromise: CVHelper.getTermList("LANGUAGE"),
        type: "cv",
        cvType: "LANGUAGE"
    })
    .addArgument({
        "key": "pt name type", name: "PT NAME TYPE",
        description: "2/3-letter name type (e.g., cn, of) for Preferred Term",
        required: true, defaultValue: "cn",
        opPromise: CVHelper.getTermList("NAME_TYPE"),
        type: "cv",
        cvType: "NAME_TYPE"
    })
    .addArgument({
        "key": "substance class", name: "SUBSTANCE CLASS",
        description: "Category", required: true,
        defaultValue: "chemical",
        opPromise: CVHelper.getTermList("SUBSTANCE_CLASS"),
        type: "cv",
        cvType: "SUBSTANCE_CLASS"
    })
    .addArgument({
        "key": "smiles", name: "SMILES", description: "Structure as SMILES",
        required: false
    })
    .addArgument({
        "key": "molfile", name: "MOLFILE", description: "Structure as molfile",
        required: false
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: false,
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        type: "cv",
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: true
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL", description: "URL for the reference",
        required: false
    })
    .addArgument({
        "key": "pd", name: "PD",
        description: "Public Domain status of the code (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON",
        defaultValue: "Creating new substance", description: "Text for the record change",
        required: false
    })
    .setExecutor(function (args) {
        console.log('Starting in Create Substance executor');

        var pt = args.pt.getValue();
        var substanceClass = args['substance class'].getValue();
        var dataPublic = args.pd.isYessy();
        var referenceType = args['reference type'].getValue();
        var referenceCitation = args['reference citation'].getValue();
        var referenceUrl = args['reference url'].getValue();
        var smiles = args.smiles.getValue();
        var molfileText = args.molfile.getValue();
        console.log('got molfile');
        var nameType = args['pt name type'].getValue();
        console.log('nameType: ' + nameType);
        var nameLang = args['pt language'].getValue();

        var refuuid = GSRSAPI.builder().UUID.randomUUID();
        var reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
        if (referenceUrl && referenceUrl.length > 0) {
            reference = reference.setUrl(referenceUrl);
        }
        if (dataPublic) {
            reference.setPublic(true);
            reference.setPublicDomain(true);
        } else {
            reference.setPublic(false);
            reference.setPublicDomain(false);
        }
        reference.uuid = refuuid;

        var langs = [];
        langs.push(nameLang);
        console.log('pushed ' + nameLang + ' onto langs');
        var name = Name.builder().setName(pt)
            .setType(nameType)
            .setPublic(dataPublic)
            .setPreferred(true)
            .setLanguages(langs)
            .addReference(reference);
        console.log('created name');
        var simpleSub = {
            substanceClass: substanceClass,
            access: ["protected"],
            names: [],
            references: []
        };
        simpleSub.names.push(name);
        simpleSub.references.push(reference);

        if ((smiles && smiles.length > 0) || (molfileText && molfileText.length > 0)) {
            console.log('Processing SMILES/molfile');
            var structure = {};
            structure.smiles = smiles;
            if (molfileText && molfileText.length > 0) {
                console.log('molfileText not null.');
                structure.molfile = molfileText;
            } else {
                console.log('molfileText null.');
                structure.molfile = smiles;
            }
            structure.references = [];
            structure.references.push(refuuid);
            simpleSub.structure = structure;
        }

        var sub = SubstanceBuilder.fromSimple(simpleSub);

        var p = sub.patch();
        if (args['change reason'] && args['change reason'].getValue()) {
            p.add("/changeReason", args['change reason'].getValue())
        }
        return p.apply()
            .andThen(function (resp) {
                /*if (typeof (resp) == 'object')
                    console.log('response to patch: ' + JSON.stringify(resp));
                else
                    console.log('response to patch: ' + resp);*/
                return resp;
            });
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });


Script.builder().mix({
    name: "Create Substance from SD File",
    description: "Creates a brand new substance record using data read in from an SD file",
    validForSheetCreation: false
})
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the new substance", required: true,
        "validator": function (val, cargs) {
            return GGlob.SubstanceFinder.searchByExactNameOrCode(val)
                .andThen(function (resp) {
                    if (resp.content && resp.content.length >= 1) {
                        return { valid: false, message: "The PT for this record already exists" };
                    } else {
                        return { valid: true };
                    }
                });
        }
    })
    .addArgument({
        "key": "pt language", name: "PT LANGUAGE",
        description: "language for Preferred Term",
        required: true, defaultValue: "en",
        opPromise: CVHelper.getTermList("LANGUAGE"),
        type: "cv",
        cvType: "LANGUAGE"
    })
    .addArgument({
        "key": "pt name type", name: "PT NAME TYPE",
        description: "2/3-letter name type (e.g., cn, of) for Preferred Term",
        required: true, defaultValue: "cn",
        opPromise: CVHelper.getTermList("NAME_TYPE"),
        type: "cv",
        cvType: "NAME_TYPE"
    })
    .addArgument({
        "key": "substance class", name: "SUBSTANCE CLASS",
        description: "Category", required: true,
        defaultValue: "chemical",
        opPromise: CVHelper.getTermList("SUBSTANCE_CLASS"),
        type: "cv",
        cvType: "SUBSTANCE_CLASS"
    })
    .addArgument({
        "key": "smiles", name: "SMILES", description: "Structure as SMILES",
        required: false
    })
    .addArgument({
        "key": "molfile", name: "MOLFILE", description: "Structure as molfile",
        required: false
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: false,
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        type: "cv",
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: true
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL", description: "URL for the reference",
        required: false
    })
    .addArgument({
        "key": "pd", name: "PD",
        description: "Public Domain status of the code (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "cas", name: "CAS",
        description: "CAS number",
        defaultValue: false, required: false
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON",
        defaultValue: "Creating new substance", description: "Text for the record change",
        required: false
    })
    .setExecutor(function (args) {
        console.log('Starting in Create Substance from SD File executor');
        var pt = args.pt.getValue();
        var substanceClass = args['substance class'].getValue();
        var dataPublic = args.pd.isYessy();
        var referenceType = args['reference type'].getValue();
        var referenceCitation = args['reference citation'].getValue();
        var referenceUrl = args['reference url'].getValue();
        var smiles = args.smiles.getValue();
        var molfileText = args.molfile.getValue();
        console.log('got molfile');
        var nameType = args['pt name type'].getValue();
        console.log('nameType: ' + nameType);
        var nameLang = args['pt language'].getValue();

        var refuuid = GSRSAPI.builder().UUID.randomUUID();
        var casno = args.cas.getValue();
        var reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
        if (referenceUrl && referenceUrl.length > 0) {
            reference = reference.setUrl(referenceUrl);
        }
        if (dataPublic) {
            reference.setPublic(true);
            reference.setPublicDomain(true);
        } else {
            reference.setPublic(false);
            reference.setPublicDomain(false);
        }
        reference.uuid = refuuid;

        var langs = [];
        langs.push(nameLang);
        console.log('pushed ' + nameLang + ' onto langs');
        var name = Name.builder().setName(pt)
            .setType(nameType)
            .setPublic(dataPublic)
            .setPreferred(true)
            .setLanguages(langs)
            .addReference(reference);
        console.log('created name');
        var simpleSub = {
            substanceClass: substanceClass,
            access: ["protected"],
            names: [],
            references: [],
            properties: []
        };
        var code = null;
        if (casno) {
            code = Code.builder().setCode(casno)
                .setType("PRIMARY")
                .setCodeSystem("CAS")
                .setPublic(dataPublic);
        }
        simpleSub.names.push(name);
        simpleSub.references.push(reference);

        for (var arg in args) {
            console.log('arg name ' + arg + ' = ' + args[arg].getValue());
            if (arg.toUpperCase().indexOf("PROPERTY:") > -1 && args[arg].getValue()) {
                var tokens = arg.split(":");

                var propName = tokens[1];
                var propInterpretation = '';
                var units = '';
                if (tokens.length >= 3) propInterpretation = tokens[2];
                if (tokens.length >= 4) units = tokens[3];

                console.log("Creating property " + propName);
                var prop = Property.builder().setName(propName);
                var floatVal = parseFloat(args[arg].getValue());
                if (isNaN(floatVal) || (propInterpretation && propInterpretation.toUpperCase() === 'TEXT')) {
                    prop.setPropertyStringValue(args[arg].getValue());
                }
                else {
                    if (propInterpretation) {
                        if (propInterpretation.toUpperCase() === "HIGH") {
                            console.log('setting high value');
                            prop.setHigh(floatVal);
                        }
                        else if (propInterpretation.toUpperCase() === "LOW") {
                            console.log('setting low value');
                            prop.setLow(floatVal);
                        }
                        else {
                            console.log('setting average value');
                            prop.setAverage(floatVal);
                        }
                    }
                    else {
                        console.log('setting average value (default)');
                        prop.setAverage(floatVal);
                    }
                }
                if (units) prop.setUnits(units);

                simpleSub.properties.push(prop);
            }
        }

        if ((smiles && smiles.length > 0) || (molfileText && molfileText.length > 0)) {
            console.log('Processing SMILES/molfile');
            var structure = {};
            structure.smiles = smiles;
            if (molfileText && molfileText.length > 0) {
                console.log('molfileText not null.');
                structure.molfile = molfileText;
            } else {
                console.log('molfileText null.');
                structure.molfile = smiles;
            }
            structure.references = [];
            structure.references.push(refuuid);
            simpleSub.structure = structure;
        }

        var sub = SubstanceBuilder.fromSimple(simpleSub);
        if (code) {
            sub.codes = [];
            sub.codes.push(code);
            console.log("Adding CAS number");
        }

        var p = sub.patch();
        if (args['change reason'] && args['change reason'].getValue()) {
            p.add("/changeReason", args['change reason'].getValue());
        }
        return p.apply()
            .andThen(function (resp) {
                /*if (typeof (resp) == 'object')
                    console.log('response to patch: ' + JSON.stringify(resp));
                else
                    console.log('response to patch: ' + resp);*/
                return resp;
            });
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });


/*Touch Record - retrieve a record and save again without making any changes to trigger update processing*/
Script.builder().mix({ name: "Touch Record", description: "Retrieve a substance record and save again with no futher changes" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: true,
        usedForLookup: true
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Trigger update processing", description: "Text for the record change", required: false
    })
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();

        var s0;
        return SubstanceFinder.get(uuid)
            .andThen(function (s) {
                console.log('Processing ' + s.uuid);
                return s.patch()
                    .add("/changeReason", args['change reason'].getValue())
                    .apply()
                    .andThen(_.identity);
            });
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });


/*Replace one name with another*/
Script.builder().mix({ name: "Replace Name", description: "Locates an existing name within a substance record and replaces it with a new name" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM", description: "BDNUM of the record (used for validation)", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "current name", name: "CURRENT NAME", description: "Name text of the name to replace", required: true,
        "validator": function (val) {
            return GGlob.SubstanceFinder.searchByExactName(val)
                .andThen(function (resp) {
                    if (resp.content && resp.content.length < 1) {
                        return { valid: false, message: "The name '" + val + "' was not found in the database. " };
                    } else {
                        return { valid: true };
                    }
                });
        }
    })
    .addArgument({
        "key": "new name", name: "NEW NAME",
        description: "Text for the record change", required: true
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Replace Name",
        description: "Text for the record change", required: false
    })
    .addValidator(validate4Params, {RequireCrossValidation: true})
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var bdnum = args.bdnum.getValue();
        var nameToReplace = args['current name'].getValue();
        var newName = args['new name'].getValue();

        var name = null;
        var s0;

        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                lookupCriterion = pt;
            }
            else {
                lookupCriterion = bdnum;
            }
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (s) {
                var substance;
                var rec = s.content[0]; /*can be undefined... todo: handle*/
                substance = GGlob.SubstanceBuilder.fromSimple(rec);
                s0 = substance;
                return substance.full();
            })
            .andThen(function (s) {
                var nameIndex = -1;
                for (var i = 0; i < s.names.length; i++) {
                    if (s.names[i].name === nameToReplace) {
                        nameIndex = i;
                        name = Name.builder().setName(newName)
                            .setType(s.names[i].type)
                            .setLanguages(s.names[i].languages)
                            .setDomains(s.names[i].domains)
                            .setNameOrgs(s.names[i].nameOrgs);
                        console.log('Built name with value ' + newName + '; type: ' + s.names[i].type
                            + '; domains: ' + s.names[i].domains);
                        name.public = s.names[i].public;
                        name.references = s.names[i].references;
                        name.access = s.names[i].access;
                        console.log('	applied additional properties such as public ' + name.public);
                        break;
                    }
                }

                if (nameIndex <= -1) {
                    return { valid: false, message: "Unable to locate name to replace: " + nameToReplace }
                }
                return s0.patch()
                    .replace("/names/" + nameIndex, name)
                    .add("/changeReason", args['change reason'].getValue())
                    .apply()
                    .andThen(_.identity);
            });
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });


/*Add a volume of distribution*/
Script.builder().mix({ name: "Volume of Distribution", description: "Add values to Volume of Distribution Property for a substance record" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM", description: "BDNUM of the record (used for validation)", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "low value", name: "LOW VALUE", description: "Minimum of the value range", required: false
    })
    .addArgument({
        "key": "high value", name: "HIGH VALUE", description: "Maximum of the value range", required: false
    })
    .addArgument({
        "key": "average", name: "AVERAGE",
        description: "Middle of the value range", required: false
    })
    .addArgument({
        "key": "units", name: "UNITS",
        description: "Unit of measure for this value", required: false,
        defaultValue: "Liters/Kilogram"
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: false,
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        type: "cv",
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: true
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL", description: "URL for the reference",
        required: false
    })
    .addArgument({
        "key": "pd", name: "PD",
        description: "Public Domain status of the property (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "reference tags", name: "REFERENCE TAGS",
        description: "pipe-delimited set of tags for the reference", required: false
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Adding a value to the Volume of Distribution property",
        description: "Text for the record change", required: false
    })
    .addValidator(validate4Params,null)
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var bdnum = args.bdnum.getValue();
        var lowValue = args["low value"].getValue();
        var highValue = args["high value"].getValue();
        var averageValue = args["average"].getValue();
        var units = args.units.getValue();
        var referenceType = args['reference type'].getValue();
        var referenceCitation = args['reference citation'].getValue();
        var referenceUrl = args['reference url'].getValue();
        var referenceTags = args['reference tags'].getValue();
        var dataPublic = args.pd.isYessy();

        var reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
        if (referenceUrl && referenceUrl.length > 0) {
            reference = reference.setUrl(referenceUrl);
        }
        if (dataPublic) {
            reference.setPublic(true);
            reference.setPublicDomain(true);
        } else {
            reference.setPublic(false);
            reference.setPublicDomain(false);
        }
        if (referenceTags && referenceTags.length > 0) {
            var tags = referenceTags.split("|");
            var tagSet = [];
            _.forEach(tags, function (tag) {
                tagSet.push(tag);
            });
            reference.tags = tagSet;
        }

        var s0;
        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                lookupCriterion = pt;
            }
            else {
                lookupCriterion = bdnum;
            }
        }
        var prop = Property.builder().setName("Volume of Distribution");
        prop.setType("PHARMACOKINETIC");
        if (!isNaN(parseFloat(lowValue))) {
            prop.setLow(lowValue);
            console.log('set low value: ' + lowValue);
        }
        else {
            console.log('omitted low value');
        }
        if (!isNaN(parseFloat(highValue))) {
            prop.setHigh(highValue);
            console.log('set high value: ' + highValue);
        }
        else {
            console.log('omitted high value');
        }
        if (!isNaN(parseFloat(averageValue))) {
            prop.setAverage(averageValue);
            console.log('set avg value: ' + averageValue);
        }
        else {
            console.log('omitted avg value');
        }
        if (units) prop.setUnits(units);
        if (!dataPublic) prop.setAccess(["restricted"]);

        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (s) {
                var substance;
                var rec = s.content[0]; /*can be undefined... todo: handle*/
                substance = GGlob.SubstanceBuilder.fromSimple(rec);
                s0 = substance;
                return substance.full();
            })
            .andThen(function (s) {
                return s0.fetch("references")
                    .andThen(function (refs) {
                        _.forEach(refs, function (ref) {
                            if (Reference.isDuplicate(ref, referenceType, referenceCitation, referenceUrl)) {
                                console.log('Duplicate reference found! Will skip creation of new one.');
                                reference = ref;
                                return false;
                            }
                        });
                        if (reference) {
                            prop.addReference(reference);
                        }
                        return s0.patch()
                            .addData(prop)
                            .add("/changeReason", args['change reason'].getValue())
                            .apply()
                            .andThen(_.identity);
                    });
            });
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });

/*Add a value to a property selected by the user*/
Script.builder().mix({ name: "Add Property Value", description: "Add a value to a specified property for a substance record" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM", description: "BDNUM of the record (used for validation)", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "property name", name: "PROPERTY NAME", description: "Property to use", required: true,
        opPromise: CVHelper.getTermList("PROPERTY_NAME"),
        type: "cv", cvType: "PROPERTY_NAME"
    })
    .addArgument({
        "key": "property category", name: "PROPERTY CATEGORY",
        description: "classification of property within GSRS", required: false,
        opPromise: CVHelper.getTermList("PROPERTY_TYPE"),
        type: "cv", cvType: "PROPERTY_TYPE"
    })
    .addArgument({
        "key": "low value", name: "LOW VALUE", description: "Minimum of the value range", required: false
    })
    .addArgument({
        "key": "high value", name: "HIGH VALUE", description: "Maximum of the value range", required: false
    })
    .addArgument({
        "key": "average", name: "AVERAGE",
        description: "Middle of the value range", required: false
    })
    .addArgument({
        "key": "text value", name: "TEXT VALUE",
        description: "String to assign to the property", required: false
    })
    .addArgument({
        "key": "units", name: "UNITS",
        description: "Unit of measure for this value", required: false,
        defaultValue: "Liters/Kilogram"
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: false,
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        type: "cv",
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: true
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL", description: "URL for the reference",
        required: false
    })
    .addArgument({
        "key": "pd", name: "PD",
        description: "Public Domain status of the property (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "reference tags", name: "REFERENCE TAGS",
        description: "pipe-delimited set of tags for the reference", required: false
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Adding a value to a selected property",
        description: "Text for the record change", required: false
    })
    .addValidator(validate4Params)
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var bdnum = args.bdnum.getValue();
        var lowValue = args["low value"].getValue();
        var highValue = args["high value"].getValue();
        var averageValue = args["average"].getValue();
        var units = args.units.getValue();
        var referenceType = args['reference type'].getValue();
        var referenceCitation = args['reference citation'].getValue();
        var referenceUrl = args['reference url'].getValue();
        var referenceTags = args['reference tags'].getValue();
        var dataPublic = args.pd.isYessy();
        var propertyName = args['property name'].getValue();
        var category = args['property category'].getValue();
        var textValue = args['text value'].getValue();

        var reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
        if (referenceUrl && referenceUrl.length > 0) {
            reference = reference.setUrl(referenceUrl);
        }
        if (dataPublic) {
            reference.setPublic(true);
            reference.setPublicDomain(true);
        } else {
            reference.setPublic(false);
            reference.setPublicDomain(false);
        }
        if (referenceTags && referenceTags.length > 0) {
            var tags = referenceTags.split("|");
            var tagSet = [];
            _.forEach(tags, function (tag) {
                tagSet.push(tag);
            });
            reference.tags = tagSet;
        }

        var s0;
        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                lookupCriterion = pt;
            }
            else {
                lookupCriterion = bdnum;
            }
        }

        var prop = Property.builder().setName(propertyName);
        prop.setType(category);
        if (!isNaN(parseFloat(lowValue))) {
            prop.setLow(lowValue);
            console.log('set low value: ' + lowValue);
        }
        else {
            console.log('omitted low value');
        }
        if (!isNaN(parseFloat(highValue))) {
            prop.setHigh(highValue);
            console.log('set high value: ' + highValue);
        }
        else {
            console.log('omitted high value');
        }
        if (!isNaN(parseFloat(averageValue))) {
            prop.setAverage(averageValue);
            console.log('set avg value: ' + averageValue);
        }
        else {
            console.log('omitted avg value');
        }
        if (units) prop.setUnits(units);
        if (textValue) prop.setPropertyStringValue(textValue);

        if (!dataPublic) prop.setAccess(["restricted"]);

        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (s) {
                var substance;
                var rec = s.content[0]; /*can be undefined... todo: handle*/
                substance = GGlob.SubstanceBuilder.fromSimple(rec);
                s0 = substance;
                return substance.full();
            })
            .andThen(function (s) {
                return s0.fetch("references")
                    .andThen(function (refs) {
                        _.forEach(refs, function (ref) {
                            if (Reference.isDuplicate(ref, referenceType, referenceCitation, referenceUrl)) {
                                console.log('Duplicate reference found! Will skip creation of new one.');
                                reference = ref;
                                return false;
                            }
                        });
                        if (reference) {
                            prop.addReference(reference);
                        }
                        return s0.patch()
                            .addData(prop)
                            .add("/changeReason", args['change reason'].getValue())
                            .apply()
                            .andThen(_.identity);
                    });
            });
    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });

/*Add a reference directly to a Substance (not connected to a lower-level connection)*/
//Script.builder().mix({ name: "Add Reference", description: "Add a reference to a substance record" })
//    .addArgument({
//        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false
//    })
//    .addArgument({
//        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)", required: false
//    })
//    .addArgument({
//        "key": "bdnum", name: "BDNUM", description: "BDNUM of the record (used for validation)", required: false
//    })
//    .addArgument({
//        "key": "reference type", name: "REFERENCE TYPE",
//        description: "Type of reference (must match a vocabulary)",
//        defaultValue: "SYSTEM", required: false,
//        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
//        type: "cv",
//        cvType: "DOCUMENT_TYPE"
//    })
//    .addArgument({
//        "key": "reference citation", name: "REFERENCE CITATION",
//        description: "Citation text for reference", required: true
//    })
//    .addArgument({
//        "key": "reference url", name: "REFERENCE URL", description: "URL for the reference",
//        required: false
//    })
//    .addArgument({
//        "key": "pd", name: "PD",
//        description: "Public Domain status of the reference",
//        defaultValue: false, required: false, type: "boolean"
//    })
//    .addArgument({
//        "key": "access", name: "ACCESS",
//        description: "Access tag for the reference",
//        opPromise: CVHelper.getTermList("ACCESS_GROUP"),
//        type: "cv",
//        cvType: "ACCESS_GROUP",
//        allowCVOverride: true
//    })
//    .addArgument({
//        "key": "reference file path", name: "REFERENCE FILE PATH",
//        description: "A file to attach to the reference", required: false
//    })
//    .addArgument({
//        "key": "reference tags", name: "REFERENCE TAGS",
//        description: "pipe-delimited set of tags for the reference", required: false
//    })
//    .addArgument({
//        "key": "change reason", name: "CHANGE REASON", defaultValue: "Adding a reference to a property",
//        description: "Text for the record change", required: false
//    })
//    .addValidator(validate4Params)
//    .setExecutor(function (args) {
//        var uuid = args.uuid.getValue();
//        var pt = args.pt.getValue();
//        var bdnum = args.bdnum.getValue();

//        var referenceType = args['reference type'].getValue();
//        var referenceCitation = args['reference citation'].getValue();
//        var referenceUrl = args['reference url'].getValue();
//        var referenceTags = args['reference tags'].getValue();
//        var dataPublic = args.pd.isYessy();
//        var referenceAccess = args['access'].getValue();
//        var referenceFilePath = args['reference file path'].getValue();

//        var reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
//        if (referenceUrl && referenceUrl.length > 0) {
//            reference = reference.setUrl(referenceUrl);
//        }
//        if (dataPublic) {
//            reference.setPublic(true);
//            reference.setPublicDomain(true);
//        } else {
//            reference.setPublic(false);
//            reference.setPublicDomain(false);
//        }
//        if (referenceTags && referenceTags.length > 0) {
//            var tags = referenceTags.split("|");
//            var tagSet = [];
//            _.forEach(tags, function (tag) {
//                tagSet.push(tag);
//            });
//            reference.tags = tagSet;
//        }
//        if (referenceFilePath && referenceFilePath.length > 0) {
//            reference.setUploadedFile(referenceFilePath);
//            console.log('adding uploaded file to reference');
//        }

//        var s0;
//        var lookupCriterion = uuid;
//        if (!uuid || uuid.length === 0) {
//            if (pt && pt.length > 0) {
//                lookupCriterion = pt;
//            }
//            else {
//                lookupCriterion = bdnum;
//            }
//        }
//        if (referenceAccess) reference.setAccess([referenceAccess]);

//        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
//            .andThen(function (s) {
//                var substance;
//                var rec = s.content[0]; /*can be undefined... todo: handle*/
//                substance = GGlob.SubstanceBuilder.fromSimple(rec);
//                s0 = substance;
//                return substance.full();
//            })
//            .andThen(function (s) {
//                return s0.fetch("references")
//                    .andThen(function (refs) {
//                        _.forEach(refs, function (ref) {
//                            if (Reference.isDuplicate(ref, referenceType, referenceCitation, referenceUrl)) {
//                                console.log('Duplicate reference found! Will skip creation of new one.');
//                                reference = ref;
//                                return false;
//                            }
//                        });
//                        if (reference) {
//                            return s0.patch()
//                                .addData(reference)
//                                .add("/changeReason", args['change reason'].getValue())
//                                .apply()
//                                .andThen(_.identity);
//                        }
//                    });
//            });
//    })
//    .useFor(function (s) {
//        Scripts.addScript(s);
//    });

Script.builder().mix({
    name: "Save Temporary Structure", description: "Saves a molfile or SMILES in a temporary area (disappears after service restart)",
    validForSheetCreation: false
})
    .addArgument({
        "key": "molfile", name: "Molfile", description: "structure to save", required: true
    })
    .setExecutor(function (args) {
        var structure = args.molfile.getValue();
        return GGlob.SubstanceFinder.saveTemporaryStructure(structure)
            .andThen(function (s) {
                console.log("saveTemporaryStructure script received s: " + JSON.stringify(s));
                if (typeof s === 'string' && s.indexOf('<html>') > -1) {
                    return "Error: not authenticated";
                }
                if (typeof s === 'object' && (!s.valid && !s.structure)) {
                    console.log('detected error');
                    if (s.message) return s.message;
                    else return "an error occurred";
                }
                console.log('going to return s.structure.id ' + s.structure.id);
                return SubstanceFinder.getExactStructureMatches(s.structure.id)
                    .andThen(function (searchResult) {
                        console.log('searchResult: ' + JSON.stringify(searchResult));
                        var msg = {
                            valid: true, message: "structureid=" + s.structure.id,
                            matches: searchResult.content
                        };
                        return msg;
                    });
            });

    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });

Script.builder().mix({ name: "Add Note", description: "Adds a note to a substance record" })
    .addArgument({
        "key": "uuid", name: "UUID", description: "UUID of the substance record", required: false,
        usedForLookup: true
    })
    .addArgument({
        "key": "pt", name: "PT", description: "Preferred Term of the record (used for validation)",
        required: false, usedForLookup: true
    })
    .addArgument({
        "key": "bdnum", name: "BDNUM",
        description: "BDNUM of the record (used for validation)", required: false, usedForLookup: true
    })
    .addArgument({
        "key": "note", name: "NOTE", description: "Note text of the new note item",
        required: true
    })
    .addArgument({
        "key": "pd", name: "PD", description: "Public Domain status of the name (sets access for reference as well)",
        defaultValue: false, required: false, type: "boolean"
    })
    .addArgument({
        "key": "reference type", name: "REFERENCE TYPE",
        description: "Type of reference (must match a vocabulary)",
        defaultValue: "SYSTEM", required: false,
        type: "cv",
        opPromise: CVHelper.getTermList("DOCUMENT_TYPE"),
        cvType: "DOCUMENT_TYPE"
    })
    .addArgument({
        "key": "reference citation", name: "REFERENCE CITATION",
        description: "Citation text for reference", required: true
    })
    .addArgument({
        "key": "reference url", name: "REFERENCE URL",
        description: "URL for the reference", required: false
    })
    .addArgument({
        "key": "change reason", name: "CHANGE REASON", defaultValue: "Added Note",
        description: "Text for the record change", required: false
    })
    .addValidator(validate4Params,null)
    .setExecutor(function (args) {
        var uuid = args.uuid.getValue();
        var pt = args.pt.getValue();
        var bdnum = args.bdnum.getValue();
        var note = args.note.getValue();

        var dataPublic = args.pd.isYessy();
        var referenceType = args["reference type"].getValue();
        var referenceCitation = args["reference citation"].getValue();
        var referenceUrl = args['reference url'].getValue();

        var reference = Reference.builder().mix({ citation: referenceCitation, docType: referenceType });
        if (referenceUrl && referenceUrl.length > 0) {
            console.log('setting file URL');
            reference = reference.setUploadFileUrl(referenceUrl);
        }

        if (dataPublic) {
            console.log('perceived public reference');
            reference.setPublic(true);
            reference.setPublicDomain(true);
        } else {
            console.log('perceived NON public reference');
            reference.setPublic(false);
            reference.setPublicDomain(false);
        }

        var noteObject = Note.builder().setNote(note)
            .setPublic(dataPublic)
        var lookupCriterion = uuid;
        if (!uuid || uuid.length === 0) {
            if (pt && pt.length > 0) {
                lookupCriterion = pt;
            }
            else {
                lookupCriterion = bdnum;
            }
        }
        return GGlob.SubstanceFinder.searchByExactNameOrCode(lookupCriterion)
            .andThen(function (s) {
                var substance;
                var rec = s.content[0]; /*can be undefined... todo: handle*/
                substance = GGlob.SubstanceBuilder.fromSimple(rec);

                substanceForPatch = substance;
                return substance.fetch("references")
                    .andThen(function (refs) {
                        _.forEach(refs, function (ref) {
                            if (Reference.isDuplicate(ref, referenceType, referenceCitation, referenceUrl)) {
                                console.log('Duplicate reference found! Will skip creation of new one.');
                                reference = ref;
                                return false;
                            }
                        });
                        noteObject.addReference(reference);
                        return substance;
                    })
                    .andThen(function (s2) {
                        return substance.patch()
                            .addData(noteObject)
                            .add("/changeReason", args['change reason'].getValue())
                            .apply()
                            .andThen(_.identity);
                    });
            });

    })
    .useFor(function (s) {
        Scripts.addScript(s);
    });
