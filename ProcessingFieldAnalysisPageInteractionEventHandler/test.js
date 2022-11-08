(function (eventNames, convenienceApi) {
    var eventHandlers = [];
    eventHandlers[eventNames.ITEM_LIST_MODIFY_ACTIONS] = function (itemListActionsApi, view) {
        //if (view.ObjectTypeID === 10) {

            itemListActionsApi.initialize();
            const customClickHandler = function () {
                convenienceApi.fieldHelper.getHtmlElement("Documents").then(function (itemListElement) {
                    alert(itemListElement.selectedKeys);
                });
            };
            itemListActionsApi.addAction("custom", customClickHandler, { title: "custom" });

        //}
    };
    return eventHandlers;
}(eventNames, convenienceApi));