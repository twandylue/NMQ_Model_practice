const nameDict = {
    1: "Task 1",
    2: "Task 2"
};
// {"id":33,"type":2,"name":"Task 2"}
class Task {
    constructor (id, type, name) {
        this.id = id;
        this.type = type;
        this.name = nameDict[type];
    }
}

module.exports = { Task };
