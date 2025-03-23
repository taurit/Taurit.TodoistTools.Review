class TodoistTaskWithModifications
{
    constructor(public originalTask: TodoistTask)
    {
        this.content = originalTask.content;
        this.description = originalTask.description;
        this.labels = ko.observableArray(originalTask.labels.map(x => x.name));
        this.priority = ko.observable(originalTask.priority);
    }

    content: string;
    description: string;
    
    /** The priority of the task (a number between 1 and 4, 4 for very urgent and 1 for default/undefined). */ 
    priority: KnockoutObservable<number>;
    labels: KnockoutObservableArray<string>;
}
