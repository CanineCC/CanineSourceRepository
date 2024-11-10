<script lang="ts">
    import Layout from '@/+layout.svelte';
    import { onMount, onDestroy } from 'svelte';
    import {  SolutionApi, SystemApi, ContainerApi,  SystemDiagramApi, ContainerDiagramApi, FeatureDiagramApi, FeatureApi } from 'BpnEngineClient/apis';
    import type {BpnSolution, BpnSystem, BpnWebApiContainer} from "BpnEngineClient";
    import {goto} from "$app/navigation";
    import Graph from "components/diagram/Graph.svelte";

    const solutionApi = new SolutionApi();
    const systemApi = new SystemApi();
    const systemDiagramApi = new SystemDiagramApi();
    const containerApi = new ContainerApi();
    const containerDiagramApi = new ContainerDiagramApi();
    const featureApi = new FeatureApi();
    const featureDiagramApi = new FeatureDiagramApi();
    let solutions : BpnSolution[]  | null = null;
    let systems : BpnSystem[] | null = null;
    let containers : BpnWebApiContainer[] | null = null;
    onMount(async () => {
        solutions = await solutionApi.getAllSolutions();
        systems = await systemApi.getAllSystems();
        containers = await containerApi.getAllContainers();
    });
    onDestroy(() => {
    });
</script>

<Layout>
    {#if solutions}
   {#each solutions as solution}
    <p style="color: red">TODO: SELECT SOLUTION from Dropdown (default to first-or-default)</p>
    <p style="color: red">TODO: Revision dropdown, and show diagrams based on that (default to newest)</p>

    <h1>1. System context diagram</h1>
    {#await Promise.resolve(systemDiagramApi.getC4Level1DiagramSvg({ solutionId:solution.id})) then c4level1}
        <div class="svgWrapper">
            {@html c4level1}
        </div>
    {/await}
    <p>{solution.description}</p>
    <hr>

    <h1>2. Container diagram</h1>
    {#if systems !== null}
        {#each systems as system}
            {#await Promise.resolve(containerDiagramApi.getC4Level2DiagramSvg({ systemId:system.id })) then c4level2}
                <div class="svgWrapper">
                    {@html c4level2}
                </div>
            {/await}
            <p>{system.description}</p>
        {/each}
    {/if}
    <hr>

    <h1>3. Component diagram</h1>
    {#if containers !== null}
        {#each containers as container}
            {#await Promise.resolve(featureDiagramApi.getC4Level3DiagramSvg({  containerId:container.id })) then c4level3}
                <div class="svgWrapper">
                    {@html c4level3}
                </div>
            {/await}
            <p>{container.description}</p>
            {#each container.features as feature}
                {#await Promise.resolve(featureApi.getFeatureRevision({featureId:feature.id, revision:1/*TODO*/ })) then c4level4}
                    <h2>4. Code diagram for '{c4level4.name}'
                        <button title="view" aria-label="view"
                                on:click={() => goto(`/feature/edit/${container.id}/${feature.id}/${c4level4.revision}`)}
                                style="border: none; background: none; cursor: pointer; user-select: none;">
                                <span class="actionRow">
                                    <i class="fas fa-eye"></i>
                                </span>
                        </button>
                    </h2>
                    <div class="svgWrapper">
                        <Graph
                                tasks={c4level4.tasks??[]}
                                diagram={c4level4.diagram}
                                featureId={feature.id}
                                readonly={true}
                                showDownload={false}
                        />

                    </div>
                {/await}
                <p>{feature.revisions[0].objective}</p>
            {/each}
        {/each}
    {/if}
    <hr>
    {/each}
    {/if}
</Layout>

<style>
    .svgWrapper {
        border-radius: 25px;       /* Apply rounded corners to the wrapper */
        overflow: hidden;          /* Hide overflow if the SVG exceeds the rounded corners */
        display: inline-block;     /* Ensure it's treated as a block element */
        max-width: 100%;           /* Optional: adjust depending on your needs */
        background-color: white;
        padding:25px;
    }

    svg {
        width: 100%;               /* Ensure SVG scales to fit the container */
        height: auto;              /* Maintain aspect ratio */
    }
    .actionRow {
        color: #e0e0e0;
        font-family: sans-serif;
        font-size: 12pt;
    }

</style>