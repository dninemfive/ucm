console.log("a");
// https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/API/bookmarks/search
let id;
function doSearch() {
    let query = "http"
    console.log(`searching for ${query}...`)
    function downloadSuccess(msg) {
        id = msg;
        console.log(`downloaded file with download id ${msg}.`);
    }
    function printError(error) {
        console.log(`error: ${error}`);
    }
    function saveResults(items) {
        let results = [];
        for(let item of items) {
            if(item === null) continue;
            let desc = item.url;
            results.push(desc);
            console.log(desc);
        }
        console.log(`found ${results.length} items matching "${query}".`);
        let url = URL.createObjectURL(new Blob([JSON.stringify(results)], { type: "application/json" }));    
        let download = browser.downloads.download({url: url, filename: "bookmarks.json"}).then(downloadSuccess, printError);
    }    
    let search = browser.bookmarks.search(query)
                        .then(saveResults, printError);  
}
function revokeUrlAfterDownload(obj) {    
    if(obj.id === id && obj.state.current === "complete") {
        URL.revokeObjectURL(obj.url);
    }    
}
browser.browserAction.onClicked.addListener(doSearch);
browser.downloads.onChanged.addListener(revokeUrlAfterDownload);
console.log("loaded ucm bookmark extractor");