class TodoistTaskWithModifications
{
    constructor(public originalTask: TodoistTask)
    {
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

    content: string;
    description: string;
    
    /** The priority of the task (a number between 1 and 4, 4 for very urgent and 1 for default/undefined). */ 
    priority: KnockoutObservable<number>;
    labels: KnockoutObservableArray<string>;
    estimatedTimeMinutes: KnockoutObservable<number>;
    timeFormatted: KnockoutComputed<string>;
}
