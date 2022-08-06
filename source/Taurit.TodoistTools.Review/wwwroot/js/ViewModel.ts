﻿// Define and initialize app's data model
class ViewModel {
    loaded: KnockoutObservable<Boolean>;
    ajaxError: KnockoutObservable<Boolean>;
    showPriority: KnockoutObservable<Boolean>;
    labels: KnockoutObservableArray<String>;
    tasks: KnockoutObservableArray<TodoistTaskWithModifications>;
    currentTaskIndex: KnockoutObservable<number>;
    currentTask: KnockoutComputed<TodoistTaskWithModifications>;

    constructor() {
        // Is all necessary data from API fully loaded?
        this.loaded = ko.observable<Boolean>(false);

        // Did any ajax error occur while loading
        this.ajaxError = ko.observable<Boolean>(false);

        // All labels defined by user in the right order 
        this.labels = ko.observableArray<String>();

        // Tasks filtered to those that are worth reviewing (the logic of choice is in back end)
        this.tasks = ko.observableArray<TodoistTaskWithModifications>();

        // Index in the array of tasks of currently visible task in UI
        this.currentTaskIndex = ko.observable(0);

        // @ts-ignore true that it can be null, but i don't want to deal with it now
        this.currentTask = ko.computed(() => {
            var numTasks = this.tasks().length;
            var currentTask = numTasks > 0 ? this.tasks()[this.currentTaskIndex()] : null;
            return currentTask;
        }, this);

        this.showPriority = ko.computed(() => {
            var currentTask = this.currentTask();
            if (currentTask != null) {
                var priority = currentTask.priority();
                if (priority < 1 || priority > 4) {
                    throw new Error(`Expected priority in range 1-4 inclusive, but got ${priority}`);
                }
                return priority == 1;
            }
            return false;
        }, this);
    }

    // Is current task the last task?
    isLastTask() {
        const currentIndex = this.currentTaskIndex();
        const numTasks = this.tasks().length;
        return currentIndex + 1 === numTasks;
    };

    // Is current task the first task?
    isFirstTask() {
        const currentIndex = this.currentTaskIndex();
        return currentIndex === 0;
    };

    // Moves to the next task in the queue if it is valid operation in current state
    selectNextTask() {
        const currentIndex = this.currentTaskIndex();
        if (!this.isLastTask()) {
            this.currentTaskIndex(currentIndex + 1);
        }

        this.displayTaskLabels();
    };

    // Moves to the previous task in the queue if it is valid operation in current state
    selectPreviousTask() {
        const currentIndex = this.currentTaskIndex();
        if (!this.isFirstTask()) {
            this.currentTaskIndex(currentIndex - 1);
        }

        this.displayTaskLabels();
    };

    addTime(timeToAddInMinutes: number) {
        const timeBeforeOperation = this.currentTask().estimatedTimeMinutes();
        const newTime = timeToAddInMinutes === 0 ? 0 : timeBeforeOperation + timeToAddInMinutes;
        this.currentTask().estimatedTimeMinutes(newTime);
    };

    // Updates label collection in a task based on what is selected by the user.
    // The clean way to do this would be with two-way binding of labels,
    // but I want to keep the model simple
    updateTaskLabels() {
        if (this.tasks().length === 0) return;

        // get selected labels
        const selectedLabels: string[] = [];
        $(".label-selected").each(function () {
            const label = ko.dataFor(this);
            selectedLabels.push(label.name);
        });

        this.currentTask().labels(selectedLabels);
    };

    // Makes sure that labels associated with the task are highlighted (have a certain CSS class)
    displayTaskLabels() {
        if (this.tasks().length === 0) return;

        const taskLabels = this.currentTask().labels();

        $(".label[data-id=-1]").removeClass("hidden"); // "eliminate task" option should always be available

        $(".label").removeClass("label-selected");
        taskLabels.forEach((taskLabel: string) => {
            $(".label[data-id=" + taskLabel + "]").addClass("label-selected");
        });
    };

    proceedToNextTaskIfInputForTaskIsComplete(actionIsSelection: boolean, howManyLabelsAreSelected: number) {
        var priorityIsNonDefault = this.currentTask().priority() !== 1;
        var timeIsNonZero = this.currentTask().estimatedTimeMinutes() !== 0;
        if (priorityIsNonDefault && actionIsSelection && howManyLabelsAreSelected === 1 && timeIsNonZero) {
            // this brings assumption that user wants to select exactly one context. When it happens, next task in the queue will be displayed automatically (without need for confirmation)
            this.selectNextTask();
        }
    };

    // Saves the information that input data has been loaded
    loadFinished(loadFinishedWithError: boolean) {
        this.loaded(true);
        this.ajaxError(loadFinishedWithError);
    };
};
