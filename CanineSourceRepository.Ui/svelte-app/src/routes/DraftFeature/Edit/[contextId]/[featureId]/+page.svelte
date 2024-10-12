<script lang="ts">
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import Layout from '../../../../../components/Layout.svelte';
    import { DraftFeatureApi, DraftFeatureDiagramApi  } from '../../../../../BpnEngineClient/apis'; // Adjust the path accordingly
	import type { BpnTask, BpnTransition, BpnFeatureDiagram, BpnDraftFeature, BpnPosition, PositionsUpdatedOnDraftFeatureRequest } from '../../../../../BpnEngineClient';
    import Graph from '../../../../../components/diagram/Graph.svelte';
    

    const draftFeatureApi = new DraftFeatureApi();
    const draftFeatureDiagramApi = new DraftFeatureDiagramApi();

    let contextId: string;
    let featureId: string;

    let updatedTasks: Array<{ featureId: string, taskId: string, position: { x: number, y: number } }> = [];

    let feature : BpnDraftFeature | null = null;
    let tasks : Array<BpnTask> = [];
    let transitions : Array<BpnTransition> = [];
    let diagram : BpnFeatureDiagram | undefined;
  
    $: {
        contextId = $page.params.contextId;
        featureId = $page.params.featureId;
    }

    onMount(() => {
        fetchDetails(contextId, featureId);
    });

    async function fetchDetails(contextId: string, featureId: string) {
        feature = await draftFeatureApi.getDraftFeature({featureId: featureId});
        tasks = feature.tasks??[];
        transitions = feature.transitions??[];
        diagram = feature.diagram;
    }

    // Event handler for task position change
    function handleTaskPositionChange(event: CustomEvent) {
        const { taskId, position } = event.detail;
        const existingTaskIndex = updatedTasks.findIndex(task => task.taskId === taskId);
        if (existingTaskIndex >= 0) {
            updatedTasks[existingTaskIndex].position = position;
        } else {
            updatedTasks.push({ featureId, taskId, position });
        }
    }

    async function saveTaskPositions() {
        if (updatedTasks.length === 0) {
            console.log("No changes to save.");
            return;
        }
        const request : PositionsUpdatedOnDraftFeatureRequest = { request5 : updatedTasks};
        await draftFeatureDiagramApi.positionsUpdatedOnDraftFeature( request );
        updatedTasks = []; // Clear the list once the server responds with HTTP 202
    }
    async function releaseFeature() {
        draftFeatureApi.releaseFeature({ request6: { featureId: featureId}})

    }

    let isLoggedIn = false; // Replace this with your actual login state logic   
</script>

<style>
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
            <span class="key">Revision:</span>
            <span class="value">DRAFT</span>
        </div>
    </div>


    TABS: (show on selected node)
    <ul>
        <li>
            -- Overview (name, businesspurpose, behavioralGoal, service selection/named configuration)
        </li>
        <li>
            -- Data structures (including input and output definition) ==> TODO: Output from backend
        </li>
        <li>
            -- Verification (given-input, expect-output)
        </li>
        <li>
            -- Code editor
        </li>
    </ul>

    <div style="width:100%; height:1000px">
        <Graph 
        {tasks} 
        {transitions} 
        {diagram} 
        on:taskPositionChange={handleTaskPositionChange}
        />
    </div>
        <button on:click={saveTaskPositions}>Save</button>

        <button on:click={releaseFeature}>RELEASE </button>
        (TODO: including review/approval? approval flow?)
{:else}
    <p>... loading ...</p>
{/if}

</Layout>
