class Label {
    name;
}
class TodoistTask {
    content;
    description;
    priority;
    labels;
    estimatedTimeMinutes;
}
class TodoistTaskWithModifications {
    originalTask;
    constructor(originalTask) {
        this.originalTask = originalTask;
        this.content = originalTask.content;
        this.description = originalTask.description;
        this.labels = ko.observableArray(originalTask.labels.map(x => x.name));
        this.estimatedTimeMinutes = ko.observable(originalTask.estimatedTimeMinutes);
        this.priority = ko.observable(originalTask.priority);
        this.timeFormatted = ko.computed(() => {
            let timeInMinutes = this.estimatedTimeMinutes();
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
    }
    content;
    description;
    priority;
    labels;
    estimatedTimeMinutes;
    timeFormatted;
}
class LabelViewModel {
    name;
    isSelected;
    constructor(name, isSelected) {
        this.name = name;
        this.isSelected = isSelected;
    }
}
class ViewModel {
    loaded;
    ajaxError;
    showPriority;
    showLabels;
    labels;
    tasks;
    currentTaskIndex;
    currentTask;
    constructor() {
        this.loaded = ko.observable(false);
        this.ajaxError = ko.observable(false);
        this.labels = ko.observableArray();
        this.tasks = ko.observableArray();
        this.currentTaskIndex = ko.observable(0);
        this.currentTask = ko.computed(() => {
            var numTasks = this.tasks().length;
            var currentTask = numTasks > 0 ? this.tasks()[this.currentTaskIndex()] : null;
            return currentTask;
        }, this);
        this.showPriority = ko.computed(() => {
            return true;
            var currentTask = this.currentTask();
            if (currentTask != null) {
                var priority = currentTask.priority();
                if (priority < 1 || priority > 4) {
                    throw new Error(`Expected priority in range 1-4 inclusive, but got ${priority}`);
                }
                return priority === 1;
            }
            return false;
        }, this);
        this.showLabels = ko.computed(() => {
            var currentTask = this.currentTask();
            return currentTask != null && currentTask.labels.length !== 1;
        }, this);
    }
    isLastTask() {
        const currentIndex = this.currentTaskIndex();
        const numTasks = this.tasks().length;
        return currentIndex + 1 === numTasks;
    }
    ;
    isFirstTask() {
        const currentIndex = this.currentTaskIndex();
        return currentIndex === 0;
    }
    ;
    selectNextTask() {
        const currentIndex = this.currentTaskIndex();
        if (!this.isLastTask()) {
            this.currentTaskIndex(currentIndex + 1);
        }
        this.displayTaskLabels();
    }
    ;
    selectPreviousTask() {
        const currentIndex = this.currentTaskIndex();
        if (!this.isFirstTask()) {
            this.currentTaskIndex(currentIndex - 1);
        }
        this.displayTaskLabels();
    }
    ;
    addTime(timeToAddInMinutes) {
        const timeBeforeOperation = this.currentTask().estimatedTimeMinutes();
        const newTime = timeToAddInMinutes === 0 ? 0 : timeBeforeOperation + timeToAddInMinutes;
        this.currentTask().estimatedTimeMinutes(newTime);
    }
    ;
    updateTaskLabels() {
        if (this.tasks().length === 0)
            return;
        const selectedLabels = [];
        $(".label-selected").each(function () {
            const label = ko.dataFor(this);
            selectedLabels.push(label.name);
        });
        this.currentTask().labels(selectedLabels);
    }
    ;
    displayTaskLabels() {
        if (this.tasks().length === 0)
            return;
        const taskLabels = this.currentTask().labels();
        this.labels().forEach((labelViewModel) => {
            const labelIsSelected = taskLabels.filter(x => x === labelViewModel.name).length > 0;
            labelViewModel.isSelected(labelIsSelected);
        });
    }
    ;
    proceedToNextTaskIfInputForTaskIsComplete(actionIsSelection, howManyLabelsAreSelected) {
        var priorityIsNonDefault = this.currentTask().priority() !== 1;
        var timeIsNonZero = this.currentTask().estimatedTimeMinutes() !== 0;
        if (priorityIsNonDefault && actionIsSelection && howManyLabelsAreSelected === 1 && timeIsNonZero) {
            this.selectNextTask();
        }
    }
    ;
    loadFinished(loadFinishedWithError) {
        this.loaded(true);
        this.ajaxError(loadFinishedWithError);
    }
    ;
}
;
$(() => {
    "use strict";
    ko.bindingHandlers.autosize = {
        init(element, valueAccessor) {
            const enabled = ko.unwrap(valueAccessor());
            if (enabled === true) {
                autosize(element);
            }
        }
    };
    const viewModel = new ViewModel();
    ko.applyBindings(viewModel);
    $.ajax({
        type: "GET",
        url: "/Home/GetAllLabels",
        data: {},
        success: (data) => {
            viewModel.labels(data.map(x => new LabelViewModel(x.name, ko.observable(false))));
            $.ajax({
                type: "GET",
                url: "/Home/GetTasksToReview",
                data: {},
                success: data => {
                    let tasksWithModifications = new Array();
                    data.forEach((row) => {
                        const updatedTodoistTask = new TodoistTaskWithModifications(row);
                        tasksWithModifications.push(updatedTodoistTask);
                    });
                    viewModel.tasks(tasksWithModifications);
                    viewModel.displayTaskLabels();
                    viewModel.loadFinished(false);
                },
                error: () => {
                    viewModel.loadFinished(true);
                }
            });
        },
        error: () => {
            viewModel.loadFinished(true);
        }
    });
    $(".reviewedTask").on("click", ".label", function (eventObject) {
        const label = eventObject.target;
        const labelName = label.dataset["id"];
        const labelViewModel = viewModel.labels().find(x => x.name === labelName);
        const oldValue = labelViewModel.isSelected();
        labelViewModel.isSelected(!oldValue);
        viewModel.updateTaskLabels();
        const actionIsSelection = oldValue === false;
        const howManyLabelsAreSelected = viewModel.labels().filter(x => x.isSelected()).length;
        viewModel.proceedToNextTaskIfInputForTaskIsComplete(actionIsSelection, howManyLabelsAreSelected);
    });
    $(".reviewedTask").on("click", ".priority", function () {
        const selectedPriority = $(this).data('priority');
        viewModel.currentTask().priority(selectedPriority);
        const howManyLabelsAreSelected = viewModel.labels().filter(x => x.isSelected()).length;
        viewModel.proceedToNextTaskIfInputForTaskIsComplete(true, howManyLabelsAreSelected);
    });
    $(".reviewedTask").on("click", "#save", () => {
        viewModel.updateTaskLabels();
        $(".label-selected").removeClass("label-selected");
        viewModel.selectNextTask();
    });
    $(".reviewedTask").on("click", "#back", () => {
        viewModel.selectPreviousTask();
    });
    $(".reviewedTask").on("click", ".time", function () {
        var timeToAddInMinutes = parseInt($(this).data('time-to-add'));
        viewModel.addTime(timeToAddInMinutes);
    });
    $(".reviewedTask").on("click", "#sync", () => {
        $.ajax({
            type: "POST",
            url: "/Home/UpdateTasks",
            data: ko.toJSON(viewModel.tasks),
            contentType: "application/json",
            success() {
                window.location.reload();
            },
            error(msg) {
                console.error(msg);
            }
        });
    });
    $("#all-done").on("click", "#reload", () => {
        location.reload();
    });
});
//# sourceMappingURL=bundle.js.map