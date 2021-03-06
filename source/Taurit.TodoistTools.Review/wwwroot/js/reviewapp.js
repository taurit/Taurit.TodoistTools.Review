﻿/// <reference path="../lib/knockout-3.5.1.debug.js" />
/// <reference path="../lib/autosize.min.js" />
/// <reference path="../lib/jquery-3.4.1-vsdoc.js" />

$(document).ready(function () {
    "use strict";

    ko.bindingHandlers.autosize = {
        init: function(element, valueAccessor) {
            var enabled = ko.unwrap(valueAccessor());
            if (enabled === true) {
                autosize(element);
            }
        }
    };

    // Define and initialize app's data model
    var ViewModel = function () {

        // Is all necessary data from API fully loaded?
        this.loaded = ko.observable(false);

        // Did any ajax error occur while loading
        this.ajaxError = ko.observable(false);

        this.timeEstimateReviewNeeded = ko.observable(false);
        this.labelsReviewNeeded = ko.observable(false);
        this.priorityReviewNeeded = ko.observable(false);

        // All labels defined by user in the right order 
        this.labels = ko.observableArray();

        // Tasks filtered to those that are worth reviewing (the logic of choice is in back end)
        this.tasks = ko.observableArray();

        // Index in the array of tasks of currently visible task in UI
        this.currentTaskIndex = ko.observable(0);

        // Current tasks
        var viewModel = this;

        this.currentTask = ko.computed(function () {
            var numTasks = viewModel.tasks().length;
            var currentTask = numTasks > 0 ? viewModel.tasks()[viewModel.currentTaskIndex()] : null;
            return currentTask;
        }, this);
        
        // Is current task the last task?
        this.isLastTask = function() {
            var currentIndex = this.currentTaskIndex();
            var numTasks = this.tasks().length;
            return currentIndex + 1 === numTasks;
        };

        // Is current task the first task?
        this.isFirstTask = function() {
            var currentIndex = this.currentTaskIndex();
            return currentIndex === 0;
        };

        // Moves to the next task in the queue if it is valid operation in current state
        this.selectNextTask = function () {
            var currentIndex = this.currentTaskIndex();
            if (!this.isLastTask()) {
                this.currentTaskIndex(currentIndex + 1);
            }
            
            this.updateReviewSectionsVisibilityForNextTask();
            this.displayTaskLabels();
        };

        // Moves to the previous task in the queue if it is valid operation in current state
        this.selectPreviousTask = function () {
            var currentIndex = this.currentTaskIndex();
            if (!this.isFirstTask()) {
                this.currentTaskIndex(currentIndex - 1);
            }
            
            this.makeAllReviewSectionsVisible();
            this.displayTaskLabels();
        };

        this.addTime = function (timeToAddInMinutes) {
            var timeBeforeOperation = this.currentTask().time();
            var newTime = timeToAddInMinutes === 0 ? 0 : timeBeforeOperation + timeToAddInMinutes;
            this.currentTask().time(newTime);
        };

        // Updates label collection in a task based on what is selected by the user.
        // The clean way to do this would be with two-way binding of labels,
        // but I want to keep the model simple
        this.updateTaskLabels = function () {
            if (this.tasks().length === 0) return;

            // get selected labels
            var selectedLabels = [];
            $(".label-selected").each(function () {
                var label = ko.dataFor(this);
                selectedLabels.push(label.id);
            });

            this.currentTask().labels(selectedLabels);
        };

        // Makes sure that labels associated with the task are highlighted (have a certain CSS class)
        this.displayTaskLabels = function() {
            if (this.tasks().length === 0) return;

            var taskLabels = this.currentTask().labels();

            if (this.labelsReviewNeeded()) {
                $(".label").removeClass("hidden");
            } else {
                $(".label").addClass("hidden");
            }
            $(".label[data-id=-1]").removeClass("hidden"); // "eliminate task" option should always be available

            $(".label").removeClass("label-selected");
            taskLabels.forEach(function(taskLabelId) {
                $(".label[data-id=" + taskLabelId + "]").addClass("label-selected");
            });
        };

        this.updateReviewSectionsVisibilityForNextTask = function() {

            let currentTask = this.currentTask();
            if (currentTask !== null && currentTask !== undefined) {
                // there's at least one task to review
                const currentTaskOriginalTime = currentTask.originalTime;
                const timeReviewNeeded = currentTaskOriginalTime === 0;

                const priorityReviewNeeded = currentTask.priority === 1;

                const labelsNeedReview = currentTask.labels().length !== 1;

                this.timeEstimateReviewNeeded(timeReviewNeeded);
                this.priorityReviewNeeded(priorityReviewNeeded);
                this.labelsReviewNeeded(labelsNeedReview);
            }
            
        };

        this.makeAllReviewSectionsVisible = function() {
            // when navigating to the previous task, we usually know we made a mistake, so we want to be able to correct the metadata even if it's already set (so in theory it doesn't need review)
            this.timeEstimateReviewNeeded(true);
            this.labelsReviewNeeded(true);
            this.priorityReviewNeeded(true);
        };

        this.proceedToNextTaskIfInputForTaskIsComplete =
            function (actionIsSelection, howManyLabelsAreSelected) {
                var priorityIsNonDefault = this.currentTask().priority !== 1;
                var timeIsNonZero = this.currentTask().time() !== 0;
                if (priorityIsNonDefault && actionIsSelection && howManyLabelsAreSelected === 1 && timeIsNonZero) {
                    // this brings assumption that user wants to select exactly one context. When it happens, next task in the queue will be displayed automatically (without need for confirmation)
                    viewModel.selectNextTask();
                }
            };

        // Saves the information that input data has been loaded
        this.loadFinished = function(withError) {
            this.loaded(true);
            this.ajaxError(withError);
        };
    };
    
    var viewModel = new ViewModel();
    ko.applyBindings(viewModel);

    // Initialize: load all necessary data in only two API-calls
    $.ajax({
        type: "GET",
        url: "/Home/GetAllLabels",
        data: {},
        success: function (data) {
            viewModel.labels(data);

            $.ajax({
                type: "GET",
                url: "/Home/GetTasksToReview",
                data: {},
                success: function (data) {
                    data.forEach(function (row) {
                        row.labels = ko.observableArray(row.labels);
                        row.time = ko.observable(row.time);
                        row.timeFormatted = ko.computed(function() {
                            let timeInMinutes = row.time();
                            let timeFormatted = `${timeInMinutes} min`;
                            if (timeInMinutes >= 60) {
                                let hours = Math.floor(timeInMinutes / 60);
                                timeFormatted = `${hours} h`;
                                let minutes = timeInMinutes % 60;
                                if (minutes !== 0) {
                                    timeFormatted += ` ${minutes} min`;
                                }
                            }

                            return timeFormatted;
                        }, this);
                    });
                    viewModel.tasks(data);
                    viewModel.displayTaskLabels();
                    viewModel.updateReviewSectionsVisibilityForNextTask();
                    viewModel.loadFinished(false);

                },
                error: function () {
                    viewModel.loadFinished(true);
                }
            });

        },
        error: function() {
            viewModel.loadFinished(true);
        }
    });

    // define app behavious
    $(".reviewedTask").on("click", ".label", function () {
        $(this).toggleClass("label-selected");
        viewModel.updateTaskLabels();

        var actionIsSelection = $(this).hasClass("label-selected"); // and not deselection
        var howManyLabelsAreSelected = $(".reviewedTask .label-selected").length;
        viewModel.proceedToNextTaskIfInputForTaskIsComplete(actionIsSelection, howManyLabelsAreSelected);
    });

    $(".reviewedTask").on("click", "#show-all-sections", function () {
        viewModel.makeAllReviewSectionsVisible();
    });

    $(".reviewedTask").on("click", ".priority", function () {
        var selectedPriority = $(this).data('priority');
        viewModel.currentTask().priority = selectedPriority; // not observable
        viewModel.currentTaskIndex.valueHasMutated(); // so force refresh of computed property this way

        var howManyLabelsAreSelected = $(".reviewedTask .label-selected").length;
        viewModel.proceedToNextTaskIfInputForTaskIsComplete(true, howManyLabelsAreSelected);
    });

    $(".reviewedTask").on("click", "#save", function () {
        viewModel.updateTaskLabels();

        $(".label-selected").removeClass("label-selected");
        viewModel.selectNextTask();
    });

    $(".reviewedTask").on("click", "#back", function () {
        viewModel.selectPreviousTask();
    });

    $(".reviewedTask").on("click", ".time", function () {
        var timeToAddInMinutes = parseInt($(this).data('time-to-add'));
        viewModel.addTime(timeToAddInMinutes);
    });
    
    $(".reviewedTask").on("click", "#sync", function () {
        $.ajax({
            type: "POST",
            url: "/Home/UpdateTasks",
            data: ko.toJSON(viewModel.tasks),
            dataType: "json",
            contentType: "application/json",
            success: function () {
                window.location.reload();
            }
        });

    });

    $("#all-done").on("click", "#reload", function () {
        location.reload();
    });


});