/// <reference path="./crema.d.ts" />

let dataBaseName: string = "master";
let token: string = login("localhost", "admin1", "admin");

if (isDataBaseLoaded(dataBaseName) === false) {
    loadDataBase(dataBaseName);
}

if (isDataBaseEntered(dataBaseName) === false) {
    enterDataBase(dataBaseName);
}

beginTableCreate(dataBaseName, "/");

let transactionID: string = beginDataBaseTransaction(dataBaseName);
try {
    beginTableCreate(dataBaseName, "/");
    cancelDataBaseTransaction(transactionID);
} finally {
    leaveDataBase("master");
    logout(token);
}