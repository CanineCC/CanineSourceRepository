<script lang="ts">
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import Layout from '../../../../../../components/Layout.svelte';
    import { FeatureApi  } from '../../../../../../BpnEngineClient/apis'; // Adjust the path accordingly
	import type { BpnFeatureVersion } from '../../../../../../BpnEngineClient';
    import Graph from '../../../../../../components/diagram/Graph.svelte';
    

    const featureApi = new FeatureApi();

    // Store the route parameters
    let contextId: string;
    let featureId: string;
    let versionId: string;


    let feature : BpnFeatureVersion | null = null;
    let tasks : Array<BpnTask> = [];
    let transitions : Array<BpnTransition> = [];
    let diagram : BpnFeatureDiagram | undefined;
  
    // Access the page store to get the dynamic parameters
    $: {
        contextId = $page.params.contextId;
        featureId = $page.params.featureId;
        versionId = $page.params.versionId;
    }

    onMount(() => {
        console.log('Loaded Edit Page');
        console.log(`Context ID: ${contextId}`);
        console.log(`Feature ID: ${featureId}`);
        console.log(`Version ID: ${versionId}`);

        // Fetch data based on these IDs if needed
        fetchVersionDetails(contextId, featureId, versionId);
    });

    async function fetchVersionDetails(contextId: string, featureId: string, versionId: string) {
        let version = parseInt(versionId);
        feature = await featureApi.getFeatureVersion({featureId: featureId, version: version});
        tasks = feature.tasks;
        transitions = feature.transitions;
        diagram = feature.diagram;
        //feature.
        //console.log(`Fetching version details for context ${contextId}, feature ${featureId}, version ${versionId}`);

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
    TODO::: update serverside /feature/stats/featureid/version to return 2d array for presentation as graph
SEE:: https://apexcharts.com/javascript-chart-demos/dashboards/
SEE:: https://www.chartjs.org/docs/latest/samples/subtitle/basic.html

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

    (component with readonly state for Feature, and non-readonly for draft)
    DIAGRAM:
     -- SVG 
    TABS: (show on selected node)
     -- Overview (name, businesspurpose, behavioralGoal, service selection/named configuration)
     -- Data structures (including input and output definition) ==> TODO: Output from backend
     -- Verification (given-input, expect-output)
     -- Code editor

     <Graph {tasks} {transitions} {diagram} />
{:else}
    <p>... loading ...</p>
{/if}

</Layout>
