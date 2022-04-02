define(function () {
    function toolbarHandler(api) {
        var toolbarId;
        var api = api;
        var workspaceArtifactId = api.startupInfo.workspaceId;

        fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/IsProcessingFieldObjectMaintenanceEnabled/${workspaceArtifactId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-Header': '-'
            }
        })
            .then(response => response.json())
            .then(data => {
                console.log('Success:', data);
                if (data.IsEnabled) {
                    console.log("is enabled is true");
                    toolbarId = api.toolbarService.createToolbar({
                        template: "<span>Greetings</span>",
                        buttons: [{ name: "Disable Processing Field Object Maintenance", eventName: "disable_proc_field_ob_maint" }],
                        init: function (scope) {
                            scope.$on("disable_proc_field_ob_maint", function () {
                                // on click - hide it
                                //api.toolbarService.hideToolbar(toolbarId);
                                disableProcessingFieldObjectMaintenance(workspaceArtifactId);
                            });
                        }
                    });
                } else {
                    console.log("is enabled is false");
                    toolbarId = api.toolbarService.createToolbar({
                        template: "<span>Greetings</span>",
                        buttons: [{ name: "Enable Processing Field Object Maintenance", eventName: "enable_proc_field_ob_maint" }],
                        init: function (scope) {
                            scope.$on("enable_proc_field_ob_maint", function () {
                                // on click - hide it
                                //api.toolbarService.hideToolbar(toolbarId);
                                enableProcessingFieldObjectMaintenance(workspaceArtifactId);
                            });
                        }
                    });

                }
            })
            .catch((error) => {
                console.error('Error:', error);
                return false;
            });


        function viewChange(data) {
            // Next, display it when view changes.
            //api.toolbarService.showToolbar(toolbarId);
            fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/IsProcessingFieldObjectMaintenanceEnabled/${workspaceArtifactId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-Header': '-'
                }
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Success:', data);
                    if (data.IsEnabled) {
                        toolbarId = api.toolbarService.createToolbar({
                            template: "<span>Greetings</span>",
                            buttons: [{ name: "Disable Processing Field Object Maintenance", eventName: "disable_proc_field_ob_maint" }],
                            init: function (scope) {
                                scope.$on("disable_proc_field_ob_maint", function () {
                                    // on click - hide it
                                    //api.toolbarService.hideToolbar(toolbarId);
                                    disableProcessingFieldObjectMaintenance(workspaceArtifactId);
                                });
                            }
                        });
                    } else {

                        toolbarId = api.toolbarService.createToolbar({
                            template: "<span>Greetings</span>",
                            buttons: [{ name: "Enable Processing Field Object Maintenance", eventName: "enable_proc_field_ob_maint" }],
                            init: function (scope) {
                                scope.$on("enable_proc_field_ob_maint", function () {
                                    // on click - hide it
                                    //api.toolbarService.hideToolbar(toolbarId);
                                    enableProcessingFieldObjectMaintenance(workspaceArtifactId);
                                });
                            }
                        });

                    }
                })
                .catch((error) => {
                    console.error('Error:', error);
                    return false;
                });
        }

        return {
            // Subscribe to the "onViewChanged" event.
            viewChange: viewChange
        }
    }

    function enableProcessingFieldObjectMaintenance(workspaceArtifactId) {
        fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/EnableProcessingFieldObjectMaintenance/${workspaceArtifactId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-Header': '-'
            }
        })
            .then(response => response.json())
            .then(data => {
                console.log('Success:', data);
            })
            .catch((error) => {
                console.error('Error:', error);
            });
    }

    function disableProcessingFieldObjectMaintenance(workspaceArtifactId) {
        fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/DisableProcessingFieldObjectMaintenance/${workspaceArtifactId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-Header': '-'
            }
        })
            .then(response => response.json())
            .then(data => {
                console.log('Success:', data);
            })
            .catch((error) => {
                console.error('Error:', error);
            });
    }

    //function isProcessingFieldObjectMaintenanceEnabled(workspaceArtifactId, api) {
    //    var api = api;
    //    fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/IsProcessingFieldObjectMaintenanceEnabled/${workspaceArtifactId}`, {
    //        method: 'GET',
    //        headers: {
    //            'Content-Type': 'application/json',
    //            'X-CSRF-Header': '-'
    //        }
    //    })
    //        .then(response => response.json())
    //        .then(data => {
    //            console.log('Success:', data);
    //            if (data.IsEnabled) {
    //                toolbarId = createToolbar({
    //                    template: "<span>Greetings</span>",
    //                    buttons: [{ name: "Disable Processing Field Object Maintenance", eventName: "disable_proc_field_ob_maint" }],
    //                    init: function (scope) {
    //                        scope.$on("disable_proc_field_ob_maint", function () {
    //                            // on click - hide it
    //                            //api.toolbarService.hideToolbar(toolbarId);
    //                            disableProcessingFieldObjectMaintenance(workspaceArtifactId);
    //                        });
    //                    }
    //                });
    //            } else {

    //                toolbarId = createToolbar({
    //                    template: "<span>Greetings</span>",
    //                    buttons: [{ name: "Enable Processing Field Object Maintenance", eventName: "enable_proc_field_ob_maint" }],
    //                    init: function (scope) {
    //                        scope.$on("enable_proc_field_ob_maint", function () {
    //                            // on click - hide it
    //                            //api.toolbarService.hideToolbar(toolbarId);
    //                            enableProcessingFieldObjectMaintenance(workspaceArtifactId);
    //                        });
    //                    }
    //                });

    //            }
    //        })
    //        .catch((error) => {
    //            console.error('Error:', error);
    //            return false;
    //        });
    //}



    return toolbarHandler;
});