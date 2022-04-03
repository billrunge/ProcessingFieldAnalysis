define(function () {
    function toolbarHandler(api) {
        var toolbarId;
        var api = api;
        var workspaceArtifactId = api.startupInfo.workspaceId;
        var isProcFieldObjectMaintEnabled = false;
        var isOtherMetadataAnaEnabled = false;

        function toolbarFactory() {

            var buttons = []

            var procFieldObjectMaintButton =
            {
                name: `${(isProcFieldObjectMaintEnabled) ? "Disable" : "Enable"} Processing Field Object Maintenance`,
                eventName: `${(isProcFieldObjectMaintEnabled) ? "disable" : "enable"}_proc_field_obj_maint`
            };

            buttons.push(procFieldObjectMaintButton);

            var forceprocFieldObjectMaintButton =
            {
                name:`Force Processing Field Object Maintenance`,
                eventName: `force_proc_field_obj_maint`
            }

            if (isProcFieldObjectMaintEnabled) {
                buttons.push(forceprocFieldObjectMaintButton);
            }

            var otherMetadataAnalysisButton =
            {
                name: `${(isOtherMetadataAnaEnabled) ? "Disable" : "Enable"} Other Metadata Analysis`,
                eventName: `${(isOtherMetadataAnaEnabled) ? "disable" : "enable"}_other_metadata_analysis`
            };

            buttons.push(otherMetadataAnalysisButton);

            var forceOtherMetadataAnalysisButton =
            {
                name: `Force Other Metadata Analysis`,
                eventName: `force_other_metadata_analysis`
            };

            if (isOtherMetadataAnaEnabled) {
                buttons.push(forceOtherMetadataAnalysisButton);
            }

            var toolbarId = api.toolbarService.createToolbar({
                toolbarClass: "proc-field-obj-toolbar",
                template: "",
                buttons: buttons,
                init: function (scope) {
                    scope.$on("enable_proc_field_obj_maint", function () {
                        enableProcessingFieldObjectMaintenance();
                    });
                    scope.$on("enable_other_metadata_analysis", function () {
                        enableOtherMetadataFieldAnalysis();
                    });
                    scope.$on("disable_proc_field_obj_maint", function () {
                        disableProcessingFieldObjectMaintenance();
                    });
                    scope.$on("disable_other_metadata_analysis", function () {
                        disableOtherMetadataFieldAnalysis();
                    });
                    scope.$on("force_proc_field_obj_maint", function () {
                        forceProcessingFieldObjectMaintenance();
                    });
                    scope.$on("force_other_metadata_analysis", function () {
                        forceOtherMetadataAnalysis();
                    });
                }
            });

            return toolbarId;

        };

        isProcessingFieldObjectMaintenanceEnabled();
        isOtherMetadataAnalysisEnabled();

        function isProcessingFieldObjectMaintenanceEnabled() {
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
                        isProcFieldObjectMaintEnabled = true;
                        api.toolbarService.hideToolbar(toolbarId);
                        toolbarId = toolbarFactory();
                        api.toolbarService.showToolbar(toolbarId);

                    }
                    else {
                        isProcFieldObjectMaintEnabled = false;
                        api.toolbarService.hideToolbar(toolbarId);
                        toolbarId = toolbarFactory();
                        api.toolbarService.showToolbar(toolbarId);
                    }
                })
                .catch((error) => {
                    console.error('Error:', error);
                    return false;
                });
        }

        function isOtherMetadataAnalysisEnabled() {
            fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/IsOtherMetadataAnalysisEnabled/${workspaceArtifactId}`, {
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
                        isOtherMetadataAnaEnabled = true;
                        api.toolbarService.hideToolbar(toolbarId);
                        toolbarId = toolbarFactory();
                        api.toolbarService.showToolbar(toolbarId);

                    }
                    else {
                        isOtherMetadataAnaEnabled = false;
                        api.toolbarService.hideToolbar(toolbarId);
                        toolbarId = toolbarFactory();
                        api.toolbarService.showToolbar(toolbarId);
                    }
                })
                .catch((error) => {
                    console.error('Error:', error);
                    return false;
                });
        }

        function enableProcessingFieldObjectMaintenance() {
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
                    isProcessingFieldObjectMaintenanceEnabled();
                })
                .catch((error) => {
                    console.error('Error:', error);
                });
        }

        function enableOtherMetadataFieldAnalysis() {
            fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/EnableOtherMetadataAnalysis/${workspaceArtifactId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-Header': '-'
                }
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Success:', data);
                    isOtherMetadataAnalysisEnabled();
                })
                .catch((error) => {
                    console.error('Error:', error);
                });
        }

        function disableProcessingFieldObjectMaintenance() {
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
                    isProcessingFieldObjectMaintenanceEnabled();
                })
                .catch((error) => {
                    console.error('Error:', error);
                });
        }

        function disableOtherMetadataFieldAnalysis() {
            fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/DisableOtherMetadataAnalysis/${workspaceArtifactId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-Header': '-'
                }
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Success:', data);
                    isOtherMetadataAnalysisEnabled();
                })
                .catch((error) => {
                    console.error('Error:', error);
                });
        }

        function forceProcessingFieldObjectMaintenance() {
            fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/ForceProcessingFieldObjectMaintenance/${workspaceArtifactId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-Header': '-'
                }
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Success:', data);
                    isOtherMetadataAnalysisEnabled();
                    isProcessingFieldObjectMaintenanceEnabled();
                })
                .catch((error) => {
                    console.error('Error:', error);
                });
        }

        function forceOtherMetadataAnalysis() {
            fetch(`/Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/ForceOtherMetadataAnalysis/${workspaceArtifactId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-Header': '-'
                }
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Success:', data);
                    isOtherMetadataAnalysisEnabled();
                    isProcessingFieldObjectMaintenanceEnabled();
                })
                .catch((error) => {
                    console.error('Error:', error);
                });
        }

        function viewChange(data) {
            api.toolbarService.showToolbar(toolbarId);
        }

        return {
            viewChange: viewChange
        }
    }

    return toolbarHandler;
});