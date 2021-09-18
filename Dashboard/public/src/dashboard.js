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

const task1 = new TaskPlot("Task 1", 0, 0);
const task2 = new TaskPlot("Task 2", 0, 0);

const publishTask = document.querySelector("#sendMsg");
const inputTask1 = document.querySelector("#task1-number");
const inputTask2 = document.querySelector("#task2-number");
publishTask.addEventListener("click", async () => {
    insertPlotyBlock(); // init block for ploty
    if (inputTask1.value < 0) inputTask1.value = 0;
    else if (inputTask2.value < 0) inputTask2.value = 0;
    task1.targetNumber = inputTask1.value;
    task2.targetNumber = inputTask2.value;
    const inputData = {
        data: [
            { type: 1, number: inputTask1.value }, { type: 2, number: inputTask2.value }
        ]
    };
    const data = JSON.stringify(inputData);
    const response = await fetch("/publishMessage", {
        body: data,
        method: "POST",
        headers: new Headers({
            "Content-Type": "application/json"
        })
    });
    console.log(response.status);
});

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

const insertPlotyBlock = () => {
    const ploty = document.createElement("div");
    ploty.id = "myDiv";
    ploty.className = "plot";

    document.querySelector("#init-title").remove();
    const existingParentElement = document.querySelector("#container");
    existingParentElement.insertBefore(ploty, existingParentElement.childNodes[0]);
};
