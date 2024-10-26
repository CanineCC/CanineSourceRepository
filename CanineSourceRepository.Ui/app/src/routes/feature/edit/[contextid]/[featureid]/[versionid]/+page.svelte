<script lang="ts">
    import { page } from '$app/stores';
    import { onMount, onDestroy } from 'svelte';
    import Layout from '@/+layout.svelte';
    import { FeatureApi, ServerApi  } from 'BpnEngineClient/apis'; 
	import type { BpnTask, BpnTransition, TaskStats, BpnFeatureDiagram, BpnFeatureVersion, BpnFeatureVersionStat, DurationClassification } from 'BpnEngineClient';
    import Graph from 'components/diagram/Graph.svelte';
    import TaskComponent from 'components/TaskComponent.svelte';
    import Accordion from 'components/Accordion.svelte';
    import FeatureDuration from 'components/FeatureDuration.svelte';
    import { formatDate  } from 'lib/Duration'; // Adjust the path accordingly
    import { writable } from 'svelte/store';
    const featureApi = new FeatureApi();
    const serverApi = new ServerApi();

	const currentTime = writable(new Date());

    // Store the route parameters
    let contextId: string;
    let featureId: string;
    let versionId: string;
	let durationClasses: DurationClassification[] = [];

    let feature : BpnFeatureVersion | null = null;
    let tasks : Array<BpnTask> = [];
    let transitions : Array<BpnTransition> = [];
    let diagram : BpnFeatureDiagram | undefined;
    let stats : BpnFeatureVersionStat | undefined;
    let taskStats: Array<TaskStats> | undefined = undefined;
    let selectedTask : BpnTask |null = null;
  
    
    $: {
        contextId = $page.params.contextid;
        featureId = $page.params.featureid;
        versionId = $page.params.versionid;
    }

    let intervalId: any;
	onMount(async () => {
        durationClasses = await serverApi.getDurationClassification();
        fetchVersionDetails(contextId, featureId, versionId);
		intervalId = setInterval(() => {
			currentTime.set(new Date());
		}, 1000);
	});
	onDestroy(() => {
		clearInterval(intervalId); // Clear interval on component destroy
	});


    async function fetchVersionDetails(contextId: string, featureId: string, versionId: string) {
        let version = parseInt(versionId);
        feature = await featureApi.getFeatureVersion({featureId: featureId, version: version});
        tasks = feature.tasks??[];
        transitions = feature.transitions??[];
        diagram = feature.diagram;

        stats = await featureApi.getFeatureVersionStats({featureId: featureId, version: version });
        console.log(stats);
        taskStats = stats.taskStats ?? [];
        console.log(taskStats);
    }

    function handleTaskSelect(event: any) {
        const { taskId } = event.detail;
        const existingTaskIndex = tasks.findIndex(task => task.id === taskId);
        if (existingTaskIndex >= 0) {
            selectedTask = { ...tasks[existingTaskIndex] };

        } 
    }
</script>

<style>
    .feature-header {
        display: grid;
        grid-template-columns: auto auto auto;
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
        /*color: #333;*/
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


<Layout>
{#if feature}
    <h1>{feature.name} (v.{feature.revision})</h1>
{/if}

<Accordion title="Feature Details" isOpen={true}>
    <div class="feature-header">
        {#if feature}
        <div class="key-value-pairs">
            <div class="pair">
                <span class="key">Objective:</span>
                <span class="value">{feature.objective}</span>
            </div>
            <div class="pair">
                <span class="key">Flow Overview:</span>
                <span class="value">{feature.flowOverview}</span>
            </div>
            <div class="pair">
                <span class="key">Released:</span>
                <span class="value">{$currentTime ? formatDate(feature.releasedDate, $currentTime): '-'} by '{feature.releasedBy}'</span>
            </div>
        </div>
        {/if}
        {#if stats}
        <div class="key-value-pairs">
            <div class="pair">
                <span class="key">Last used:</span>
                <span class="value">{$currentTime ? formatDate(stats.versionStats.lastUsed, $currentTime): '-'}</span>
            </div>
            <div class="pair">
                <span class="key">Max duration:</span>
                <span class="value">
                    <FeatureDuration duration={stats.versionStats.maxDurationMs}  durationClasses={durationClasses}></FeatureDuration>
                </span>
            </div>
            <div class="pair">
                <span class="key">Avg duration:</span>
                <span class="value">
                    <FeatureDuration duration={stats.versionStats.avgDurationMs}  durationClasses={durationClasses}></FeatureDuration>
                </span>
            </div>
            <div class="pair">
                <span class="key">Min duration:</span>
                <span class="value">
                    <FeatureDuration duration={stats.versionStats.minDurationMs}   durationClasses={durationClasses}></FeatureDuration>
                </span>
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
        </div>
        {/if}
    </div>
</Accordion>
<Accordion title="Telemetry" isOpen={false}>
    TODO::: update serverside /feature/stats/featureid/version to return 2d array for presentation as graph
    SEE:: https://apexcharts.com/javascript-chart-demos/dashboards/
    SEE:: https://www.chartjs.org/docs/latest/samples/subtitle/basic.html
</Accordion>

{#if feature && taskStats}
<Accordion title="BPN" isOpen={false}>
    <div class="graph-wrapper">
            <Graph 
            {tasks} 
            {diagram} 
            featureId={featureId}
            readonly={true}
            taskStats={taskStats}
            on:taskSelect={handleTaskSelect}
            />
    </div>

    {#if selectedTask}
        <div class="task-wrapper">
        <TaskComponent readonly={true} task={selectedTask} featureId={featureId} />
        </div>
    {/if}
</Accordion>

{:else}
    <p>... loading ...</p>
{/if}



</Layout>
