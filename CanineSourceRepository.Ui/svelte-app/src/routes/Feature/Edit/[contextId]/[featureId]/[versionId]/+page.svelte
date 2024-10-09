<script lang="ts">
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import Layout from '../../../../../../components/Layout.svelte';
    import { FeatureApi  } from '../../../../../../BpnEngineClient/apis'; // Adjust the path accordingly
	import type { BpnTask, BpnTransition, BpnFeatureDiagram, BpnFeatureVersion, BpnFeatureVersionStat } from '../../../../../../BpnEngineClient';
    import Graph from '../../../../../../components/diagram/Graph.svelte';
    

    const featureApi = new FeatureApi();

    // Store the route parameters
    let contextId: string;
    let featureId: string;
    let versionId: string;

    let updatedTasks: Array<{ taskId: string, position: { x: number, y: number } }> = [];

    let feature : BpnFeatureVersion | null = null;
    let tasks : Array<BpnTask> = [];
    let transitions : Array<BpnTransition> = [];
    let diagram : BpnFeatureDiagram | undefined;
    let stats : BpnFeatureVersionStat | undefined;
  
    
    $: {
        contextId = $page.params.contextId;
        featureId = $page.params.featureId;
        versionId = $page.params.versionId;
    }

    onMount(() => {
        fetchVersionDetails(contextId, featureId, versionId);
    });

    async function fetchVersionDetails(contextId: string, featureId: string, versionId: string) {
        let version = parseInt(versionId);
        feature = await featureApi.getFeatureVersion({featureId: featureId, version: version});
        tasks = feature.tasks??[];
        transitions = feature.transitions??[];
        diagram = feature.diagram;

        stats = await featureApi.getFeatureVersionStats({featureId: featureId, version: version })

//featureStats?: Stats;
//taskStats?: { [key: string]: Stats; };        
    }

    // Event handler for task position change
    function handleTaskPositionChange(event: CustomEvent) {
        const { taskId, position } = event.detail;
        const existingTaskIndex = updatedTasks.findIndex(task => task.taskId === taskId);
        if (existingTaskIndex >= 0) {
            updatedTasks[existingTaskIndex].position = position;
        } else {
            updatedTasks.push({ taskId, position });
        }
    }

    async function saveTaskPositions() {
        if (updatedTasks.length === 0) {
            console.log("No changes to save.");
            return;
        }

        try {
            updatedTasks = []; // Clear the list once the server responds with HTTP 202
        } catch (error) {
            console.error(`Error saving task positions:`, error);
        }
    }

    let isLoggedIn = false; // Replace this with your actual login state logic   
</script>

<style>
    .feature-header {
        display: grid;
    grid-template-columns: auto auto auto;
    gap: 10px;    }
    .key-value-pairs {
        display: flex;
        flex-direction: column;
        gap: 10px;
    }

    .pair {
        display: flex;
        justify-content: space-between;
        /*max-width: 500px; Adjust based on your layout */
    }

    .key {
        font-weight: bold;
        text-align: right; /* Right align the key */
        width: 150px; /* Fixed width to allow space for the keys */
    }

    .value {
        padding-left: 25px;
        color: #333;
        text-align: left; /* Left align the value */
        flex: 1; /* Allow the value to take the remaining space */
    }
</style>


<Layout {isLoggedIn}>
    TODO::: update serverside /feature/stats/featureid/version to return 2d array for presentation as graph
    SEE:: https://apexcharts.com/javascript-chart-demos/dashboards/
    SEE:: https://www.chartjs.org/docs/latest/samples/subtitle/basic.html
    
<div class="feature-header">
    {#if feature}
    <div class="key-value-pairs">
        <div class="pair">
            <span class="key">Name:</span>
            <span class="value">{feature.name}</span>
        </div>
        <div class="pair">
            <span class="key">Objective:</span>
            <span class="value">{feature.objective}</span>
        </div>
        <div class="pair">
            <span class="key">Flow Overview:</span>
            <span class="value">{feature.flowOverview}</span>
        </div>
        <div class="pair">
            <span class="key">Released By:</span>
            <span class="value">{feature.releasedBy}</span>
        </div>
        <div class="pair">
            <span class="key">Released Date:</span>
            <span class="value">{feature.releasedDate}</span>
        </div>
        <div class="pair">
            <span class="key">Revision:</span>
            <span class="value">{feature.revision}</span>
        </div>
    </div>
    {/if}


    {#if stats?.versionStats}
    <div class="key-value-pairs">
        <div class="pair">
            <span class="key">Max duration (ms):</span>
            <span class="value">{stats.versionStats.maxDurationMs}</span>
        </div>
        <div class="pair">
            <span class="key">Avg duration (ms):</span>
            <span class="value">{stats.versionStats.avgDurationMs}</span>
        </div>
        <div class="pair">
            <span class="key">Min duration (ms):</span>
            <span class="value">{stats.versionStats.minDurationMs}</span>
        </div>
        <div class="pair">
            <span class="key">Total duration (ms):</span>
            <span class="value">{stats.versionStats.totalDurationMs}</span>
        </div>
    </div>
    <div class="key-value-pairs">
        <div class="pair">
            <span class="key">Started:</span>
            <span class="value">{stats.versionStats.invocationCount}</span>
        </div>
        <div class="pair">
            <span class="key">Completed:</span>
            <span class="value">{stats.versionStats.invocationCompletedCount}</span>
        </div>
        <div class="pair">
            <span class="key">Failed:</span>
            <span class="value">{stats.versionStats.invocationErrorCount}</span>
        </div>
        <div class="pair">
            <span class="key">In progresss:</span>
            <span class="value">{stats.versionStats.invocationsInProgressCount}</span>
        </div>
        <div class="pair">
            <span class="key">Last used:</span>
            <span class="value">{stats.versionStats.lastUsed}</span>
        </div>
    </div>
{/if}
</div>
{#if feature}
TABS: (show on selected node)
-- Overview (name, businesspurpose, behavioralGoal, service selection/named configuration)
-- Data structures (including input and output definition) ==> TODO: Output from backend
-- Verification (given-input, expect-output)
-- Code viewer

<Graph 
    {tasks} 
    {transitions} 
    {diagram} 
    width={1440} 
    height={1000} 
    readonly={true}
    on:taskPositionChange={handleTaskPositionChange}
    />

{:else}
    <p>... loading ...</p>
{/if}

</Layout>
