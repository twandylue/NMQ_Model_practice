const socket = io({
    reconnect: true
});

socket.on("connect", () => {
    console.log("Connected！");
});

class TaskPlot {
    constructor (name, targetNumber, doneNumber) {
        this.name = name;
        this.targetNumber = targetNumber;
        this.doneNumber = doneNumber;
    }
}

const task1 = new TaskPlot("Task 1", 30, 0);
const task2 = new TaskPlot("Task 2", 30, 0);

socket.on("newData", (message) => {
    const task = JSON.parse(message);
    // console.log(task);
    if (task.type === 1) task1.doneNumber++;
    else if (task.type === 2) task2.doneNumber++;

    const Target = {
        x: [task1.name, task2.name],
        y: [task1.targetNumber, task2.targetNumber],
        name: "Target task number",
        type: "bar"
    };

    const Done = {
        x: [task1.name, task2.name],
        y: [task1.doneNumber, task2.doneNumber],
        name: "Done task number",
        type: "bar"
    };

    const data = [Target, Done];
    const layout = { barmode: "group" };
    Plotly.newPlot("myDiv", data, layout);
});

const publishTask = document.querySelector("#send");
publishTask.addEventListener("click", async () => {
    const test = {
        data: [
            { type: 1, number: 30 }, { type: 2, number: 20 }
        ]
    };
    const data = JSON.stringify(test);
    const response = await fetch("/publishMessage", {
        body: data,
        method: "POST",
        headers: new Headers({
            "Content-Type": "application/json"
        })
    });
    console.log(response.status);
});
