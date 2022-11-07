(function (eventNames, convenienceApi) {
    var eventHandlers = [];
    eventHandlers[eventNames.PAGE_LOAD_COMPLETE] = function () {
        console.log("Page load complete!");
    }
    return eventHandlers
}(eventNames, convenienceApi));