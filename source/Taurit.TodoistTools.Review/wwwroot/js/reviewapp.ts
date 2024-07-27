$(() => {
    "use strict";

    // ReSharper disable once TsResolvedFromInaccessibleModule
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

    // Initialize: load all necessary data in only two API-calls
    $.ajax({
        type: "GET",
        url: "/Home/GetAllLabels",
        data: {},
        success: (data: Label[]) => {
            viewModel.labels(data.map(x => new LabelViewModel(x.name, ko.observable(false))));

            $.ajax({
                type: "GET",
                url: "/Home/GetTasksToReview",
                data: {},
                success: data => {
                    let tasksWithModifications = new Array<TodoistTaskWithModifications>();

                    data.forEach((row: TodoistTask) => {
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

    $(".reviewedTask").on("click", ".label", function (eventObject: Event) {
        const label = eventObject.currentTarget as HTMLDivElement;
        const labelName = label.dataset["id"];
        const labelViewModel = viewModel.labels().find(x => x.name === labelName) as LabelViewModel;
        
        const oldValue = labelViewModel.isSelected();
        labelViewModel.isSelected(!oldValue);
        viewModel.updateTaskLabels();

        const actionIsSelection = oldValue === false; // and not deselection
        const howManyLabelsAreSelected = viewModel.labels().filter(x => x.isSelected()).length;
    });

    $(".reviewedTask").on("click", ".priority", function () {
        const selectedPriority = $(this).data('priority');
        viewModel.currentTask().priority(selectedPriority);
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
