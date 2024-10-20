import * as signalR from "@microsoft/signalr";

// Initialize the SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5228/bpnHub", { transport: signalR.HttpTransportType.WebSockets }) // Adjust the URL to your SignalR hub endpoint
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Optional: automatically reconnect if the connection is dropped
    .configureLogging(signalR.LogLevel.Debug)
    .build();

// Start the connection
export async function startConnection() {
    try {
        console.log("Try starting SignalR connection.");
        await connection.start();
        console.log("SignalR connection started.");
    } catch (err) {
        console.error("Error starting SignalR connection:", err);
        setTimeout(startConnection, 5000); // Retry after 5 seconds if the connection fails
    }
}
connection.onclose(error => {
    console.error("SignalR connection closed:", error);
});

connection.onreconnecting(error => {
    console.warn("SignalR reconnecting:", error);
});

connection.onreconnected(connectionId => {
    console.log("SignalR reconnected:", connectionId);
});

// Subscribe to the "ReceiveModelUpdate" event from the server
export function onModelUpdate(callback: (message: string) => void) {
    connection.on("ReceiveMessage", callback);
};
export function onContextUpdate(callback: (message: string) => void) {
    connection.on("ReceiveBpnContextUpdate", callback);
};
export function onFeatureUpdate(callback: (featureId: string, message: string) => void) {
    connection.on("ReceiveBpnFeatureUpdate", callback);
};

export function offModelUpdate(callback: (message: string) => void) {
    connection.off("ReceiveMessage", callback);
};
export function offContextUpdate(callback: (message: string) => void) {
    connection.off("ReceiveBpnContextUpdate", callback);
};
export function offFeatureUpdate(callback: (featureId: string, message: string) => void) {
    connection.off("ReceiveBpnFeatureUpdate", callback);
};


export async function joinBpnContext() {
    try {
        await connection.invoke("JoinBpnContext");
        console.log(`Joined BpnContext`);
    } catch (err) {
        console.error("Error joining BpnContext:", err);
    }
};
export async function leaveBpnContext() {
    try {
        await connection.invoke("LeaveBpnContext");
        console.log("Left BpnContext");
    } catch (err) {
        console.error("Error leaving BpnFeature group:", err);
    }
};

export async function joinBpnFeatureGroup(bpnFeatureId: string) {
    try {
        await connection.invoke("JoinBpnFeatureGroup", bpnFeatureId);
        console.log(`Joined BpnFeature group for feature ID: ${bpnFeatureId}`);
    } catch (err) {
        console.error("Error joining BpnFeature group:", err);
    }
};
export async function leaveBpnFeatureGroup(bpnFeatureId: string) {
    try {
        await connection.invoke("LeaveBpnFeatureGroup", bpnFeatureId);
        console.log(`Left BpnFeature group for feature ID: ${bpnFeatureId}`);
    } catch (err) {
        console.error("Error leaving BpnFeature group:", err);
    }
};

