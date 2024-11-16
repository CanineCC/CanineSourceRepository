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

        addEventListeners();
    });
    onDestroy(() => {
    });
    function addEventListeners() {
        const svgElement = document.querySelector('svg');
        const userSystemElement = svgElement?.getElementById('elem_UserSystem');//todo: get id+next resource url from server...
        //also: create breadcrumb on the way in, to give the user a way back....

        if (userSystemElement) {
            console.log("Adding event");
            userSystemElement.addEventListener('dblclick', handleDoubleClick);
            userSystemElement.addEventListener('mouseover', handleMouseOver);
            userSystemElement.addEventListener('mouseout', handleMouseOut);
        } else {
            console.log("Did NOT add the event");
        }
    }
    function handleMouseOver(event) {
        event.target.style.cursor = 'pointer';
    }

    function handleMouseOut(event) {
        event.target.style.cursor = '';
    }
    function handleDoubleClick() {
        console.log("User clicked on the UserSystem element!");
    }
    //{solution.systems[0].name /*TODO: Get "id"/"alias" of the system in svg...*/}
    // Add a listener for a double-click event on the 'elem_UserSystem' element
//    document.getElementById('elem_UserSystem').addEventListener('dblclick', function() {
        // Replace the SVG content with a detailed SVG when the element is double-clicked
  //      console.log("User clicked on the UserSystem element!");

        // This is just a sample replacement. You can customize it to load a more detailed SVG or content.
/*        const detailedSvg = `
                <svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" version="1.1" viewBox="0 0 500 500">
                    <circle cx="250" cy="250" r="200" fill="#FF5733"/>
                    <text x="250" y="250" font-size="20" text-anchor="middle" fill="white">Detailed UserSystem</text>
                </svg>
            `;

        document.body.innerHTML = detailedSvg;*/
    //});

</script>

<Layout>
   <p style="color: red">TODO: CONSIDER UX - one solution, many systems - one system - many container, one container - many components, one component - many codes ... this will be a mess once we start to have many-to-many-to-many-to-many</p>
   <p style="color: red">The diagrams can not be made interactive, but we can make text/buttons/links etc. around them - also it is possible to make a graph/tree representation if needed</p>
   <br/>
   {#if solutions}
   {#each solutions as solution}
    <p style="color: red">TODO: SELECT SOLUTION from Dropdown (default to first-or-default)</p>
    <h1>1. System context diagram</h1>
    <p>{solution.description}</p>
    {#await Promise.resolve(systemDiagramApi.getC4Level1DiagramSvg({ solutionId:solution.id})) then c4level1}
        <div class="svgWrapper">
            {@html c4level1}
        </div>


    {/await}
    <hr>

    <h1>2. Container diagram</h1>
    {#if systems !== null}
        {#each systems as system}
            <p>{system.description}</p>
            {#await Promise.resolve(containerDiagramApi.getC4Level2DiagramSvg({ systemId:system.id })) then c4level2}
                <div class="svgWrapper">
                    {@html c4level2}
                </div>
            {/await}
        {/each}
    {/if}
    <hr>

    <h1>3. Component diagram</h1>
    {#if containers !== null}
        {#each containers as container}
            <p>{container.description}</p>
            {#await Promise.resolve(featureDiagramApi.getC4Level3DiagramSvg({  containerId:container.id })) then c4level3}
                <div class="svgWrapper">
                    {@html c4level3}
                </div>
            {/await}
            {#each container.features as feature}
                {#await Promise.resolve(featureApi.getFeatureRevision({featureId:feature.id, revision: feature.revisions[feature.revisions.length -1].revision })) then c4level4}
                    <h2>4. Code diagram for '{c4level4.name} (v. {c4level4.revision})'
                        <button title="view" aria-label="view"
                                on:click={() => goto(`/feature/edit/${container.id}/${feature.id}/${c4level4.revision}`)}
                                style="border: none; background: none; cursor: pointer; user-select: none;">
                                <span class="actionRow">
                                    <i class="fas fa-eye"></i>
                                </span>
                        </button>
                    </h2>
                    <p>{feature.revisions[feature.revisions.length -1].objective}</p>
                    <div class="svgWrapper">
                        <Graph
                                tasks={c4level4.tasks??[]}
                                diagram={c4level4.componentDiagram}
                                featureId={feature.id}
                                readonly={true}
                                showDownload={false}
                        />

                    </div>
                {/await}
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

