const socket = io({
    reconnect: true
});

socket.on("connect", () => {
    console.log("Connected！");
});

const taskList = {};
const x = [];
const y = [];
// socket.on("newData", (message) => {
//     const task = JSON.parse(message);
//     if (taskList[task.type] === undefined) taskList[task.type] = 1;
//     else taskList[task.type]++;
//     console.log(taskList);

//     x.push(taskList[task.type]);
//     y.push(taskList[task.type]);
//     const trace1 = {
//         x: x,
//         y: y,
//         type: "scatter"
//     };

//     const trace2 = {
//         x: [1, 2, 3, 4],
//         y: [16, 5, 11, 9],
//         type: "scatter"
//     };

//     const data = [trace1, trace2];

//     Plotly.newPlot("myDiv", data);
// });

socket.on("newData", (message) => {
    const task = JSON.parse(message);
    if (taskList[task.type] === undefined) taskList[task.type] = 1;
    else taskList[task.type]++;
    console.log(taskList);

    const trace1 = {
        x: ["giraffes", "orangutans", "monkeys"],
        y: [20, 14, 23],
        name: "SF Zoo",
        type: "bar"
    };

    const trace2 = {
        x: ["giraffes", "orangutans", "monkeys"],
        y: [12, 18, 29],
        name: "LA Zoo",
        type: "bar"
    };

    const data = [trace1, trace2];

    const layout = { barmode: "stack" };

    Plotly.newPlot("myDiv", data, layout);
});

// const x = [];
// const y = [];
// const plotLine = (x, y) => {
//     const trace = {
//         x: x,
//         y: y,
//         type: "scatter"
//     };
//     const data = [trace];
//     Plotly.newPlot("myDiv", data);
// };

// const trace1 = {
//     x: [1, 2, 3, 4],
//     y: [10, 15, 13, 17],
//     type: "scatter"
// };

// const trace2 = {
//     x: [1, 2, 3, 4],
//     y: [16, 5, 11, 9],
//     type: "scatter"
// };

// const data = [trace1, trace2];

// Plotly.newPlot("myDiv", data);

// function TotalRevenue () {
//     const xhr = new XMLHttpRequest();
//     xhr.onreadystatechange = function () {
//         if (xhr.readyState === 4) {
//             if (xhr.status === 200) {
//                 const ans = JSON.parse(xhr.responseText);
//                 // console.log(ans)
//                 const number = ans.data["Total Revenue"];
//                 document.querySelector("#number").innerHTML = "Total Revenue: " + number;
//             }
//         }
//     };
//     xhr.open("GET", "/api/1.0/TotalRevenue");
//     xhr.send();
// };
// TotalRevenue();

// function PieChart () {
//     const xhr = new XMLHttpRequest();
//     xhr.onreadystatechange = function () {
//         if (xhr.readyState === 4) {
//             if (xhr.status === 200) {
//                 const ans = JSON.parse(xhr.responseText);
//                 // console.log(ans)
//                 const values = [];
//                 const labels = [];
//                 const colors = [];
//                 for (const i in ans.data) {
//                     values.push(ans.data[i]["SUM(qty)"]);
//                     labels.push(ans.data[i].color_name);
//                     colors.push(ans.data[i].color_code);
//                 }

//                 const colorData = [{
//                     values: values,
//                     labels: labels,
//                     marker: {
//                         colors: colors
//                     },
//                     type: "pie"
//                 }];

//                 const layout = {
//                     title: {
//                         text: "Product sold percentage in different colors"
//                     },
//                     height: 350
//                 };
//                 Plotly.newPlot("pie", colorData, layout);
//             }
//         }
//     };
//     xhr.open("GET", "/api/1.0/PieChart");
//     xhr.send();
// }
// PieChart();

// function Histograms () {
//     const xhr = new XMLHttpRequest();
//     xhr.onreadystatechange = function () {
//         if (xhr.readyState === 4) {
//             if (xhr.status === 200) {
//                 const ans = JSON.parse(xhr.responseText);

//                 const trace = {
//                     x: ans.data,
//                     type: "histogram"
//                 };
//                 const layout = {
//                     title: {
//                         text: "Product sold quantity in different price range"
//                     },
//                     xaxis: {
//                         title: {
//                             text: "Price Range"
//                         }
//                     },
//                     yaxis: {
//                         title: {
//                             text: "Quantity"
//                         }
//                     }
//                 };
//                 const data = [trace];
//                 Plotly.newPlot("histogram", data, layout);
//             }
//         }
//     };
//     xhr.open("GET", "/api/1.0/Histograms");
//     xhr.send();
// }
// Histograms();

// function BarChart () {
//     const xhr = new XMLHttpRequest();
//     xhr.onreadystatechange = function () {
//         if (xhr.readyState === 4) {
//             if (xhr.status === 200) {
//                 const ans = JSON.parse(xhr.responseText);
//                 const sizeData = ans.data.map(d => ({
//                     x: d.idList.map(id => "product " + id),
//                     y: d.count,
//                     name: d.size,
//                     type: "bar"
//                 }));

//                 const layout = {
//                     barmode: "stack",
//                     title: {
//                         text: "Quantity of top 5 sold products in different sizes"
//                     },
//                     yaxis: {
//                         title: {
//                             text: "Quantity"
//                         }
//                     }
//                 };
//                 Plotly.newPlot("bar", sizeData, layout);
//             }
//         }
//     };
//     xhr.open("GET", "/api/1.0/BarChart");
//     xhr.send();
// }
// BarChart();
