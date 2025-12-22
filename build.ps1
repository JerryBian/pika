$WWWRootLoc = Join-Path $PSScriptRoot src wwwroot
$NodeModulesLoc = Join-Path $PSScriptRoot node_modules

npm install
ncu -u
npm install

Copy-Item -Path $(Join-Path $NodeModulesLoc bootstrap-icons font fonts *) `
    -Destination $(Join-Path $WWWRootLoc fonts)
Write-Output "Bootstrap fonts copied"

npx sass $(Join-Path $WWWRootLoc src style.scss) $(Join-Path $WWWRootLoc dist style.css)
Write-Output "SASS completed."

npx uglifycss --ugly-comments --output $(Join-Path $WWWRootLoc style.min.css) `
    $(Join-Path $NodeModulesLoc \@highlightjs cdn-assets styles dark.min.css) `
    $(Join-Path $WWWRootLoc dist style.css)
Write-Output "Minify CSS completed."

npx uglifyjs --compress -o $(Join-Path $WWWRootLoc script.min.js) `
    $(Join-Path $NodeModulesLoc \@highlightjs cdn-assets highlight.js) `
    $(Join-Path $NodeModulesLoc bootstrap dist js/bootstrap.bundle.js) `
    $(Join-Path $NodeModulesLoc \@microsoft/signalr dist/browser/signalr.js) `
    $(Join-Path $NodeModulesLoc chart.js dist/chart.umd.js) `
    $(Join-Path $WWWRootLoc src script.js)
Write-Output "Minify JS completed."

Write-Output "All Done!"