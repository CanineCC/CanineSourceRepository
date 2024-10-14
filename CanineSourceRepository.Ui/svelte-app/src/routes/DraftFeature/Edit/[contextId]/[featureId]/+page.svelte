<script lang="ts">
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import Layout from '../../../../../components/Layout.svelte';
    import TaskComponent from '../../../../../components/TaskComponent.svelte';
    import Accordion from '../../../../../lib/Accordion.svelte';
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
    let selectedTask : BpnTask |null = null;

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

    function handleTaskSelect(event: any) {
        const { taskId } = event.detail;
        const existingTaskIndex = tasks.findIndex(task => task.id === taskId);
        if (existingTaskIndex >= 0) {
            selectedTask = { ...tasks[existingTaskIndex] };

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
        await saveTaskPositions(); //save other changes too
        draftFeatureApi.releaseFeature({ request6: { featureId: featureId}})
    }

    let isLoggedIn = false; // Replace this with your actual login state logic   
</script>

<style>
    .feature-header {
        display: grid;
        grid-template-columns: auto auto;
        gap: 10px;    
    }
    
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
        color:white;
        text-align: right; /* Right align the key */
        width: 150px; /* Fixed width to allow space for the keys */
    }

    .value {
        padding-left: 25px;
        text-align: left; /* Left align the value */
        flex: 1; /* Allow the value to take the remaining space */
    }
    .graph-wrapper {
        padding: 0px; 
        width: calc(100vw - 170px); 
        height:600px; 
        overflow-x: auto; 
        overflow-y: auto; 
        box-sizing: border-box; 
        position: relative;
        scrollbar-width: thin; /* For Firefox */
        scrollbar-color: #888 #3c3c3c; 
        background-color: #232323;
    }
    .task-wrapper {
        padding-top: 25px;
    }
</style>

<Layout {isLoggedIn}>
<Accordion title="Feature Details" isOpen={true}>
{#if feature}
<div class="feature-header">
    <div class="key-value-pairs">
        <div class="pair">
            <span class="key">Name:</span>
            <span class="value">{feature.name} (Draft)</span>
        </div>
        <div class="pair">
            <span class="key">Objective:</span>
            <span class="value">{feature.objective}</span>
        </div>
        <div class="pair">
            <span class="key">Flow Overview:</span>
            <span class="value">{feature.flowOverview}</span>
        </div>
    </div>

    <div style="display: flex; gap:25px; flex-flow: row-reverse;">
        <a href="#top" title="Release" class="button" on:click={releaseFeature}><i class="fas fa-rocket "></i></a>
        <a href="#top" title="Save" class="button" on:click={saveTaskPositions}><i class="fas fa-save"></i></a>
    </div>
   <!-- (TODO: including review/approval? approval flow?)-->
</div>
{/if}
</Accordion>


{#if feature}
<Accordion title="BPN" isOpen={true}>
    <div class="graph-wrapper">
        <Graph 
        {tasks} 
        {diagram} 
        on:taskPositionChange={handleTaskPositionChange}
        on:taskSelect={handleTaskSelect}
        />
    </div>

    {#if selectedTask}
        <div class="task-wrapper">
        <TaskComponent readonly={false} task={selectedTask} />
        </div>
    {/if}
    </Accordion>

{:else}
    <p>... loading ...</p>
{/if}

</Layout>
