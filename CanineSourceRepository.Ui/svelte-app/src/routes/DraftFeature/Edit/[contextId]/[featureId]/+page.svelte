<script lang="ts">
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import Layout from '../../../../../components/Layout.svelte';
    import TaskComponent from '../../../../../components/TaskComponent.svelte';
    import Accordion from '../../../../../lib/Accordion.svelte';
    import { DraftFeatureApi, DraftFeatureDiagramApi  } from '../../../../../BpnEngineClient/apis'; // Adjust the path accordingly
	import { type BpnTask, type BpnTransition, type BpnFeatureDiagram, type BpnDraftFeature, type BpnPosition, type PositionsUpdatedOnDraftFeatureRequest, UpdateDraftFeaturePurposeBodyFromJSON } from '../../../../../BpnEngineClient';
    import Graph from '../../../../../components/diagram/Graph.svelte';
    import { onModelUpdate, onFeatureUpdate, joinBpnContext,joinBpnFeatureGroup } from '../../../../../lib/signalRService'
    
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
        onModelUpdate((message: string) => {
            console.log("Received update from SignalR:", message);


        });
        onFeatureUpdate((featureId: string, message: string) => {
            console.log(`Feature Update for ${featureId}: ${message}`);
        });
        
        joinBpnContext();
        joinBpnFeatureGroup(featureId);

        fetchDetails(contextId, featureId);
    });
    async function fetchDetails(contextId: string, featureId: string) {
        feature = await draftFeatureApi.getDraftFeature({featureId: featureId});
        tasks = feature.tasks??[];
        transitions = feature.transitions??[];
        diagram = feature.diagram;
    }
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
    async function saveFeaturePurpose() {
        if (!feature) return;
        await draftFeatureApi.updateDraftFeaturePurpose({ updateDraftFeaturePurposeBody : {
            featureId: featureId,
            name: feature.name,
            objective: feature.objective,
            flowOverview: feature.flowOverview
         }});
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
      flex: 1; /* Take the remaining space */
      padding: 20px; /* Padding inside content area */
      overflow-y: auto; /* Vertical scroll for content if needed */
      gap:25px;
      display: grid;
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
    {#if feature}
    <Accordion title="Feature Details" isOpen={true}>
        <div class="feature-header">
            <div class="key-value-pairs">
                <div class="pair">
                    <label for="feature-name">Name:</label>
                    <input id="feature-name" type="text" bind:value={feature.name} placeholder="Feature Name">
                </div>
                <div class="pair">
                    <label for="business-purpose">Objective:</label>
                    <textarea id="business-purpose" rows="5" bind:value={feature.objective} placeholder="Objective"></textarea>
                </div>
                <div class="pair">
                    <label for="flow-overview">Flow Overview:</label>
                    <textarea id="flow-overview" rows="5" bind:value={feature.flowOverview} placeholder="Flow overview description"></textarea>
                </div>

                <a href="#top" title="Save feature purpose" class="button" on:click={saveFeaturePurpose}><i class="fas fa-save "></i></a>
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
