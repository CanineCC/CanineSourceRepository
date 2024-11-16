<script lang="ts">
    import { page } from '$app/stores';
    import { onMount, onDestroy } from 'svelte';
    import Layout from '@/+layout.svelte';
    import TaskComponent from 'components/TaskComponent.svelte';
    import Accordion from 'components/Accordion.svelte';
    import { DraftFeatureApi  } from 'BpnEngineClient/apis';
	import { type BpnTask, type FeatureComponentDiagram, type BpnDraftFeature } from 'BpnEngineClient';
    import Graph from 'components/diagram/Graph.svelte';
    import {  onEntityUpdate, offEntityUpdate, joinEntityView,leaveEntityView } from 'signalRService'
    
    const draftFeatureApi = new DraftFeatureApi();
//    const draftFeatureDiagramApi = new DraftFeatureDiagramApi();

    let contextId: string = "";
    let featureId: string = "";
    let feature : BpnDraftFeature | null = null;
    let tasks : Array<BpnTask> =[];
//    let transitions : Array<BpnTransition> =[];
    let diagram : FeatureComponentDiagram | undefined = undefined;
    let selectedTask : BpnTask |null = null;

    const callback = (name: string, id: string, message: string) => {
        fetchDetails(contextId, featureId);
    };
    
    onMount(() => {
        contextId = $page.params.contextid;
        featureId = $page.params.featureid;

        onEntityUpdate(callback);
        joinEntityView("bpndraftfeature", featureId);
        fetchDetails(contextId, featureId);
    });
    
    onDestroy(() => {
        offEntityUpdate(callback);
        leaveEntityView("bpndraftfeature", featureId);
    });
    
    async function fetchDetails(contextId: string, featureId: string) {
        feature = await draftFeatureApi.getDraftFeature({featureId: featureId});
        tasks = feature.tasks;
      //  transitions = feature.transitions;
        diagram = feature.componentDiagram;
        if (selectedTask)
        {
            selectedTask = tasks.find((p) => p.id === selectedTask!.id) ?? null;
        }
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
        await draftFeatureApi.addTaskToDraftFeature({ addTaskToDraftFeatureBody: {
            featureId: featureId,
            task: {
                name: "<task>",
                code: "",
                businessPurpose: "",
                behavioralGoal: "",
                input: null,
                output: null,
                serviceDependency: "",
                namedConfiguration: "",
                recordTypes: [],
                validDatatypes: [],
                testCases: []
            }
        } });
    }
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

<Layout>
    {#if feature}
    <div class="feature-header">
        <h1>{feature.name}</h1>
        <div style="display: flex; padding:12px; gap:25px; flex-flow: row-reverse;">
            <a href="#top" title="Release" aria-label="Release" class="button" on:click={releaseFeature}><i class="fas fa-rocket "></i></a>
            <a href="#top" title="Restore" aria-label="Restore" class="button" on:click={reset}><i class="fas fa-undo"></i></a>
            <a href="#top" title="Add code task" aria-label="Add code task" class="button" on:click={addTask}><i class="fas fa-code"></i></a>
        </div>
    </div>

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

            </div>

            <div style="display: flex; gap:25px; flex-flow: row-reverse;">
                <a href="#top" title="Save feature purpose" aria-label="Release" class="button" on:click={saveFeaturePurpose}><i class="fas fa-save "></i></a>
            </div>
        </div>
    </Accordion>
    <Accordion title="BPN" isOpen={true}>
        <div class="graph-wrapper">
            <Graph 
            {tasks} 
            {diagram} 
            featureId={featureId}
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
