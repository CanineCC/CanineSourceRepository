import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5228/bpnHub", { transport: signalR.HttpTransportType.WebSockets }) 
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .build();

// Start the connection
export async function startConnection() {
    try {
        await connection.start();
    } catch (err) {
        setTimeout(startConnection, 5000); // Retry after 5 seconds if the connection fails
    }
}

export async function stopConnection() {
    await connection.stop();
}
/*
connection.onclose(error => {
    console.error("SignalR connection closed:", error);
});

connection.onreconnecting(error => {
    console.warn("SignalR reconnecting:", error);
});

connection.onreconnected(connectionId => {
    console.log("SignalR reconnected:", connectionId);
});*/

// Subscribe to the "ReceiveModelUpdate" event from the server
export function onModelUpdate(callback: (message: string) => void) {
    connection.on("ReceiveMessage", callback);
};
export function onEntityUpdate(callback: (name: string, id: string, message: string) => void) {
    connection.on("onEntityUpdate", callback);
};
export function onGroupUpdate(callback: (name: string, message: string) => void) {
    connection.on("onGroupUpdate", callback);
};

export function offModelUpdate(callback: (message: string) => void) {
    connection.off("ReceiveMessage", callback);
};
export function offEntityUpdate(callback: (name: string, id: string, message: string)=> void) {
    connection.off("onEntityUpdate", callback);
};
export function offGroupUpdate(callback: (name: string, message: string)=> void) {
    connection.off("onGroupUpdate", callback);
};


export async function joinEntityView(name: string, id: string) {
    try {
        await connection.invoke("JoinEntityView", name, id);
        console.log(`Joined entity: ${id} for ressource : ${name}`);
    } catch (err) {
        console.error(`Error joining entity: ${id} for ressource : ${name}`, err);
    }
};
export async function leaveEntityView(name: string, id: string) {
    try {
        await connection.invoke("LeaveEntityView", name, id);
        console.log(`Left entity: ${id} for ressource : ${name}`);
    } catch (err) {
        console.error(`Error leaving entity: ${id} for ressource : ${name}`, err);
    }
};

export async function joinGroupView(name: string) {
    try {
        await connection.invoke("JoinGroupView", name);
        console.log(`Joined group for ressource : ${name}`);
    } catch (err) {
        console.error(`Error joining group for ressource : ${name}`, err);
    }
};
export async function leaveGroupView(name: string) {
    try {
        await connection.invoke("LeaveGroupView", name);
        console.log(`left group for ressource : ${name}`);
    } catch (err) {
        console.error(`Error leaving group for ressource : ${name}`, err);
    }
};

