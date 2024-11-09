<script lang="ts">
    import Layout from '@/+layout.svelte';
    import { onMount, onDestroy } from 'svelte';
    import {  SystemApi, ContainerApi,  SystemDiagramApi, ContainerDiagramApi, FeatureDiagramApi } from 'BpnEngineClient/apis';
    import type {BpnSystem, BpnWebApiContainer} from "BpnEngineClient";
    import {goto} from "$app/navigation";

    const systemApi = new SystemApi();
    const systemDiagramApi = new SystemDiagramApi();
    const containerApi = new ContainerApi();
    const containerDiagramApi = new ContainerDiagramApi();
    const featureDiagramApi = new FeatureDiagramApi();
    let systems : BpnSystem[] | null = null;
    let containers : BpnWebApiContainer[] | null = null;
    onMount(async () => {
        systems = await systemApi.getAllSystems();
        containers = await containerApi.getAllContainers();
    });
    onDestroy(() => {
    });
    function editVersion(containerId: string, featureId: string, versionId: string) {
        if (versionId === "-1")
            goto(`/draftfeature/edit/${containerId}/${featureId}`);
        else
            goto(`/feature/edit/${containerId}/${featureId}/${versionId}`);
    }

</script>

<Layout>
    <h1>Documentation</h1>
    <p>C4 Model</p>
    <hr>
    <h2>1. System context diagram</h2>
    <p style="color: red">TODO: Revision dropdown for solution/program, and show diagrams based on that (default to newest)</p>
    {#await Promise.resolve(systemDiagramApi.getC4Level1DiagramSvg()) then c4level1}
        <div class="svgWrapper">
            {@html c4level1}
        </div>
    {/await}
    <hr>

    <h2>2. Container diagram</h2>
    <p style="color: red">TODO: Revision dropdown pr. system, and show diagrams based on that (default to newest)</p>
    {#if systems !== null}
        {#each systems as system}
            {#await Promise.resolve(containerDiagramApi.getC4Level2DiagramSvg({ systemId:system.id })) then c4level2}
                <div class="svgWrapper">
                    {@html c4level2}
                </div>
            {/await}
        {/each}
    {/if}
    <hr>

    <h2>3. Component diagram</h2>
    <p style="color: red">TODO: Revision dropdown pr. container, and show diagrams based on that (default to newest)</p>
    {#if containers !== null}
        {#each containers as container}
            {#await Promise.resolve(featureDiagramApi.getC4Level3DiagramSvg({  containerId:container.id })) then c4level3}
                <div class="svgWrapper">
                    {@html c4level3}
                </div>
            {/await}
            {#each container.features as feature}
                <h2>4. Code diagram for '{feature.revisions[0].name}'</h2>
                <p style="color: red">Todo: use the diagram component instead of linking via a table</p>

                <table style="width:100%">
                    <thead>
                        <th style="text-align: left">Feature</th>
                        <th style="text-align: left">Revision</th>
                    </thead>
                    <tbody>

                {#each feature.revisions as revision}
                    <tr>
                        <td>
                            <button title={revision.revision == -1 ? "edit": "view"} aria-label={revision.revision == -1 ? "edit": "view"}
                                    on:click={() =>
                                            editVersion(
                                                container.id ?? '',
                                                feature.id ?? '',
                                                revision.revision?.toString() ?? ''
                                            )}
                                    style="border: none; background: none; cursor: pointer; user-select: none;"
                            >
                                <span class="actionRow">
                                    <i class={revision.revision == -1 ? "fas fa-edit": "fas fa-eye"}></i>
                                    {revision.name} <span class="technology">[C#]</span>
                                </span>
                            </button>
                        </td>
                        <td>
                            {revision.revision === -1 ? "draft" : revision.revision}
                        </td>
                    </tr>
                {/each}
                    </tbody>
                </table>
            {/each}
        {/each}
    {/if}
    <hr>

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
    .technology {
        color: #555;
    }
    .actionRow {
        color: #e0e0e0;
        font-family: sans-serif;
        font-size: 12pt;
    }

</style>