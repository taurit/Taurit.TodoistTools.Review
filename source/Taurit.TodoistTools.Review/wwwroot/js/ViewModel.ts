class LabelViewModel {
    constructor(
        public name: string,
        public isSelected: KnockoutObservable<boolean>)
    {
    }
}

class ViewModel {
    loaded: KnockoutObservable<Boolean>;
    ajaxError: KnockoutObservable<Boolean>;
    showPriority: KnockoutObservable<Boolean>;
    labels: KnockoutObservableArray<LabelViewModel>;
    tasks: KnockoutObservableArray<TodoistTaskWithModifications>;
    currentTaskIndex: KnockoutObservable<number>;
    currentTask: KnockoutComputed<TodoistTaskWithModifications>;

    constructor() {
        // Is all necessary data from API fully loaded?
        this.loaded = ko.observable<Boolean>(false);

        // Did any ajax error occur while loading
        this.ajaxError = ko.observable<Boolean>(false);

        // All labels defined by user in the right order 
        this.labels = ko.observableArray<LabelViewModel>();

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

    
    // Updates label collection in a task based on what is selected by the user.
    // The clean way to do this would be with two-way binding of labels (?),
    // but I want to keep the model simple
    updateTaskLabels() {
        if (this.tasks().length === 0) return;

        // get selected labels
        const selectedLabels: string[] = [];
        $(".label-selected").each(function () {
            const label: Label = ko.dataFor(this);
            selectedLabels.push(label.name);
        });

        this.currentTask().labels(selectedLabels);
    };

    // Makes sure that labels associated with the task are highlighted (have a certain CSS class)
    displayTaskLabels() {
        if (this.tasks().length === 0) return;

        const taskLabels = this.currentTask().labels();

        this.labels().forEach((labelViewModel: LabelViewModel) => {
            const labelIsSelected = taskLabels.filter(x => x === labelViewModel.name).length > 0;
            labelViewModel.isSelected(labelIsSelected);
        });
    };

    proceedToNextTaskIfInputForTaskIsComplete(actionIsSelection: boolean, howManyLabelsAreSelected: number) {
        var priorityIsNonDefault = this.currentTask().priority() !== 1;
        if (priorityIsNonDefault && actionIsSelection && howManyLabelsAreSelected === 1) {
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
