$WWWRootLoc = Join-Path $PSScriptRoot src web wwwroot
$NodeModulesLoc = Join-Path $PSScriptRoot node_modules
$LibRootLoc = Join-Path $PSScriptRoot lib

npx sass $(Join-Path $WWWRootLoc src style.scss) $(Join-Path $WWWRootLoc dist style.css)

npx uglifycss --ugly-comments --output $(Join-Path $WWWRootLoc style.min.css) `
    $(Join-Path $LibRootLoc highlightjs style.css) `
    $(Join-Path $WWWRootLoc dist style.css)

npx uglifyjs --compress -o $(Join-Path $WWWRootLoc script.min.js) `
    $(Join-Path $LibRootLoc highlightjs highlight.js) `
    $(Join-Path $NodeModulesLoc "@fortawesome" fontawesome-free js solid.js) `
    $(Join-Path $NodeModulesLoc "@fortawesome" fontawesome-free js brands.js) `
    $(Join-Path $NodeModulesLoc "@fortawesome" fontawesome-free js fontawesome.js) `
    $(Join-Path $NodeModulesLoc bootstrap dist js bootstrap.js) `
    $(Join-Path $WWWRootLoc src script.js)