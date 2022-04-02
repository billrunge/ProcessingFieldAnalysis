// JavaScript source code
define(function () {
    function sampleHandler(api) {
        var toolbarId;
        var api = api;

        // First, create a toolbar.
        toolbarId = api.toolbarService.createToolbar({
            template: "<span>Greetings</span>",
            buttons: [{ name: "Close", eventName: "close_toolbar" }],
            init: function (scope) {
                scope.$on("close_toolbar", function () {
                    // on click - hide it
                    api.toolbarService.hideToolbar(toolbarId);
                });
            }
        });

        function viewChange(data) {
            // Next, display it when view changes.
            api.toolbarService.showToolbar(toolbarId);
        }

        return {
            // Subscribe to the "onViewChanged" event.
            viewChange: viewChange
        }
    }
});