var TodoistTask = /** @class */ (function () {
    function TodoistTask() {
    }
    return TodoistTask;
}());
var TodoistTaskWithModifications = /** @class */ (function () {
    function TodoistTaskWithModifications(originalTask) {
        var _this = this;
        this.originalTask = originalTask;
        this.content = originalTask.content;
        this.description = originalTask.description;
        this.labels = ko.observableArray(originalTask.labels);
        this.estimatedTimeMinutes = ko.observable(originalTask.estimatedTimeMinutes);
        this.priority = originalTask.priority;
        this.timeFormatted = ko.computed(function () {
            var timeInMinutes = _this.estimatedTimeMinutes();
            var timeFormatted = "".concat(timeInMinutes, " min");
            if (timeInMinutes >= 60) {
                var hours = Math.floor(timeInMinutes / 60);
                timeFormatted = "".concat(hours, " h");
                var minutes = timeInMinutes % 60;
                if (minutes !== 0) {
                    timeFormatted += " ".concat(minutes, " min");
                }
            }
            return timeFormatted;
        }, this);
    }
    return TodoistTaskWithModifications;
}());
// Define and initialize app's data model
var ViewModel = /** @class */ (function () {
    function ViewModel() {
        var _this = this;
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
        this.currentTask = ko.computed(function () {
            var numTasks = _this.tasks().length;
            var currentTask = numTasks > 0 ? _this.tasks()[_this.currentTaskIndex()] : null;
            return currentTask;
        }, this);
    }
    // Is current task the last task?
    ViewModel.prototype.isLastTask = function () {
        var currentIndex = this.currentTaskIndex();
        var numTasks = this.tasks().length;
        return currentIndex + 1 === numTasks;
    };
    ;
    // Is current task the first task?
    ViewModel.prototype.isFirstTask = function () {
        var currentIndex = this.currentTaskIndex();
        return currentIndex === 0;
    };
    ;
    // Moves to the next task in the queue if it is valid operation in current state
    ViewModel.prototype.selectNextTask = function () {
        var currentIndex = this.currentTaskIndex();
        if (!this.isLastTask()) {
            this.currentTaskIndex(currentIndex + 1);
        }
        this.displayTaskLabels();
    };
    ;
    // Moves to the previous task in the queue if it is valid operation in current state
    ViewModel.prototype.selectPreviousTask = function () {
        var currentIndex = this.currentTaskIndex();
        if (!this.isFirstTask()) {
            this.currentTaskIndex(currentIndex - 1);
        }
        this.displayTaskLabels();
    };
    ;
    ViewModel.prototype.addTime = function (timeToAddInMinutes) {
        var timeBeforeOperation = this.currentTask().estimatedTimeMinutes();
        var newTime = timeToAddInMinutes === 0 ? 0 : timeBeforeOperation + timeToAddInMinutes;
        this.currentTask().estimatedTimeMinutes(newTime);
    };
    ;
    // Updates label collection in a task based on what is selected by the user.
    // The clean way to do this would be with two-way binding of labels,
    // but I want to keep the model simple
    ViewModel.prototype.updateTaskLabels = function () {
        if (this.tasks().length === 0)
            return;
        // get selected labels
        var selectedLabels = [];
        $(".label-selected").each(function () {
            var label = ko.dataFor(this);
            selectedLabels.push(label.name);
        });
        this.currentTask().labels(selectedLabels);
    };
    ;
    // Makes sure that labels associated with the task are highlighted (have a certain CSS class)
    ViewModel.prototype.displayTaskLabels = function () {
        if (this.tasks().length === 0)
            return;
        var taskLabels = this.currentTask().labels();
        $(".label[data-id=-1]").removeClass("hidden"); // "eliminate task" option should always be available
        $(".label").removeClass("label-selected");
        taskLabels.forEach(function (taskLabel) {
            $(".label[data-id=" + taskLabel + "]").addClass("label-selected");
        });
    };
    ;
    ViewModel.prototype.proceedToNextTaskIfInputForTaskIsComplete = function (actionIsSelection, howManyLabelsAreSelected) {
        var priorityIsNonDefault = this.currentTask().priority !== 1;
        var timeIsNonZero = this.currentTask().estimatedTimeMinutes() !== 0;
        if (priorityIsNonDefault && actionIsSelection && howManyLabelsAreSelected === 1 && timeIsNonZero) {
            // this brings assumption that user wants to select exactly one context. When it happens, next task in the queue will be displayed automatically (without need for confirmation)
            this.selectNextTask();
        }
    };
    ;
    // Saves the information that input data has been loaded
    ViewModel.prototype.loadFinished = function (loadFinishedWithError) {
        this.loaded(true);
        this.ajaxError(loadFinishedWithError);
    };
    ;
    return ViewModel;
}());
;
$(function () {
    "use strict";
    // ReSharper disable once TsResolvedFromInaccessibleModule
    ko.bindingHandlers.autosize = {
        init: function (element, valueAccessor) {
            var enabled = ko.unwrap(valueAccessor());
            if (enabled === true) {
                autosize(element);
            }
        }
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
                    var tasksWithModifications = new Array();
                    data.forEach(function (row) {
                        var updatedTodoistTask = new TodoistTaskWithModifications(row);
                        tasksWithModifications.push(updatedTodoistTask);
                    });
                    viewModel.tasks(tasksWithModifications);
                    viewModel.displayTaskLabels();
                    viewModel.loadFinished(false);
                },
                error: function () {
                    viewModel.loadFinished(true);
                }
            });
        },
        error: function () {
            viewModel.loadFinished(true);
        }
    });
    $(".reviewedTask").on("click", ".label", function () {
        $(this).toggleClass("label-selected");
        viewModel.updateTaskLabels();
        var actionIsSelection = $(this).hasClass("label-selected"); // and not deselection
        var howManyLabelsAreSelected = $(".reviewedTask .label-selected").length;
        viewModel.proceedToNextTaskIfInputForTaskIsComplete(actionIsSelection, howManyLabelsAreSelected);
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
//# sourceMappingURL=bundle.js.map