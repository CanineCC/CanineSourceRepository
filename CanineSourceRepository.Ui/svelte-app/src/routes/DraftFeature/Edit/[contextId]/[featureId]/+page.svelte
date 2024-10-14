<script lang="ts">
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import Layout from '../../../../../components/Layout.svelte';
    import TaskComponent from '../../../../../components/TaskComponent.svelte';
    import Accordion from '../../../../../lib/Accordion.svelte';
    import { DraftFeatureApi, DraftFeatureDiagramApi  } from '../../../../../BpnEngineClient/apis'; // Adjust the path accordingly
	import { type BpnTask, type BpnTransition, type BpnFeatureDiagram, type BpnDraftFeature, type BpnPosition, type PositionsUpdatedOnDraftFeatureRequest, UpdateDraftFeaturePurposeBodyFromJSON } from '../../../../../BpnEngineClient';
    import Graph from '../../../../../components/diagram/Graph.svelte';
    

    const draftFeatureApi = new DraftFeatureApi();
    const draftFeatureDiagramApi = new DraftFeatureDiagramApi();

    let contextId: string;
    let featureId: string;

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
    async function handleTaskPositionChange(event: CustomEvent) {
        const { taskId, position } = event.detail;
        await draftFeatureDiagramApi.positionUpdatedOnDraftFeature({ positionUpdatedOnDraftFeatureBody: { featureId, taskId, position }});
    }
    function handleTaskSelect(event: any) {
        const { taskId } = event.detail;
        const existingTaskIndex = tasks.findIndex(task => task.id === taskId);
        if (existingTaskIndex >= 0) {
            selectedTask = { ...tasks[existingTaskIndex] };
        } 
    }
    async function reset() {
        await draftFeatureApi.resetDraftFeature({ resetDraftFeatureBody:{ featureId }});
    }
    async function releaseFeature() {
        await draftFeatureApi.releaseFeature({ releaseFeatureBody: { featureId: featureId}})
    }


    async function addTask() {
        await draftFeatureApi.addCodeTaskToDraftFeature({ addCodeTaskToDraftFeatureBody: { featureId: featureId, task: {  
            id: undefined,
            name: "<task>",
            businessPurpose: "",
            behavioralGoal: "",
            input: null,
            output: null,
            serviceDependency: "",
            namedConfiguration: "",
            recordTypes: [],
            validDatatypes: []
        }} });
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
    .bpn-action-bar {
        padding: 25px 0 0 0;
    }
</style>

<Layout {isLoggedIn}>
    {#if feature}
    <Accordion title="Feature Details" isOpen={true}>
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
                <a href="#top" title="Restore" class="button" on:click={reset}><i class="fas fa-undo"></i></a>
                <a href="#top" title="Add code task" class="button" on:click={addTask}><i class="fas fa-code"></i></a>
             <!--   <a href="#top" title="Add api task" class="button" on:click={addTask}><i class="fas fa-cloud"></i></a>-->
            </div>
        </div>
    </Accordion>
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
                <TaskComponent readonly={false} task={selectedTask} featureId={featureId} />
            </div>
        {/if}
    </Accordion>

{:else}
    <p>... loading ...</p>
{/if}

</Layout>
