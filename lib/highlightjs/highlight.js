/*!
  Highlight.js v11.4.0 (git: 2d0e7c1094)
  (c) 2006-2022 Ivan Sagalaev and other contributors
  License: BSD-3-Clause
 */
  var hljs=function(){"use strict";var e={exports:{}};function t(e){
    return e instanceof Map?e.clear=e.delete=e.set=()=>{
    throw Error("map is read-only")}:e instanceof Set&&(e.add=e.clear=e.delete=()=>{
    throw Error("set is read-only")
    }),Object.freeze(e),Object.getOwnPropertyNames(e).forEach((n=>{var i=e[n]
    ;"object"!=typeof i||Object.isFrozen(i)||t(i)})),e}
    e.exports=t,e.exports.default=t;var n=e.exports;class i{constructor(e){
    void 0===e.data&&(e.data={}),this.data=e.data,this.isMatchIgnored=!1}
    ignoreMatch(){this.isMatchIgnored=!0}}function r(e){
    return e.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;").replace(/'/g,"&#x27;")
    }function s(e,...t){const n=Object.create(null);for(const t in e)n[t]=e[t]
    ;return t.forEach((e=>{for(const t in e)n[t]=e[t]})),n}const o=e=>!!e.kind
    ;class a{constructor(e,t){
    this.buffer="",this.classPrefix=t.classPrefix,e.walk(this)}addText(e){
    this.buffer+=r(e)}openNode(e){if(!o(e))return;let t=e.kind
    ;t=e.sublanguage?"language-"+t:((e,{prefix:t})=>{if(e.includes(".")){
    const n=e.split(".")
    ;return[`${t}${n.shift()}`,...n.map(((e,t)=>`${e}${"_".repeat(t+1)}`))].join(" ")
    }return`${t}${e}`})(t,{prefix:this.classPrefix}),this.span(t)}closeNode(e){
    o(e)&&(this.buffer+="</span>")}value(){return this.buffer}span(e){
    this.buffer+=`<span class="${e}">`}}class c{constructor(){this.rootNode={
    children:[]},this.stack=[this.rootNode]}get top(){
    return this.stack[this.stack.length-1]}get root(){return this.rootNode}add(e){
    this.top.children.push(e)}openNode(e){const t={kind:e,children:[]}
    ;this.add(t),this.stack.push(t)}closeNode(){
    if(this.stack.length>1)return this.stack.pop()}closeAllNodes(){
    for(;this.closeNode(););}toJSON(){return JSON.stringify(this.rootNode,null,4)}
    walk(e){return this.constructor._walk(e,this.rootNode)}static _walk(e,t){
    return"string"==typeof t?e.addText(t):t.children&&(e.openNode(t),
    t.children.forEach((t=>this._walk(e,t))),e.closeNode(t)),e}static _collapse(e){
    "string"!=typeof e&&e.children&&(e.children.every((e=>"string"==typeof e))?e.children=[e.children.join("")]:e.children.forEach((e=>{
    c._collapse(e)})))}}class l extends c{constructor(e){super(),this.options=e}
    addKeyword(e,t){""!==e&&(this.openNode(t),this.addText(e),this.closeNode())}
    addText(e){""!==e&&this.add(e)}addSublanguage(e,t){const n=e.root
    ;n.kind=t,n.sublanguage=!0,this.add(n)}toHTML(){
    return new a(this,this.options).value()}finalize(){return!0}}function g(e){
    return e?"string"==typeof e?e:e.source:null}function d(e){return f("(?=",e,")")}
    function u(e){return f("(?:",e,")*")}function h(e){return f("(?:",e,")?")}
    function f(...e){return e.map((e=>g(e))).join("")}function p(...e){const t=(e=>{
    const t=e[e.length-1]
    ;return"object"==typeof t&&t.constructor===Object?(e.splice(e.length-1,1),t):{}
    })(e);return"("+(t.capture?"":"?:")+e.map((e=>g(e))).join("|")+")"}
    function b(e){return RegExp(e.toString()+"|").exec("").length-1}
    const m=/\[(?:[^\\\]]|\\.)*\]|\(\??|\\([1-9][0-9]*)|\\./
    ;function E(e,{joinWith:t}){let n=0;return e.map((e=>{n+=1;const t=n
    ;let i=g(e),r="";for(;i.length>0;){const e=m.exec(i);if(!e){r+=i;break}
    r+=i.substring(0,e.index),
    i=i.substring(e.index+e[0].length),"\\"===e[0][0]&&e[1]?r+="\\"+(Number(e[1])+t):(r+=e[0],
    "("===e[0]&&n++)}return r})).map((e=>`(${e})`)).join(t)}
    const x="[a-zA-Z]\\w*",w="[a-zA-Z_]\\w*",y="\\b\\d+(\\.\\d+)?",_="(-?)(\\b0[xX][a-fA-F0-9]+|(\\b\\d+(\\.\\d*)?|\\.\\d+)([eE][-+]?\\d+)?)",v="\\b(0b[01]+)",k={
    begin:"\\\\[\\s\\S]",relevance:0},O={scope:"string",begin:"'",end:"'",
    illegal:"\\n",contains:[k]},N={scope:"string",begin:'"',end:'"',illegal:"\\n",
    contains:[k]},M=(e,t,n={})=>{const i=s({scope:"comment",begin:e,end:t,
    contains:[]},n);i.contains.push({scope:"doctag",
    begin:"[ ]*(?=(TODO|FIXME|NOTE|BUG|OPTIMIZE|HACK|XXX):)",
    end:/(TODO|FIXME|NOTE|BUG|OPTIMIZE|HACK|XXX):/,excludeBegin:!0,relevance:0})
    ;const r=p("I","a","is","so","us","to","at","if","in","it","on",/[A-Za-z]+['](d|ve|re|ll|t|s|n)/,/[A-Za-z]+[-][a-z]+/,/[A-Za-z][a-z]{2,}/)
    ;return i.contains.push({begin:f(/[ ]+/,"(",r,/[.]?[:]?([.][ ]|[ ])/,"){3}")}),i
    },S=M("//","$"),R=M("/\\*","\\*/"),j=M("#","$");var A=Object.freeze({
    __proto__:null,MATCH_NOTHING_RE:/\b\B/,IDENT_RE:x,UNDERSCORE_IDENT_RE:w,
    NUMBER_RE:y,C_NUMBER_RE:_,BINARY_NUMBER_RE:v,
    RE_STARTERS_RE:"!|!=|!==|%|%=|&|&&|&=|\\*|\\*=|\\+|\\+=|,|-|-=|/=|/|:|;|<<|<<=|<=|<|===|==|=|>>>=|>>=|>=|>>>|>>|>|\\?|\\[|\\{|\\(|\\^|\\^=|\\||\\|=|\\|\\||~",
    SHEBANG:(e={})=>{const t=/^#![ ]*\//
    ;return e.binary&&(e.begin=f(t,/.*\b/,e.binary,/\b.*/)),s({scope:"meta",begin:t,
    end:/$/,relevance:0,"on:begin":(e,t)=>{0!==e.index&&t.ignoreMatch()}},e)},
    BACKSLASH_ESCAPE:k,APOS_STRING_MODE:O,QUOTE_STRING_MODE:N,PHRASAL_WORDS_MODE:{
    begin:/\b(a|an|the|are|I'm|isn't|don't|doesn't|won't|but|just|should|pretty|simply|enough|gonna|going|wtf|so|such|will|you|your|they|like|more)\b/
    },COMMENT:M,C_LINE_COMMENT_MODE:S,C_BLOCK_COMMENT_MODE:R,HASH_COMMENT_MODE:j,
    NUMBER_MODE:{scope:"number",begin:y,relevance:0},C_NUMBER_MODE:{scope:"number",
    begin:_,relevance:0},BINARY_NUMBER_MODE:{scope:"number",begin:v,relevance:0},
    REGEXP_MODE:{begin:/(?=\/[^/\n]*\/)/,contains:[{scope:"regexp",begin:/\//,
    end:/\/[gimuy]*/,illegal:/\n/,contains:[k,{begin:/\[/,end:/\]/,relevance:0,
    contains:[k]}]}]},TITLE_MODE:{scope:"title",begin:x,relevance:0},
    UNDERSCORE_TITLE_MODE:{scope:"title",begin:w,relevance:0},METHOD_GUARD:{
    begin:"\\.\\s*[a-zA-Z_]\\w*",relevance:0},END_SAME_AS_BEGIN:e=>Object.assign(e,{
    "on:begin":(e,t)=>{t.data._beginMatch=e[1]},"on:end":(e,t)=>{
    t.data._beginMatch!==e[1]&&t.ignoreMatch()}})});function I(e,t){
    "."===e.input[e.index-1]&&t.ignoreMatch()}function T(e,t){
    void 0!==e.className&&(e.scope=e.className,delete e.className)}function L(e,t){
    t&&e.beginKeywords&&(e.begin="\\b("+e.beginKeywords.split(" ").join("|")+")(?!\\.)(?=\\b|\\s)",
    e.__beforeBegin=I,e.keywords=e.keywords||e.beginKeywords,delete e.beginKeywords,
    void 0===e.relevance&&(e.relevance=0))}function B(e,t){
    Array.isArray(e.illegal)&&(e.illegal=p(...e.illegal))}function D(e,t){
    if(e.match){
    if(e.begin||e.end)throw Error("begin & end are not supported with match")
    ;e.begin=e.match,delete e.match}}function H(e,t){
    void 0===e.relevance&&(e.relevance=1)}const P=(e,t)=>{if(!e.beforeMatch)return
    ;if(e.starts)throw Error("beforeMatch cannot be used with starts")
    ;const n=Object.assign({},e);Object.keys(e).forEach((t=>{delete e[t]
    })),e.keywords=n.keywords,e.begin=f(n.beforeMatch,d(n.begin)),e.starts={
    relevance:0,contains:[Object.assign(n,{endsParent:!0})]
    },e.relevance=0,delete n.beforeMatch
    },C=["of","and","for","in","not","or","if","then","parent","list","value"]
    ;function $(e,t,n="keyword"){const i=Object.create(null)
    ;return"string"==typeof e?r(n,e.split(" ")):Array.isArray(e)?r(n,e):Object.keys(e).forEach((n=>{
    Object.assign(i,$(e[n],t,n))})),i;function r(e,n){
    t&&(n=n.map((e=>e.toLowerCase()))),n.forEach((t=>{const n=t.split("|")
    ;i[n[0]]=[e,U(n[0],n[1])]}))}}function U(e,t){
    return t?Number(t):(e=>C.includes(e.toLowerCase()))(e)?0:1}const z={},K=e=>{
    console.error(e)},W=(e,...t)=>{console.log("WARN: "+e,...t)},X=(e,t)=>{
    z[`${e}/${t}`]||(console.log(`Deprecated as of ${e}. ${t}`),z[`${e}/${t}`]=!0)
    },G=Error();function Z(e,t,{key:n}){let i=0;const r=e[n],s={},o={}
    ;for(let e=1;e<=t.length;e++)o[e+i]=r[e],s[e+i]=!0,i+=b(t[e-1])
    ;e[n]=o,e[n]._emit=s,e[n]._multi=!0}function F(e){(e=>{
    e.scope&&"object"==typeof e.scope&&null!==e.scope&&(e.beginScope=e.scope,
    delete e.scope)})(e),"string"==typeof e.beginScope&&(e.beginScope={
    _wrap:e.beginScope}),"string"==typeof e.endScope&&(e.endScope={_wrap:e.endScope
    }),(e=>{if(Array.isArray(e.begin)){
    if(e.skip||e.excludeBegin||e.returnBegin)throw K("skip, excludeBegin, returnBegin not compatible with beginScope: {}"),
    G
    ;if("object"!=typeof e.beginScope||null===e.beginScope)throw K("beginScope must be object"),
    G;Z(e,e.begin,{key:"beginScope"}),e.begin=E(e.begin,{joinWith:""})}})(e),(e=>{
    if(Array.isArray(e.end)){
    if(e.skip||e.excludeEnd||e.returnEnd)throw K("skip, excludeEnd, returnEnd not compatible with endScope: {}"),
    G
    ;if("object"!=typeof e.endScope||null===e.endScope)throw K("endScope must be object"),
    G;Z(e,e.end,{key:"endScope"}),e.end=E(e.end,{joinWith:""})}})(e)}function V(e){
    function t(t,n){
    return RegExp(g(t),"m"+(e.case_insensitive?"i":"")+(e.unicodeRegex?"u":"")+(n?"g":""))
    }class n{constructor(){
    this.matchIndexes={},this.regexes=[],this.matchAt=1,this.position=0}
    addRule(e,t){
    t.position=this.position++,this.matchIndexes[this.matchAt]=t,this.regexes.push([t,e]),
    this.matchAt+=b(e)+1}compile(){0===this.regexes.length&&(this.exec=()=>null)
    ;const e=this.regexes.map((e=>e[1]));this.matcherRe=t(E(e,{joinWith:"|"
    }),!0),this.lastIndex=0}exec(e){this.matcherRe.lastIndex=this.lastIndex
    ;const t=this.matcherRe.exec(e);if(!t)return null
    ;const n=t.findIndex(((e,t)=>t>0&&void 0!==e)),i=this.matchIndexes[n]
    ;return t.splice(0,n),Object.assign(t,i)}}class i{constructor(){
    this.rules=[],this.multiRegexes=[],
    this.count=0,this.lastIndex=0,this.regexIndex=0}getMatcher(e){
    if(this.multiRegexes[e])return this.multiRegexes[e];const t=new n
    ;return this.rules.slice(e).forEach((([e,n])=>t.addRule(e,n))),
    t.compile(),this.multiRegexes[e]=t,t}resumingScanAtSamePosition(){
    return 0!==this.regexIndex}considerAll(){this.regexIndex=0}addRule(e,t){
    this.rules.push([e,t]),"begin"===t.type&&this.count++}exec(e){
    const t=this.getMatcher(this.regexIndex);t.lastIndex=this.lastIndex
    ;let n=t.exec(e)
    ;if(this.resumingScanAtSamePosition())if(n&&n.index===this.lastIndex);else{
    const t=this.getMatcher(0);t.lastIndex=this.lastIndex+1,n=t.exec(e)}
    return n&&(this.regexIndex+=n.position+1,
    this.regexIndex===this.count&&this.considerAll()),n}}
    if(e.compilerExtensions||(e.compilerExtensions=[]),
    e.contains&&e.contains.includes("self"))throw Error("ERR: contains `self` is not supported at the top-level of a language.  See documentation.")
    ;return e.classNameAliases=s(e.classNameAliases||{}),function n(r,o){const a=r
    ;if(r.isCompiled)return a
    ;[T,D,F,P].forEach((e=>e(r,o))),e.compilerExtensions.forEach((e=>e(r,o))),
    r.__beforeBegin=null,[L,B,H].forEach((e=>e(r,o))),r.isCompiled=!0;let c=null
    ;return"object"==typeof r.keywords&&r.keywords.$pattern&&(r.keywords=Object.assign({},r.keywords),
    c=r.keywords.$pattern,
    delete r.keywords.$pattern),c=c||/\w+/,r.keywords&&(r.keywords=$(r.keywords,e.case_insensitive)),
    a.keywordPatternRe=t(c,!0),
    o&&(r.begin||(r.begin=/\B|\b/),a.beginRe=t(a.begin),r.end||r.endsWithParent||(r.end=/\B|\b/),
    r.end&&(a.endRe=t(a.end)),
    a.terminatorEnd=g(a.end)||"",r.endsWithParent&&o.terminatorEnd&&(a.terminatorEnd+=(r.end?"|":"")+o.terminatorEnd)),
    r.illegal&&(a.illegalRe=t(r.illegal)),
    r.contains||(r.contains=[]),r.contains=[].concat(...r.contains.map((e=>(e=>(e.variants&&!e.cachedVariants&&(e.cachedVariants=e.variants.map((t=>s(e,{
    variants:null},t)))),e.cachedVariants?e.cachedVariants:q(e)?s(e,{
    starts:e.starts?s(e.starts):null
    }):Object.isFrozen(e)?s(e):e))("self"===e?r:e)))),r.contains.forEach((e=>{n(e,a)
    })),r.starts&&n(r.starts,o),a.matcher=(e=>{const t=new i
    ;return e.contains.forEach((e=>t.addRule(e.begin,{rule:e,type:"begin"
    }))),e.terminatorEnd&&t.addRule(e.terminatorEnd,{type:"end"
    }),e.illegal&&t.addRule(e.illegal,{type:"illegal"}),t})(a),a}(e)}function q(e){
    return!!e&&(e.endsWithParent||q(e.starts))}class J extends Error{
    constructor(e,t){super(e),this.name="HTMLInjectionError",this.html=t}}
    const Y=r,Q=s,ee=Symbol("nomatch");var te=(e=>{
    const t=Object.create(null),r=Object.create(null),s=[];let o=!0
    ;const a="Could not find the language '{}', did you forget to load/include a language module?",c={
    disableAutodetect:!0,name:"Plain text",contains:[]};let g={
    ignoreUnescapedHTML:!1,throwUnescapedHTML:!1,noHighlightRe:/^(no-?highlight)$/i,
    languageDetectRe:/\blang(?:uage)?-([\w-]+)\b/i,classPrefix:"hljs-",
    cssSelector:"pre code",languages:null,__emitter:l};function b(e){
    return g.noHighlightRe.test(e)}function m(e,t,n){let i="",r=""
    ;"object"==typeof t?(i=e,
    n=t.ignoreIllegals,r=t.language):(X("10.7.0","highlight(lang, code, ...args) has been deprecated."),
    X("10.7.0","Please use highlight(code, options) instead.\nhttps://github.com/highlightjs/highlight.js/issues/2277"),
    r=e,i=t),void 0===n&&(n=!0);const s={code:i,language:r};N("before:highlight",s)
    ;const o=s.result?s.result:E(s.language,s.code,n)
    ;return o.code=s.code,N("after:highlight",o),o}function E(e,n,r,s){
    const c=Object.create(null);function l(){if(!O.keywords)return void M.addText(S)
    ;let e=0;O.keywordPatternRe.lastIndex=0;let t=O.keywordPatternRe.exec(S),n=""
    ;for(;t;){n+=S.substring(e,t.index)
    ;const r=y.case_insensitive?t[0].toLowerCase():t[0],s=(i=r,O.keywords[i]);if(s){
    const[e,i]=s
    ;if(M.addText(n),n="",c[r]=(c[r]||0)+1,c[r]<=7&&(R+=i),e.startsWith("_"))n+=t[0];else{
    const n=y.classNameAliases[e]||e;M.addKeyword(t[0],n)}}else n+=t[0]
    ;e=O.keywordPatternRe.lastIndex,t=O.keywordPatternRe.exec(S)}var i
    ;n+=S.substr(e),M.addText(n)}function d(){null!=O.subLanguage?(()=>{
    if(""===S)return;let e=null;if("string"==typeof O.subLanguage){
    if(!t[O.subLanguage])return void M.addText(S)
    ;e=E(O.subLanguage,S,!0,N[O.subLanguage]),N[O.subLanguage]=e._top
    }else e=x(S,O.subLanguage.length?O.subLanguage:null)
    ;O.relevance>0&&(R+=e.relevance),M.addSublanguage(e._emitter,e.language)
    })():l(),S=""}function u(e,t){let n=1;for(;void 0!==t[n];){if(!e._emit[n]){n++
    ;continue}const i=y.classNameAliases[e[n]]||e[n],r=t[n]
    ;i?M.addKeyword(r,i):(S=r,l(),S=""),n++}}function h(e,t){
    return e.scope&&"string"==typeof e.scope&&M.openNode(y.classNameAliases[e.scope]||e.scope),
    e.beginScope&&(e.beginScope._wrap?(M.addKeyword(S,y.classNameAliases[e.beginScope._wrap]||e.beginScope._wrap),
    S=""):e.beginScope._multi&&(u(e.beginScope,t),S="")),O=Object.create(e,{parent:{
    value:O}}),O}function f(e,t,n){let r=((e,t)=>{const n=e&&e.exec(t)
    ;return n&&0===n.index})(e.endRe,n);if(r){if(e["on:end"]){const n=new i(e)
    ;e["on:end"](t,n),n.isMatchIgnored&&(r=!1)}if(r){
    for(;e.endsParent&&e.parent;)e=e.parent;return e}}
    if(e.endsWithParent)return f(e.parent,t,n)}function p(e){
    return 0===O.matcher.regexIndex?(S+=e[0],1):(I=!0,0)}function b(e){
    const t=e[0],i=n.substr(e.index),r=f(O,e,i);if(!r)return ee;const s=O
    ;O.endScope&&O.endScope._wrap?(d(),
    M.addKeyword(t,O.endScope._wrap)):O.endScope&&O.endScope._multi?(d(),
    u(O.endScope,e)):s.skip?S+=t:(s.returnEnd||s.excludeEnd||(S+=t),
    d(),s.excludeEnd&&(S=t));do{
    O.scope&&M.closeNode(),O.skip||O.subLanguage||(R+=O.relevance),O=O.parent
    }while(O!==r.parent);return r.starts&&h(r.starts,e),s.returnEnd?0:t.length}
    let m={};function w(t,s){const a=s&&s[0];if(S+=t,null==a)return d(),0
    ;if("begin"===m.type&&"end"===s.type&&m.index===s.index&&""===a){
    if(S+=n.slice(s.index,s.index+1),!o){const t=Error(`0 width match regex (${e})`)
    ;throw t.languageName=e,t.badRule=m.rule,t}return 1}
    if(m=s,"begin"===s.type)return(e=>{
    const t=e[0],n=e.rule,r=new i(n),s=[n.__beforeBegin,n["on:begin"]]
    ;for(const n of s)if(n&&(n(e,r),r.isMatchIgnored))return p(t)
    ;return n.skip?S+=t:(n.excludeBegin&&(S+=t),
    d(),n.returnBegin||n.excludeBegin||(S=t)),h(n,e),n.returnBegin?0:t.length})(s)
    ;if("illegal"===s.type&&!r){
    const e=Error('Illegal lexeme "'+a+'" for mode "'+(O.scope||"<unnamed>")+'"')
    ;throw e.mode=O,e}if("end"===s.type){const e=b(s);if(e!==ee)return e}
    if("illegal"===s.type&&""===a)return 1
    ;if(A>1e5&&A>3*s.index)throw Error("potential infinite loop, way more iterations than matches")
    ;return S+=a,a.length}const y=v(e)
    ;if(!y)throw K(a.replace("{}",e)),Error('Unknown language: "'+e+'"')
    ;const _=V(y);let k="",O=s||_;const N={},M=new g.__emitter(g);(()=>{const e=[]
    ;for(let t=O;t!==y;t=t.parent)t.scope&&e.unshift(t.scope)
    ;e.forEach((e=>M.openNode(e)))})();let S="",R=0,j=0,A=0,I=!1;try{
    for(O.matcher.considerAll();;){
    A++,I?I=!1:O.matcher.considerAll(),O.matcher.lastIndex=j
    ;const e=O.matcher.exec(n);if(!e)break;const t=w(n.substring(j,e.index),e)
    ;j=e.index+t}return w(n.substr(j)),M.closeAllNodes(),M.finalize(),k=M.toHTML(),{
    language:e,value:k,relevance:R,illegal:!1,_emitter:M,_top:O}}catch(t){
    if(t.message&&t.message.includes("Illegal"))return{language:e,value:Y(n),
    illegal:!0,relevance:0,_illegalBy:{message:t.message,index:j,
    context:n.slice(j-100,j+100),mode:t.mode,resultSoFar:k},_emitter:M};if(o)return{
    language:e,value:Y(n),illegal:!1,relevance:0,errorRaised:t,_emitter:M,_top:O}
    ;throw t}}function x(e,n){n=n||g.languages||Object.keys(t);const i=(e=>{
    const t={value:Y(e),illegal:!1,relevance:0,_top:c,_emitter:new g.__emitter(g)}
    ;return t._emitter.addText(e),t})(e),r=n.filter(v).filter(O).map((t=>E(t,e,!1)))
    ;r.unshift(i);const s=r.sort(((e,t)=>{
    if(e.relevance!==t.relevance)return t.relevance-e.relevance
    ;if(e.language&&t.language){if(v(e.language).supersetOf===t.language)return 1
    ;if(v(t.language).supersetOf===e.language)return-1}return 0})),[o,a]=s,l=o
    ;return l.secondBest=a,l}function w(e){let t=null;const n=(e=>{
    let t=e.className+" ";t+=e.parentNode?e.parentNode.className:""
    ;const n=g.languageDetectRe.exec(t);if(n){const t=v(n[1])
    ;return t||(W(a.replace("{}",n[1])),
    W("Falling back to no-highlight mode for this block.",e)),t?n[1]:"no-highlight"}
    return t.split(/\s+/).find((e=>b(e)||v(e)))})(e);if(b(n))return
    ;if(N("before:highlightElement",{el:e,language:n
    }),e.children.length>0&&(g.ignoreUnescapedHTML||(console.warn("One of your code blocks includes unescaped HTML. This is a potentially serious security risk."),
    console.warn("https://github.com/highlightjs/highlight.js/wiki/security"),
    console.warn("The element with unescaped HTML:"),
    console.warn(e)),g.throwUnescapedHTML))throw new J("One of your code blocks includes unescaped HTML.",e.innerHTML)
    ;t=e;const i=t.textContent,s=n?m(i,{language:n,ignoreIllegals:!0}):x(i)
    ;e.innerHTML=s.value,((e,t,n)=>{const i=t&&r[t]||n
    ;e.classList.add("hljs"),e.classList.add("language-"+i)
    })(e,n,s.language),e.result={language:s.language,re:s.relevance,
    relevance:s.relevance},s.secondBest&&(e.secondBest={
    language:s.secondBest.language,relevance:s.secondBest.relevance
    }),N("after:highlightElement",{el:e,result:s,text:i})}let y=!1;function _(){
    "loading"!==document.readyState?document.querySelectorAll(g.cssSelector).forEach(w):y=!0
    }function v(e){return e=(e||"").toLowerCase(),t[e]||t[r[e]]}
    function k(e,{languageName:t}){"string"==typeof e&&(e=[e]),e.forEach((e=>{
    r[e.toLowerCase()]=t}))}function O(e){const t=v(e)
    ;return t&&!t.disableAutodetect}function N(e,t){const n=e;s.forEach((e=>{
    e[n]&&e[n](t)}))}
    "undefined"!=typeof window&&window.addEventListener&&window.addEventListener("DOMContentLoaded",(()=>{
    y&&_()}),!1),Object.assign(e,{highlight:m,highlightAuto:x,highlightAll:_,
    highlightElement:w,
    highlightBlock:e=>(X("10.7.0","highlightBlock will be removed entirely in v12.0"),
    X("10.7.0","Please use highlightElement now."),w(e)),configure:e=>{g=Q(g,e)},
    initHighlighting:()=>{
    _(),X("10.6.0","initHighlighting() deprecated.  Use highlightAll() now.")},
    initHighlightingOnLoad:()=>{
    _(),X("10.6.0","initHighlightingOnLoad() deprecated.  Use highlightAll() now.")
    },registerLanguage:(n,i)=>{let r=null;try{r=i(e)}catch(e){
    if(K("Language definition for '{}' could not be registered.".replace("{}",n)),
    !o)throw e;K(e),r=c}
    r.name||(r.name=n),t[n]=r,r.rawDefinition=i.bind(null,e),r.aliases&&k(r.aliases,{
    languageName:n})},unregisterLanguage:e=>{delete t[e]
    ;for(const t of Object.keys(r))r[t]===e&&delete r[t]},
    listLanguages:()=>Object.keys(t),getLanguage:v,registerAliases:k,
    autoDetection:O,inherit:Q,addPlugin:e=>{(e=>{
    e["before:highlightBlock"]&&!e["before:highlightElement"]&&(e["before:highlightElement"]=t=>{
    e["before:highlightBlock"](Object.assign({block:t.el},t))
    }),e["after:highlightBlock"]&&!e["after:highlightElement"]&&(e["after:highlightElement"]=t=>{
    e["after:highlightBlock"](Object.assign({block:t.el},t))})})(e),s.push(e)}
    }),e.debugMode=()=>{o=!1},e.safeMode=()=>{o=!0
    },e.versionString="11.4.0",e.regex={concat:f,lookahead:d,either:p,optional:h,
    anyNumberOfTimes:u};for(const e in A)"object"==typeof A[e]&&n(A[e])
    ;return Object.assign(e,A),e})({});return te}()
    ;"object"==typeof exports&&"undefined"!=typeof module&&(module.exports=hljs);/*! `less` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var e=(()=>{"use strict"
    ;const e=["a","abbr","address","article","aside","audio","b","blockquote","body","button","canvas","caption","cite","code","dd","del","details","dfn","div","dl","dt","em","fieldset","figcaption","figure","footer","form","h1","h2","h3","h4","h5","h6","header","hgroup","html","i","iframe","img","input","ins","kbd","label","legend","li","main","mark","menu","nav","object","ol","p","q","quote","samp","section","span","strong","summary","sup","table","tbody","td","textarea","tfoot","th","thead","time","tr","ul","var","video"],t=["any-hover","any-pointer","aspect-ratio","color","color-gamut","color-index","device-aspect-ratio","device-height","device-width","display-mode","forced-colors","grid","height","hover","inverted-colors","monochrome","orientation","overflow-block","overflow-inline","pointer","prefers-color-scheme","prefers-contrast","prefers-reduced-motion","prefers-reduced-transparency","resolution","scan","scripting","update","width","min-width","max-width","min-height","max-height"],i=["active","any-link","blank","checked","current","default","defined","dir","disabled","drop","empty","enabled","first","first-child","first-of-type","fullscreen","future","focus","focus-visible","focus-within","has","host","host-context","hover","indeterminate","in-range","invalid","is","lang","last-child","last-of-type","left","link","local-link","not","nth-child","nth-col","nth-last-child","nth-last-col","nth-last-of-type","nth-of-type","only-child","only-of-type","optional","out-of-range","past","placeholder-shown","read-only","read-write","required","right","root","scope","target","target-within","user-invalid","valid","visited","where"],r=["after","backdrop","before","cue","cue-region","first-letter","first-line","grammar-error","marker","part","placeholder","selection","slotted","spelling-error"],o=["align-content","align-items","align-self","all","animation","animation-delay","animation-direction","animation-duration","animation-fill-mode","animation-iteration-count","animation-name","animation-play-state","animation-timing-function","backface-visibility","background","background-attachment","background-clip","background-color","background-image","background-origin","background-position","background-repeat","background-size","border","border-bottom","border-bottom-color","border-bottom-left-radius","border-bottom-right-radius","border-bottom-style","border-bottom-width","border-collapse","border-color","border-image","border-image-outset","border-image-repeat","border-image-slice","border-image-source","border-image-width","border-left","border-left-color","border-left-style","border-left-width","border-radius","border-right","border-right-color","border-right-style","border-right-width","border-spacing","border-style","border-top","border-top-color","border-top-left-radius","border-top-right-radius","border-top-style","border-top-width","border-width","bottom","box-decoration-break","box-shadow","box-sizing","break-after","break-before","break-inside","caption-side","caret-color","clear","clip","clip-path","clip-rule","color","column-count","column-fill","column-gap","column-rule","column-rule-color","column-rule-style","column-rule-width","column-span","column-width","columns","contain","content","content-visibility","counter-increment","counter-reset","cue","cue-after","cue-before","cursor","direction","display","empty-cells","filter","flex","flex-basis","flex-direction","flex-flow","flex-grow","flex-shrink","flex-wrap","float","flow","font","font-display","font-family","font-feature-settings","font-kerning","font-language-override","font-size","font-size-adjust","font-smoothing","font-stretch","font-style","font-synthesis","font-variant","font-variant-caps","font-variant-east-asian","font-variant-ligatures","font-variant-numeric","font-variant-position","font-variation-settings","font-weight","gap","glyph-orientation-vertical","grid","grid-area","grid-auto-columns","grid-auto-flow","grid-auto-rows","grid-column","grid-column-end","grid-column-start","grid-gap","grid-row","grid-row-end","grid-row-start","grid-template","grid-template-areas","grid-template-columns","grid-template-rows","hanging-punctuation","height","hyphens","icon","image-orientation","image-rendering","image-resolution","ime-mode","isolation","justify-content","left","letter-spacing","line-break","line-height","list-style","list-style-image","list-style-position","list-style-type","margin","margin-bottom","margin-left","margin-right","margin-top","marks","mask","mask-border","mask-border-mode","mask-border-outset","mask-border-repeat","mask-border-slice","mask-border-source","mask-border-width","mask-clip","mask-composite","mask-image","mask-mode","mask-origin","mask-position","mask-repeat","mask-size","mask-type","max-height","max-width","min-height","min-width","mix-blend-mode","nav-down","nav-index","nav-left","nav-right","nav-up","none","normal","object-fit","object-position","opacity","order","orphans","outline","outline-color","outline-offset","outline-style","outline-width","overflow","overflow-wrap","overflow-x","overflow-y","padding","padding-bottom","padding-left","padding-right","padding-top","page-break-after","page-break-before","page-break-inside","pause","pause-after","pause-before","perspective","perspective-origin","pointer-events","position","quotes","resize","rest","rest-after","rest-before","right","row-gap","scroll-margin","scroll-margin-block","scroll-margin-block-end","scroll-margin-block-start","scroll-margin-bottom","scroll-margin-inline","scroll-margin-inline-end","scroll-margin-inline-start","scroll-margin-left","scroll-margin-right","scroll-margin-top","scroll-padding","scroll-padding-block","scroll-padding-block-end","scroll-padding-block-start","scroll-padding-bottom","scroll-padding-inline","scroll-padding-inline-end","scroll-padding-inline-start","scroll-padding-left","scroll-padding-right","scroll-padding-top","scroll-snap-align","scroll-snap-stop","scroll-snap-type","shape-image-threshold","shape-margin","shape-outside","speak","speak-as","src","tab-size","table-layout","text-align","text-align-all","text-align-last","text-combine-upright","text-decoration","text-decoration-color","text-decoration-line","text-decoration-style","text-emphasis","text-emphasis-color","text-emphasis-position","text-emphasis-style","text-indent","text-justify","text-orientation","text-overflow","text-rendering","text-shadow","text-transform","text-underline-position","top","transform","transform-box","transform-origin","transform-style","transition","transition-delay","transition-duration","transition-property","transition-timing-function","unicode-bidi","vertical-align","visibility","voice-balance","voice-duration","voice-family","voice-pitch","voice-range","voice-rate","voice-stress","voice-volume","white-space","widows","width","will-change","word-break","word-spacing","word-wrap","writing-mode","z-index"].reverse(),n=i.concat(r)
    ;return a=>{const s=(e=>({IMPORTANT:{scope:"meta",begin:"!important"},
    BLOCK_COMMENT:e.C_BLOCK_COMMENT_MODE,HEXCOLOR:{scope:"number",
    begin:/#(([0-9a-fA-F]{3,4})|(([0-9a-fA-F]{2}){3,4}))\b/},FUNCTION_DISPATCH:{
    className:"built_in",begin:/[\w-]+(?=\()/},ATTRIBUTE_SELECTOR_MODE:{
    scope:"selector-attr",begin:/\[/,end:/\]/,illegal:"$",
    contains:[e.APOS_STRING_MODE,e.QUOTE_STRING_MODE]},CSS_NUMBER_MODE:{
    scope:"number",
    begin:e.NUMBER_RE+"(%|em|ex|ch|rem|vw|vh|vmin|vmax|cm|mm|in|pt|pc|px|deg|grad|rad|turn|s|ms|Hz|kHz|dpi|dpcm|dppx)?",
    relevance:0},CSS_VARIABLE:{className:"attr",begin:/--[A-Za-z][A-Za-z0-9_-]*/}
    }))(a),l=n,d="([\\w-]+|@\\{[\\w-]+\\})",c=[],g=[],m=e=>({className:"string",
    begin:"~?"+e+".*?"+e}),p=(e,t,i)=>({className:e,begin:t,relevance:i}),b={
    $pattern:/[a-z-]+/,keyword:"and or not only",attribute:t.join(" ")},u={
    begin:"\\(",end:"\\)",contains:g,keywords:b,relevance:0}
    ;g.push(a.C_LINE_COMMENT_MODE,a.C_BLOCK_COMMENT_MODE,m("'"),m('"'),s.CSS_NUMBER_MODE,{
    begin:"(url|data-uri)\\(",starts:{className:"string",end:"[\\)\\n]",
    excludeEnd:!0}
    },s.HEXCOLOR,u,p("variable","@@?[\\w-]+",10),p("variable","@\\{[\\w-]+\\}"),p("built_in","~?`[^`]*?`"),{
    className:"attribute",begin:"[\\w-]+\\s*:",end:":",returnBegin:!0,excludeEnd:!0
    },s.IMPORTANT);const h=g.concat({begin:/\{/,end:/\}/,contains:c}),f={
    beginKeywords:"when",endsWithParent:!0,contains:[{beginKeywords:"and not"
    }].concat(g)},v={begin:d+"\\s*:",returnBegin:!0,end:/[;}]/,relevance:0,
    contains:[{begin:/-(webkit|moz|ms|o)-/},s.CSS_VARIABLE,{className:"attribute",
    begin:"\\b("+o.join("|")+")\\b",end:/(?=:)/,starts:{endsWithParent:!0,
    illegal:"[<=$]",relevance:0,contains:g}}]},w={className:"keyword",
    begin:"@(import|media|charset|font-face|(-[a-z]+-)?keyframes|supports|document|namespace|page|viewport|host)\\b",
    starts:{end:"[;{}]",keywords:b,returnEnd:!0,contains:g,relevance:0}},k={
    className:"variable",variants:[{begin:"@[\\w-]+\\s*:",relevance:15},{
    begin:"@[\\w-]+"}],starts:{end:"[;}]",returnEnd:!0,contains:h}},y={variants:[{
    begin:"[\\.#:&\\[>]",end:"[;{}]"},{begin:d,end:/\{/}],returnBegin:!0,
    returnEnd:!0,illegal:"[<='$\"]",relevance:0,
    contains:[a.C_LINE_COMMENT_MODE,a.C_BLOCK_COMMENT_MODE,f,p("keyword","all\\b"),p("variable","@\\{[\\w-]+\\}"),{
    begin:"\\b("+e.join("|")+")\\b",className:"selector-tag"
    },s.CSS_NUMBER_MODE,p("selector-tag",d,0),p("selector-id","#"+d),p("selector-class","\\."+d,0),p("selector-tag","&",0),s.ATTRIBUTE_SELECTOR_MODE,{
    className:"selector-pseudo",begin:":("+i.join("|")+")"},{
    className:"selector-pseudo",begin:":(:)?("+r.join("|")+")"},{begin:/\(/,
    end:/\)/,relevance:0,contains:h},{begin:"!important"},s.FUNCTION_DISPATCH]},x={
    begin:`[\\w-]+:(:)?(${l.join("|")})`,returnBegin:!0,contains:[y]}
    ;return c.push(a.C_LINE_COMMENT_MODE,a.C_BLOCK_COMMENT_MODE,w,k,x,v,y),{
    name:"Less",case_insensitive:!0,illegal:"[=>'/<($\"]",contains:c}}})()
    ;hljs.registerLanguage("less",e)})();/*! `csharp` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var e=(()=>{"use strict";return e=>{const n={
    keyword:["abstract","as","base","break","case","catch","class","const","continue","do","else","event","explicit","extern","finally","fixed","for","foreach","goto","if","implicit","in","interface","internal","is","lock","namespace","new","operator","out","override","params","private","protected","public","readonly","record","ref","return","sealed","sizeof","stackalloc","static","struct","switch","this","throw","try","typeof","unchecked","unsafe","using","virtual","void","volatile","while"].concat(["add","alias","and","ascending","async","await","by","descending","equals","from","get","global","group","init","into","join","let","nameof","not","notnull","on","or","orderby","partial","remove","select","set","unmanaged","value|0","var","when","where","with","yield"]),
    built_in:["bool","byte","char","decimal","delegate","double","dynamic","enum","float","int","long","nint","nuint","object","sbyte","short","string","ulong","uint","ushort"],
    literal:["default","false","null","true"]},a=e.inherit(e.TITLE_MODE,{
    begin:"[a-zA-Z](\\.?\\w)*"}),i={className:"number",variants:[{
    begin:"\\b(0b[01']+)"},{
    begin:"(-?)\\b([\\d']+(\\.[\\d']*)?|\\.[\\d']+)(u|U|l|L|ul|UL|f|F|b|B)"},{
    begin:"(-?)(\\b0[xX][a-fA-F0-9']+|(\\b[\\d']+(\\.[\\d']*)?|\\.[\\d']+)([eE][-+]?[\\d']+)?)"
    }],relevance:0},s={className:"string",begin:'@"',end:'"',contains:[{begin:'""'}]
    },t=e.inherit(s,{illegal:/\n/}),r={className:"subst",begin:/\{/,end:/\}/,
    keywords:n},l=e.inherit(r,{illegal:/\n/}),c={className:"string",begin:/\$"/,
    end:'"',illegal:/\n/,contains:[{begin:/\{\{/},{begin:/\}\}/
    },e.BACKSLASH_ESCAPE,l]},o={className:"string",begin:/\$@"/,end:'"',contains:[{
    begin:/\{\{/},{begin:/\}\}/},{begin:'""'},r]},d=e.inherit(o,{illegal:/\n/,
    contains:[{begin:/\{\{/},{begin:/\}\}/},{begin:'""'},l]})
    ;r.contains=[o,c,s,e.APOS_STRING_MODE,e.QUOTE_STRING_MODE,i,e.C_BLOCK_COMMENT_MODE],
    l.contains=[d,c,t,e.APOS_STRING_MODE,e.QUOTE_STRING_MODE,i,e.inherit(e.C_BLOCK_COMMENT_MODE,{
    illegal:/\n/})];const g={variants:[o,c,s,e.APOS_STRING_MODE,e.QUOTE_STRING_MODE]
    },E={begin:"<",end:">",contains:[{beginKeywords:"in out"},a]
    },_=e.IDENT_RE+"(<"+e.IDENT_RE+"(\\s*,\\s*"+e.IDENT_RE+")*>)?(\\[\\])?",b={
    begin:"@"+e.IDENT_RE,relevance:0};return{name:"C#",aliases:["cs","c#"],
    keywords:n,illegal:/::/,contains:[e.COMMENT("///","$",{returnBegin:!0,
    contains:[{className:"doctag",variants:[{begin:"///",relevance:0},{
    begin:"\x3c!--|--\x3e"},{begin:"</?",end:">"}]}]
    }),e.C_LINE_COMMENT_MODE,e.C_BLOCK_COMMENT_MODE,{className:"meta",begin:"#",
    end:"$",keywords:{
    keyword:"if else elif endif define undef warning error line region endregion pragma checksum"
    }},g,i,{beginKeywords:"class interface",relevance:0,end:/[{;=]/,
    illegal:/[^\s:,]/,contains:[{beginKeywords:"where class"
    },a,E,e.C_LINE_COMMENT_MODE,e.C_BLOCK_COMMENT_MODE]},{beginKeywords:"namespace",
    relevance:0,end:/[{;=]/,illegal:/[^\s:]/,
    contains:[a,e.C_LINE_COMMENT_MODE,e.C_BLOCK_COMMENT_MODE]},{
    beginKeywords:"record",relevance:0,end:/[{;=]/,illegal:/[^\s:]/,
    contains:[a,E,e.C_LINE_COMMENT_MODE,e.C_BLOCK_COMMENT_MODE]},{className:"meta",
    begin:"^\\s*\\[(?=[\\w])",excludeBegin:!0,end:"\\]",excludeEnd:!0,contains:[{
    className:"string",begin:/"/,end:/"/}]},{
    beginKeywords:"new return throw await else",relevance:0},{className:"function",
    begin:"("+_+"\\s+)+"+e.IDENT_RE+"\\s*(<[^=]+>\\s*)?\\(",returnBegin:!0,
    end:/\s*[{;=]/,excludeEnd:!0,keywords:n,contains:[{
    beginKeywords:"public private protected static internal protected abstract async extern override unsafe virtual new sealed partial",
    relevance:0},{begin:e.IDENT_RE+"\\s*(<[^=]+>\\s*)?\\(",returnBegin:!0,
    contains:[e.TITLE_MODE,E],relevance:0},{match:/\(\)/},{className:"params",
    begin:/\(/,end:/\)/,excludeBegin:!0,excludeEnd:!0,keywords:n,relevance:0,
    contains:[g,i,e.C_BLOCK_COMMENT_MODE]
    },e.C_LINE_COMMENT_MODE,e.C_BLOCK_COMMENT_MODE]},b]}}})()
    ;hljs.registerLanguage("csharp",e)})();/*! `sql` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var e=(()=>{"use strict";return e=>{
    const r=e.regex,t=e.COMMENT("--","$"),n=["true","false","unknown"],a=["bigint","binary","blob","boolean","char","character","clob","date","dec","decfloat","decimal","float","int","integer","interval","nchar","nclob","national","numeric","real","row","smallint","time","timestamp","varchar","varying","varbinary"],i=["abs","acos","array_agg","asin","atan","avg","cast","ceil","ceiling","coalesce","corr","cos","cosh","count","covar_pop","covar_samp","cume_dist","dense_rank","deref","element","exp","extract","first_value","floor","json_array","json_arrayagg","json_exists","json_object","json_objectagg","json_query","json_table","json_table_primitive","json_value","lag","last_value","lead","listagg","ln","log","log10","lower","max","min","mod","nth_value","ntile","nullif","percent_rank","percentile_cont","percentile_disc","position","position_regex","power","rank","regr_avgx","regr_avgy","regr_count","regr_intercept","regr_r2","regr_slope","regr_sxx","regr_sxy","regr_syy","row_number","sin","sinh","sqrt","stddev_pop","stddev_samp","substring","substring_regex","sum","tan","tanh","translate","translate_regex","treat","trim","trim_array","unnest","upper","value_of","var_pop","var_samp","width_bucket"],s=["create table","insert into","primary key","foreign key","not null","alter table","add constraint","grouping sets","on overflow","character set","respect nulls","ignore nulls","nulls first","nulls last","depth first","breadth first"],o=i,c=["abs","acos","all","allocate","alter","and","any","are","array","array_agg","array_max_cardinality","as","asensitive","asin","asymmetric","at","atan","atomic","authorization","avg","begin","begin_frame","begin_partition","between","bigint","binary","blob","boolean","both","by","call","called","cardinality","cascaded","case","cast","ceil","ceiling","char","char_length","character","character_length","check","classifier","clob","close","coalesce","collate","collect","column","commit","condition","connect","constraint","contains","convert","copy","corr","corresponding","cos","cosh","count","covar_pop","covar_samp","create","cross","cube","cume_dist","current","current_catalog","current_date","current_default_transform_group","current_path","current_role","current_row","current_schema","current_time","current_timestamp","current_path","current_role","current_transform_group_for_type","current_user","cursor","cycle","date","day","deallocate","dec","decimal","decfloat","declare","default","define","delete","dense_rank","deref","describe","deterministic","disconnect","distinct","double","drop","dynamic","each","element","else","empty","end","end_frame","end_partition","end-exec","equals","escape","every","except","exec","execute","exists","exp","external","extract","false","fetch","filter","first_value","float","floor","for","foreign","frame_row","free","from","full","function","fusion","get","global","grant","group","grouping","groups","having","hold","hour","identity","in","indicator","initial","inner","inout","insensitive","insert","int","integer","intersect","intersection","interval","into","is","join","json_array","json_arrayagg","json_exists","json_object","json_objectagg","json_query","json_table","json_table_primitive","json_value","lag","language","large","last_value","lateral","lead","leading","left","like","like_regex","listagg","ln","local","localtime","localtimestamp","log","log10","lower","match","match_number","match_recognize","matches","max","member","merge","method","min","minute","mod","modifies","module","month","multiset","national","natural","nchar","nclob","new","no","none","normalize","not","nth_value","ntile","null","nullif","numeric","octet_length","occurrences_regex","of","offset","old","omit","on","one","only","open","or","order","out","outer","over","overlaps","overlay","parameter","partition","pattern","per","percent","percent_rank","percentile_cont","percentile_disc","period","portion","position","position_regex","power","precedes","precision","prepare","primary","procedure","ptf","range","rank","reads","real","recursive","ref","references","referencing","regr_avgx","regr_avgy","regr_count","regr_intercept","regr_r2","regr_slope","regr_sxx","regr_sxy","regr_syy","release","result","return","returns","revoke","right","rollback","rollup","row","row_number","rows","running","savepoint","scope","scroll","search","second","seek","select","sensitive","session_user","set","show","similar","sin","sinh","skip","smallint","some","specific","specifictype","sql","sqlexception","sqlstate","sqlwarning","sqrt","start","static","stddev_pop","stddev_samp","submultiset","subset","substring","substring_regex","succeeds","sum","symmetric","system","system_time","system_user","table","tablesample","tan","tanh","then","time","timestamp","timezone_hour","timezone_minute","to","trailing","translate","translate_regex","translation","treat","trigger","trim","trim_array","true","truncate","uescape","union","unique","unknown","unnest","update","upper","user","using","value","values","value_of","var_pop","var_samp","varbinary","varchar","varying","versioning","when","whenever","where","width_bucket","window","with","within","without","year","add","asc","collation","desc","final","first","last","view"].filter((e=>!i.includes(e))),l={
    begin:r.concat(/\b/,r.either(...o),/\s*\(/),relevance:0,keywords:{built_in:o}}
    ;return{name:"SQL",case_insensitive:!0,illegal:/[{}]|<\//,keywords:{
    $pattern:/\b[\w\.]+/,keyword:((e,{exceptions:r,when:t}={})=>{const n=t
    ;return r=r||[],e.map((e=>e.match(/\|\d+$/)||r.includes(e)?e:n(e)?e+"|0":e))
    })(c,{when:e=>e.length<3}),literal:n,type:a,
    built_in:["current_catalog","current_date","current_default_transform_group","current_path","current_role","current_schema","current_transform_group_for_type","current_user","session_user","system_time","system_user","current_time","localtime","current_timestamp","localtimestamp"]
    },contains:[{begin:r.either(...s),relevance:0,keywords:{$pattern:/[\w\.]+/,
    keyword:c.concat(s),literal:n,type:a}},{className:"type",
    begin:r.either("double precision","large object","with timezone","without timezone")
    },l,{className:"variable",begin:/@[a-z0-9]+/},{className:"string",variants:[{
    begin:/'/,end:/'/,contains:[{begin:/''/}]}]},{begin:/"/,end:/"/,contains:[{
    begin:/""/}]},e.C_NUMBER_MODE,e.C_BLOCK_COMMENT_MODE,t,{className:"operator",
    begin:/[-+*/=%^~]|&&?|\|\|?|!=?|<(?:=>?|<|>)?|>[>=]?/,relevance:0}]}}})()
    ;hljs.registerLanguage("sql",e)})();/*! `bash` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var e=(()=>{"use strict";return e=>{const s=e.regex,t={},n={begin:/\$\{/,
    end:/\}/,contains:["self",{begin:/:-/,contains:[t]}]};Object.assign(t,{
    className:"variable",variants:[{
    begin:s.concat(/\$[\w\d#@][\w\d_]*/,"(?![\\w\\d])(?![$])")},n]});const a={
    className:"subst",begin:/\$\(/,end:/\)/,contains:[e.BACKSLASH_ESCAPE]},i={
    begin:/<<-?\s*(?=\w+)/,starts:{contains:[e.END_SAME_AS_BEGIN({begin:/(\w+)/,
    end:/(\w+)/,className:"string"})]}},c={className:"string",begin:/"/,end:/"/,
    contains:[e.BACKSLASH_ESCAPE,t,a]};a.contains.push(c);const o={begin:/\$\(\(/,
    end:/\)\)/,contains:[{begin:/\d+#[0-9a-f]+/,className:"number"},e.NUMBER_MODE,t]
    },r=e.SHEBANG({binary:"(fish|bash|zsh|sh|csh|ksh|tcsh|dash|scsh)",relevance:10
    }),l={className:"function",begin:/\w[\w\d_]*\s*\(\s*\)\s*\{/,returnBegin:!0,
    contains:[e.inherit(e.TITLE_MODE,{begin:/\w[\w\d_]*/})],relevance:0};return{
    name:"Bash",aliases:["sh"],keywords:{$pattern:/\b[a-z._-]+\b/,
    keyword:["if","then","else","elif","fi","for","while","in","do","done","case","esac","function"],
    literal:["true","false"],
    built_in:["break","cd","continue","eval","exec","exit","export","getopts","hash","pwd","readonly","return","shift","test","times","trap","umask","unset","alias","bind","builtin","caller","command","declare","echo","enable","help","let","local","logout","mapfile","printf","read","readarray","source","type","typeset","ulimit","unalias","set","shopt","autoload","bg","bindkey","bye","cap","chdir","clone","comparguments","compcall","compctl","compdescribe","compfiles","compgroups","compquote","comptags","comptry","compvalues","dirs","disable","disown","echotc","echoti","emulate","fc","fg","float","functions","getcap","getln","history","integer","jobs","kill","limit","log","noglob","popd","print","pushd","pushln","rehash","sched","setcap","setopt","stat","suspend","ttyctl","unfunction","unhash","unlimit","unsetopt","vared","wait","whence","where","which","zcompile","zformat","zftp","zle","zmodload","zparseopts","zprof","zpty","zregexparse","zsocket","zstyle","ztcp","chcon","chgrp","chown","chmod","cp","dd","df","dir","dircolors","ln","ls","mkdir","mkfifo","mknod","mktemp","mv","realpath","rm","rmdir","shred","sync","touch","truncate","vdir","b2sum","base32","base64","cat","cksum","comm","csplit","cut","expand","fmt","fold","head","join","md5sum","nl","numfmt","od","paste","ptx","pr","sha1sum","sha224sum","sha256sum","sha384sum","sha512sum","shuf","sort","split","sum","tac","tail","tr","tsort","unexpand","uniq","wc","arch","basename","chroot","date","dirname","du","echo","env","expr","factor","groups","hostid","id","link","logname","nice","nohup","nproc","pathchk","pinky","printenv","printf","pwd","readlink","runcon","seq","sleep","stat","stdbuf","stty","tee","test","timeout","tty","uname","unlink","uptime","users","who","whoami","yes"]
    },contains:[r,e.SHEBANG(),l,o,e.HASH_COMMENT_MODE,i,{match:/(\/[a-z._-]+)+/},c,{
    className:"",begin:/\\"/},{className:"string",begin:/'/,end:/'/},t]}}})()
    ;hljs.registerLanguage("bash",e)})();/*! `shell` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var s=(()=>{"use strict";return s=>({name:"Shell Session",
    aliases:["console","shellsession"],contains:[{className:"meta",
    begin:/^\s{0,3}[/~\w\d[\]()@-]*[>%$#][ ]?/,starts:{end:/[^\\](?=\s*$)/,
    subLanguage:"bash"}}]})})();hljs.registerLanguage("shell",s)})();/*! `dos` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var e=(()=>{"use strict";return e=>{const r=e.COMMENT(/^\s*@?rem\b/,/$/,{
    relevance:10});return{name:"Batch file (DOS)",aliases:["bat","cmd"],
    case_insensitive:!0,illegal:/\/\*/,keywords:{
    keyword:["if","else","goto","for","in","do","call","exit","not","exist","errorlevel","defined","equ","neq","lss","leq","gtr","geq"],
    built_in:["prn","nul","lpt3","lpt2","lpt1","con","com4","com3","com2","com1","aux","shift","cd","dir","echo","setlocal","endlocal","set","pause","copy","append","assoc","at","attrib","break","cacls","cd","chcp","chdir","chkdsk","chkntfs","cls","cmd","color","comp","compact","convert","date","dir","diskcomp","diskcopy","doskey","erase","fs","find","findstr","format","ftype","graftabl","help","keyb","label","md","mkdir","mode","more","move","path","pause","print","popd","pushd","promt","rd","recover","rem","rename","replace","restore","rmdir","shift","sort","start","subst","time","title","tree","type","ver","verify","vol","ping","net","ipconfig","taskkill","xcopy","ren","del"]
    },contains:[{className:"variable",begin:/%%[^ ]|%[^ ]+?%|![^ ]+?!/},{
    className:"function",begin:"^\\s*[A-Za-z._?][A-Za-z0-9_$#@~.?]*(:|\\s+label)",
    end:"goto:eof",contains:[e.inherit(e.TITLE_MODE,{
    begin:"([_a-zA-Z]\\w*\\.)*([_a-zA-Z]\\w*:)?[_a-zA-Z]\\w*"}),r]},{
    className:"number",begin:"\\b\\d+",relevance:0},r]}}})()
    ;hljs.registerLanguage("dos",e)})();/*! `powershell` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var e=(()=>{"use strict";return e=>{const n={$pattern:/-?[A-z\.\-]+\b/,
    keyword:"if else foreach return do while until elseif begin for trap data dynamicparam end break throw param continue finally in switch exit filter try process catch hidden static parameter",
    built_in:"ac asnp cat cd CFS chdir clc clear clhy cli clp cls clv cnsn compare copy cp cpi cpp curl cvpa dbp del diff dir dnsn ebp echo|0 epal epcsv epsn erase etsn exsn fc fhx fl ft fw gal gbp gc gcb gci gcm gcs gdr gerr ghy gi gin gjb gl gm gmo gp gps gpv group gsn gsnp gsv gtz gu gv gwmi h history icm iex ihy ii ipal ipcsv ipmo ipsn irm ise iwmi iwr kill lp ls man md measure mi mount move mp mv nal ndr ni nmo npssc nsn nv ogv oh popd ps pushd pwd r rbp rcjb rcsn rd rdr ren ri rjb rm rmdir rmo rni rnp rp rsn rsnp rujb rv rvpa rwmi sajb sal saps sasv sbp sc scb select set shcm si sl sleep sls sort sp spjb spps spsv start stz sujb sv swmi tee trcm type wget where wjb write"
    },s={begin:"`[\\s\\S]",relevance:0},i={className:"variable",variants:[{
    begin:/\$\B/},{className:"keyword",begin:/\$this/},{begin:/\$[\w\d][\w\d_:]*/}]
    },a={className:"string",variants:[{begin:/"/,end:/"/},{begin:/@"/,end:/^"@/}],
    contains:[s,i,{className:"variable",begin:/\$[A-z]/,end:/[^A-z]/}]},t={
    className:"string",variants:[{begin:/'/,end:/'/},{begin:/@'/,end:/^'@/}]
    },r=e.inherit(e.COMMENT(null,null),{variants:[{begin:/#/,end:/$/},{begin:/<#/,
    end:/#>/}],contains:[{className:"doctag",variants:[{
    begin:/\.(synopsis|description|example|inputs|outputs|notes|link|component|role|functionality)/
    },{
    begin:/\.(parameter|forwardhelptargetname|forwardhelpcategory|remotehelprunspace|externalhelp)\s+\S+/
    }]}]}),c={className:"class",beginKeywords:"class enum",end:/\s*[{]/,
    excludeEnd:!0,relevance:0,contains:[e.TITLE_MODE]},l={className:"function",
    begin:/function\s+/,end:/\s*\{|$/,excludeEnd:!0,returnBegin:!0,relevance:0,
    contains:[{begin:"function",relevance:0,className:"keyword"},{className:"title",
    begin:/\w[\w\d]*((-)[\w\d]+)*/,relevance:0},{begin:/\(/,end:/\)/,
    className:"params",relevance:0,contains:[i]}]},o={begin:/using\s/,end:/$/,
    returnBegin:!0,contains:[a,t,{className:"keyword",
    begin:/(using|assembly|command|module|namespace|type)/}]},p={
    className:"function",begin:/\[.*\]\s*[\w]+[ ]??\(/,end:/$/,returnBegin:!0,
    relevance:0,contains:[{className:"keyword",
    begin:"(".concat(n.keyword.toString().replace(/\s/g,"|"),")\\b"),endsParent:!0,
    relevance:0},e.inherit(e.TITLE_MODE,{endsParent:!0})]
    },g=[p,r,s,e.NUMBER_MODE,a,t,{className:"built_in",variants:[{
    begin:"(Add|Clear|Close|Copy|Enter|Exit|Find|Format|Get|Hide|Join|Lock|Move|New|Open|Optimize|Pop|Push|Redo|Remove|Rename|Reset|Resize|Search|Select|Set|Show|Skip|Split|Step|Switch|Undo|Unlock|Watch|Backup|Checkpoint|Compare|Compress|Convert|ConvertFrom|ConvertTo|Dismount|Edit|Expand|Export|Group|Import|Initialize|Limit|Merge|Mount|Out|Publish|Restore|Save|Sync|Unpublish|Update|Approve|Assert|Build|Complete|Confirm|Deny|Deploy|Disable|Enable|Install|Invoke|Register|Request|Restart|Resume|Start|Stop|Submit|Suspend|Uninstall|Unregister|Wait|Debug|Measure|Ping|Repair|Resolve|Test|Trace|Connect|Disconnect|Read|Receive|Send|Write|Block|Grant|Protect|Revoke|Unblock|Unprotect|Use|ForEach|Sort|Tee|Where)+(-)[\\w\\d]+"
    }]},i,{className:"literal",begin:/\$(null|true|false)\b/},{
    className:"selector-tag",begin:/@\B/,relevance:0}],m={begin:/\[/,end:/\]/,
    excludeBegin:!0,excludeEnd:!0,relevance:0,contains:[].concat("self",g,{
    begin:"(string|char|byte|int|long|bool|decimal|single|double|DateTime|xml|array|hashtable|void)",
    className:"built_in",relevance:0},{className:"type",begin:/[\.\w\d]+/,
    relevance:0})};return p.contains.unshift(m),{name:"PowerShell",
    aliases:["pwsh","ps","ps1"],case_insensitive:!0,keywords:n,
    contains:g.concat(c,l,o,{variants:[{className:"operator",
    begin:"(-and|-as|-band|-bnot|-bor|-bxor|-casesensitive|-ccontains|-ceq|-cge|-cgt|-cle|-clike|-clt|-cmatch|-cne|-cnotcontains|-cnotlike|-cnotmatch|-contains|-creplace|-csplit|-eq|-exact|-f|-file|-ge|-gt|-icontains|-ieq|-ige|-igt|-ile|-ilike|-ilt|-imatch|-in|-ine|-inotcontains|-inotlike|-inotmatch|-ireplace|-is|-isnot|-isplit|-join|-le|-like|-lt|-match|-ne|-not|-notcontains|-notin|-notlike|-notmatch|-or|-regex|-replace|-shl|-shr|-split|-wildcard|-xor)\\b"
    },{className:"literal",begin:/(-){1,2}[\w\d-]+/,relevance:0}]},m)}}})()
    ;hljs.registerLanguage("powershell",e)})();/*! `json` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var e=(()=>{"use strict";return e=>({name:"JSON",contains:[{
    className:"attr",begin:/"(\\.|[^\\"\r\n])*"(?=\s*:)/,relevance:1.01},{
    match:/[{}[\],:]/,className:"punctuation",relevance:0},e.QUOTE_STRING_MODE,{
    beginKeywords:"true false null"
    },e.C_NUMBER_MODE,e.C_LINE_COMMENT_MODE,e.C_BLOCK_COMMENT_MODE],illegal:"\\S"})
    })();hljs.registerLanguage("json",e)})();/*! `plaintext` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var t=(()=>{"use strict";return t=>({name:"Plain text",
    aliases:["text","txt"],disableAutodetect:!0})})()
    ;hljs.registerLanguage("plaintext",t)})();/*! `xml` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var e=(()=>{"use strict";return e=>{
    const a=e.regex,n=a.concat(/[A-Z_]/,a.optional(/[A-Z0-9_.-]*:/),/[A-Z0-9_.-]*/),s={
    className:"symbol",begin:/&[a-z]+;|&#[0-9]+;|&#x[a-f0-9]+;/},t={begin:/\s/,
    contains:[{className:"keyword",begin:/#?[a-z_][a-z1-9_-]+/,illegal:/\n/}]
    },i=e.inherit(t,{begin:/\(/,end:/\)/}),c=e.inherit(e.APOS_STRING_MODE,{
    className:"string"}),l=e.inherit(e.QUOTE_STRING_MODE,{className:"string"}),r={
    endsWithParent:!0,illegal:/</,relevance:0,contains:[{className:"attr",
    begin:/[A-Za-z0-9._:-]+/,relevance:0},{begin:/=\s*/,relevance:0,contains:[{
    className:"string",endsParent:!0,variants:[{begin:/"/,end:/"/,contains:[s]},{
    begin:/'/,end:/'/,contains:[s]},{begin:/[^\s"'=<>`]+/}]}]}]};return{
    name:"HTML, XML",
    aliases:["html","xhtml","rss","atom","xjb","xsd","xsl","plist","wsf","svg"],
    case_insensitive:!0,contains:[{className:"meta",begin:/<![a-z]/,end:/>/,
    relevance:10,contains:[t,l,c,i,{begin:/\[/,end:/\]/,contains:[{className:"meta",
    begin:/<![a-z]/,end:/>/,contains:[t,i,l,c]}]}]},e.COMMENT(/<!--/,/-->/,{
    relevance:10}),{begin:/<!\[CDATA\[/,end:/\]\]>/,relevance:10},s,{
    className:"meta",begin:/<\?xml/,end:/\?>/,relevance:10},{className:"tag",
    begin:/<style(?=\s|>)/,end:/>/,keywords:{name:"style"},contains:[r],starts:{
    end:/<\/style>/,returnEnd:!0,subLanguage:["css","xml"]}},{className:"tag",
    begin:/<script(?=\s|>)/,end:/>/,keywords:{name:"script"},contains:[r],starts:{
    end:/<\/script>/,returnEnd:!0,subLanguage:["javascript","handlebars","xml"]}},{
    className:"tag",begin:/<>|<\/>/},{className:"tag",
    begin:a.concat(/</,a.lookahead(a.concat(n,a.either(/\/>/,/>/,/\s/)))),
    end:/\/?>/,contains:[{className:"name",begin:n,relevance:0,starts:r}]},{
    className:"tag",begin:a.concat(/<\//,a.lookahead(a.concat(n,/>/))),contains:[{
    className:"name",begin:n,relevance:0},{begin:/>/,relevance:0,endsParent:!0}]}]}}
    })();hljs.registerLanguage("xml",e)})();/*! `markdown` grammar compiled for Highlight.js 11.4.0 */
    (()=>{var e=(()=>{"use strict";return e=>{const n={begin:/<\/?[A-Za-z_]/,
    end:">",subLanguage:"xml",relevance:0},a={variants:[{begin:/\[.+?\]\[.*?\]/,
    relevance:0},{
    begin:/\[.+?\]\(((data|javascript|mailto):|(?:http|ftp)s?:\/\/).*?\)/,
    relevance:2},{
    begin:e.regex.concat(/\[.+?\]\(/,/[A-Za-z][A-Za-z0-9+.-]*/,/:\/\/.*?\)/),
    relevance:2},{begin:/\[.+?\]\([./?&#].*?\)/,relevance:1},{
    begin:/\[.*?\]\(.*?\)/,relevance:0}],returnBegin:!0,contains:[{match:/\[(?=\])/
    },{className:"string",relevance:0,begin:"\\[",end:"\\]",excludeBegin:!0,
    returnEnd:!0},{className:"link",relevance:0,begin:"\\]\\(",end:"\\)",
    excludeBegin:!0,excludeEnd:!0},{className:"symbol",relevance:0,begin:"\\]\\[",
    end:"\\]",excludeBegin:!0,excludeEnd:!0}]},i={className:"strong",contains:[],
    variants:[{begin:/_{2}/,end:/_{2}/},{begin:/\*{2}/,end:/\*{2}/}]},s={
    className:"emphasis",contains:[],variants:[{begin:/\*(?!\*)/,end:/\*/},{
    begin:/_(?!_)/,end:/_/,relevance:0}]};i.contains.push(s),s.contains.push(i)
    ;let c=[n,a]
    ;return i.contains=i.contains.concat(c),s.contains=s.contains.concat(c),
    c=c.concat(i,s),{name:"Markdown",aliases:["md","mkdown","mkd"],contains:[{
    className:"section",variants:[{begin:"^#{1,6}",end:"$",contains:c},{
    begin:"(?=^.+?\\n[=-]{2,}$)",contains:[{begin:"^[=-]*$"},{begin:"^",end:"\\n",
    contains:c}]}]},n,{className:"bullet",begin:"^[ \t]*([*+-]|(\\d+\\.))(?=\\s+)",
    end:"\\s+",excludeEnd:!0},i,s,{className:"quote",begin:"^>\\s+",contains:c,
    end:"$"},{className:"code",variants:[{begin:"(`{3,})[^`](.|\\n)*?\\1`*[ ]*"},{
    begin:"(~{3,})[^~](.|\\n)*?\\1~*[ ]*"},{begin:"```",end:"```+[ ]*$"},{
    begin:"~~~",end:"~~~+[ ]*$"},{begin:"`.+?`"},{begin:"(?=^( {4}|\\t))",
    contains:[{begin:"^( {4}|\\t)",end:"(\\n)$"}],relevance:0}]},{
    begin:"^[-\\*]{3,}",end:"$"},a,{begin:/^\[[^\n]+\]:/,returnBegin:!0,contains:[{
    className:"symbol",begin:/\[/,end:/\]/,excludeBegin:!0,excludeEnd:!0},{
    className:"link",begin:/:\s*/,end:/$/,excludeBegin:!0}]}]}}})()
    ;hljs.registerLanguage("markdown",e)})();