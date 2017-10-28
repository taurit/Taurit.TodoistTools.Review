/// <reference path="knockout-3.4.2.debug.js" />
/// <reference path="jquery-3.1.1.intellisense.js" />

$(document).ready(function () {
    "use strict";

    // Define and initialize app's data model
    var ViewModel = function () {

        // Is all necessary data from API fully loaded?
        this.loaded = ko.observable(false);

        // Did any ajax error occur while loading
        this.ajaxError = ko.observable(false);

        // All labels defined by user in the right order 
        this.labels = ko.observableArray();

        // Tasks filtered to those that are worth reviewing (the logic of choice is in back end)
        this.tasks = ko.observableArray();

        // Index in the array of tasks of currently visible task in UI
        this.currentTaskIndex = ko.observable(0);

        // Current tasks
        this.currentTask = ko.computed(function () {
            var numTasks = this.tasks().length;
            var currentTask = numTasks > 0 ? this.tasks()[this.currentTaskIndex()] : null;
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
            this.displayTaskLabels();
        };

        // Moves to the previous task in the queue if it is valid operation in current state
        this.selectPreviousTask = function () {
            var currentIndex = this.currentTaskIndex();
            if (!this.isFirstTask()) {
                this.currentTaskIndex(currentIndex - 1);
            }
            this.displayTaskLabels();
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

            $(".label").removeClass("label-selected");
            taskLabels.forEach(function(taskLabelId) {
                $(".label[data-id=" + taskLabelId + "]").addClass("label-selected");
            });
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
    var makeChildrenObservables = function (object) {
        if (!ko.isObservable(object)) return;

        // Loop through its children
        for (var child in object()) {
            if (!ko.isObservable(object()[child])) {
                object()[child] = ko.observable(object()[child]);
            }
        }
    };

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
                    });
                    viewModel.tasks(data);
                    viewModel.displayTaskLabels();
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
        var label = ko.dataFor(this);
        $(this).toggleClass("label-selected");
        viewModel.updateTaskLabels();

        var contextWasSelected = $(this).hasClass("label-selected");
        if (contextWasSelected) { // as opposed to deselected
            // this brings assumption that user wants to select exactly one context. When it happens, next task in the queue will be displayed automatically (without need for confirmation)
            viewModel.selectNextTask();
        }
    });

    $(".reviewedTask").on("click", "#save", function () {
        viewModel.updateTaskLabels();

        $(".label-selected").removeClass("label-selected");
        viewModel.selectNextTask();
    });

    $(".reviewedTask").on("click", "#back", function () {
        viewModel.selectPreviousTask();
    });

    $(".reviewedTask").on("click", "#sync", function () {
        $.ajax({
            type: "POST",
            url: "/Home/UpdateTasks",
            data: ko.toJSON(viewModel.tasks),
            dataType: "json",
            contentType: "application/json",
            success: function (data) {
                window.location.reload();
            }
        });

    });


});