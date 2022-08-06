class TodoistTaskWithModifications {
    originalTask: TodoistTask;
    content: string;
    description: string;
    priority: number;
    labels: KnockoutObservableArray<Label>;
    estimatedTimeMinutes: KnockoutObservable<number>;
    timeFormatted: KnockoutComputed<string>;
}
