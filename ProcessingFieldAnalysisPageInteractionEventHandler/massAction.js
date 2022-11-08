(function (eventNames, convenienceApi) {
	var eventHandlers = {};

	function customActionHandler() { console.log("Click!") }

	function customClickHandler() {
		convenienceApi.fieldHelper.getHtmlElement("Documents").then(function (itemListElement) {

			//alert(itemListElement.selectedKeys);
			console.log(itemListElement.selectedKeys);

		});
	};

	eventHandlers[eventNames.ITEM_LIST_MODIFY_ACTIONS] = function (itemListActionsApi, view) {
		if (view.ObjectTypeID === 10) {
			itemListActionsApi.initialize();
			itemListActionsApi.addAction("Republish", customClickHandler, { title: "Republish" });
		}
	};
	return eventHandlers;
}(eventNames, convenienceApi));