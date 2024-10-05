<script lang="ts">
    import { onMount } from 'svelte';
    import { ServerApi } from '../BpnEngineClient/apis'; // Adjust the path if necessary
    import type { ServerHealth } from '../BpnEngineClient';

    let serverHealthObj: ServerHealth; // Store the server health data
    let fetchStatusText: string = "Attempt 1: Loading server status...";
    let lastSuccessfulLoadTime: Date | null = null; // Store the last successful load time
    let isLoading: boolean = false; // Track if fetchServerStatus is currently in progress
    let retryCount: number = 0; // Track the number of retries
    const pollInterval = 45000; // Polling interval in milliseconds (45 seconds)
    const serverApi = new ServerApi();

    // Reactive properties for color and tooltip
    let statusColor: string = '#f8d7da'; // Default to red
    let tooltipMessage: string = 'Loading...';

    // Function to fetch the server status with retries
    async function fetchServerStatus(attempt = 1, maxAttempts = 5) {
        isLoading = true; // Indicate that a request is in progress
        retryCount = attempt - 1; // Set retry count (attempt starts at 1)
        updateStatus(); // Update UI color and tooltip

        try {
            serverHealthObj = await serverApi.serverHealth();
            lastSuccessfulLoadTime = new Date(); // Update the last successful load time
            fetchStatusText = "Server status loaded successfully!";
            retryCount = 0; // Reset retry count on success
        } catch (error) {
            fetchStatusText = `Attempt ${attempt}: Error fetching server status`;

            if (attempt < maxAttempts) {
                // Calculate the delay (attempt * 0.5)^2 in milliseconds
                const delay = Math.pow(attempt * 0.5, 2) * 1000;
                await new Promise(resolve => setTimeout(resolve, delay));
                return fetchServerStatus(attempt + 1, maxAttempts); // Retry
            } else {
                fetchStatusText = 'Max attempts reached. Could not fetch server status.';
            }
        } finally {
            isLoading = false; // Indicate that the request has completed
            updateStatus(); // Update the UI after the request finishes
        }
    }

    // Function to continuously poll the server status
    async function startPolling() {
        while (true) {
            if (!isLoading) {
                // Start a new fetch only if no request is in progress
                await fetchServerStatus(1, 12);
            }
            await new Promise(resolve => setTimeout(resolve, serverHealthObj?.isHealthy ? pollInterval : 5)); // Wait for 15 seconds before the next fetch
        }
    }

    onMount(() => {
        startPolling(); // Start polling when the component mounts
    });

    // Function to update the color and tooltip reactively
    function updateStatus() {
        statusColor = getStatusColor();
        tooltipMessage = getTooltipMessage();
    }

    // Determine the color based on the number of retries and the fetch status
    function getStatusColor() {
        if (serverHealthObj?.isHealthy && retryCount < 3)
            return '#d4edda'; // Green if healthy
        if (retryCount < 5)
            return '#fff3cd'; // Yellow
        if (retryCount < 8)
            return '#ffeeba'; // Orange
        return '#f8d7da'; // Red by default (error state)
    }

    // Tooltip message with status information
    function getTooltipMessage() {
        if (isLoading) {
            return fetchStatusText; // Show loading/failure message
        }
        if (serverHealthObj) {
            return `Status: ${serverHealthObj.isHealthy ? 'Healthy' : 'Unhealthy'}\nMemory Usage: ${serverHealthObj.serverMemoryUsageInMegaBytes} MB\nServer Started: ${serverHealthObj.serverStartedTime?.toLocaleTimeString()}\nFetch status: ${fetchStatusText}`;
        }
        return fetchStatusText;
    }
</script>

<!-- HTML Template -->
<div class="status-container">
    <div
        class="status-circle"
        style="background-color: {statusColor};"
        title={tooltipMessage}></div>
</div>

<style>
    .status-container {
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .status-circle {
        width: 20px;
        height: 20px;
        border-radius: 50%;
        border: 1px solid #ccc;
        display: inline-block;
        cursor: pointer;
    }

    /* Optional: You can add some hover effect if desired */
    .status-circle:hover {
        border-color: #888;
    }
</style>
