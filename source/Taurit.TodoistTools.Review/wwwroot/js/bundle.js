class Label {
    name;
}
class TodoistTask {
    content;
    description;
    priority;
    labels;
}
class TodoistTaskWithModifications {
    originalTask;
    constructor(originalTask) {
        this.originalTask = originalTask;
        this.content = originalTask.content;
        this.description = originalTask.description;
        this.labels = ko.observableArray(originalTask.labels.map(x => x.name));
        this.priority = ko.observable(originalTask.priority);
    }
    content;
    description;
    priority;
    labels;
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
        if (priorityIsNonDefault && actionIsSelection && howManyLabelsAreSelected === 1) {
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
        const label = eventObject.currentTarget;
        const labelName = label.dataset["id"];
        const labelViewModel = viewModel.labels().find(x => x.name === labelName);
        const oldValue = labelViewModel.isSelected();
        labelViewModel.isSelected(!oldValue);
        viewModel.updateTaskLabels();
        const actionIsSelection = oldValue === false;
        const howManyLabelsAreSelected = viewModel.labels().filter(x => x.isSelected()).length;
    });
    $(".reviewedTask").on("click", ".priority", function () {
        const selectedPriority = $(this).data('priority');
        viewModel.currentTask().priority(selectedPriority);
    });
    $(".reviewedTask").on("click", "#save", () => {
        viewModel.updateTaskLabels();
        viewModel.selectNextTask();
    });
    $(".reviewedTask").on("click", "#back", () => {
        viewModel.selectPreviousTask();
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