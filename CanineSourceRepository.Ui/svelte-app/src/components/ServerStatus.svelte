<script lang="ts">
    import { onMount } from 'svelte';
    import { ServerApi } from '../BpnEngineClient/apis'; // Adjust the path if necessary
	import type { ServerHealth } from '../BpnEngineClient';

    let serverHealthObj: ServerHealth; // You can define a more specific type if you know the structure
    const serverApi = new ServerApi();

    async function fetchServerStatus() {
        try {
        serverHealthObj = await serverApi.serverHealth({}, {
            cache: 'default' // Follows server cache control
        }); // Call your API method
        } catch (error) {
        console.error('Error fetching server status:', error);
        }
    }
 
    onMount(() => {
        fetchServerStatus(); // Call the function when the component mounts
    });
</script>


<div>
    <h1>Server Status</h1>
    {#if serverHealthObj}
        {#if serverHealthObj.isHealthy}
            <p>{serverHealthObj.message}</p>
            <p>{serverHealthObj.serverMemoryUsageInMegaBytes} MB</p>
            <p>{serverHealthObj.serverStartedTime}</p>
        {:else}
            <p>{serverHealthObj.message}</p>
            <p>{serverHealthObj.serverMemoryUsageInMegaBytes} MB</p>
            <p>{serverHealthObj.serverStartedTime}</p>
        {/if}
    {:else}
    <p>Loading server status...</p>
    {/if}
</div>

<style>
    /* Add any component-specific styles here */
    div {
        border: 1px solid #ccc;
        padding: 1em;
        margin: 1em 0;
        border-radius: 5px;
    }
</style>