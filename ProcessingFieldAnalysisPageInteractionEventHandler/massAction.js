(function (eventNames, convenienceApi) {
    var eventHandlers = {};
    var workspaceArtifactId;

    function customActionHandler() { console.log("Click!") }

    function customClickHandler() {
        convenienceApi.fieldHelper.getHtmlElement("Documents").then(function (itemListElement) {
            var documentArtifactIds = itemListElement.selectedKeys + '';
            var documentArtifactIdsArray = [];
            documentArtifactIdsArray = documentArtifactIds.split(',');

            fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Publish/${workspaceArtifactId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-Header': '-'
                },
                body: JSON.stringify({ "documentArtifactIds": documentArtifactIdsArray}),
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Success:', data);
                })
                .catch((error) => {
                    console.error('Error:', error);
                });
        });
    };

    eventHandlers[eventNames.REPLACE_OBTAIN_ADDITIONAL_DATA] = function (
        fieldsRequiringAdditionalData,
        workspaceId,
        viewModelName
    ) {
        workspaceArtifactId = workspaceId;
    }

    eventHandlers[eventNames.ITEM_LIST_MODIFY_ACTIONS] = function (itemListActionsApi, view) {
        if (view.ObjectTypeID === 10) {
            itemListActionsApi.initialize();
            itemListActionsApi.addAction("Republish", customClickHandler, { title: "Republish" });
        }
    };
    return eventHandlers;
}(eventNames, convenienceApi));