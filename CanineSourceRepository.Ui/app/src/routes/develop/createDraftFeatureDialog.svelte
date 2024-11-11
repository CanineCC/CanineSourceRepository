<script lang="ts">
    import {  DraftFeatureApi } from 'BpnEngineClient/apis';
    const draftFeatureApi = new DraftFeatureApi();
    export let bpnContextId : string;
    let name = '';
    let objective = '';
    let flowOverview = '';
    let isOpen = false; // To control if the dialog is open

    function openDialog() {
        isOpen = true;
    }

    function closeDialog() {
        isOpen = false;
    }

    async function submit() {
        // Handle form submission, e.g., send data to an API
        await draftFeatureApi.addDraftFeature({ addDraftFeatureBody: { name:name, objective:objective, flowOverview:flowOverview, bpnContextId:bpnContextId} });
        closeDialog(); // Close the modal after submission
    }


</script>

<div class="showCreateDialog" on:click={openDialog}><i class="fas fa-plus"></i></div>

{#if isOpen}
<div class="backdrop" on:click={closeDialog}></div>
{/if}
<dialog open={isOpen}>
    <form method="dialog">
        <h2>Create draft feature</h2>

        <label for="name">Name:</label>
        <input id="name" type="text" bind:value={name} required />

        <label for="objective">Objective:</label>
        <textarea id="objective" rows="5"  bind:value={objective} required />

        <label for="flowOverview">Flow overview:</label>
        <textarea id="flowOverview" rows="5"  bind:value={flowOverview} required />

        <div>
            <button type="button" on:click={submit}>Create</button>
            <button type="button" on:click={closeDialog}>Cancel</button>
        </div>
    </form>
</dialog>

<style>
    .backdrop {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(100, 100, 100, 0.5); /* Semi-transparent background */
        z-index: 9; /* Ensures backdrop is behind the modal */
    }


    dialog{
        z-index: 10;
        top:100px;
        margin-left: auto;
        margin-right: auto;
        padding: 25px;
        border-radius: 15px;
        border-style: solid;
        border-width: 1px;
        background-color: #2b2b2b;
        box-shadow: 0px 0px 8px #3c3c3c;
    }
    dialog button {
        background-color:   #2b2b2b;
        color: #e0e0e0;
        padding: 12px;
        border-radius: 8px;
        border-width: 1px;
        border-style: solid;
        margin: 10px 10px 0 0;
    }

    dialog h2 {
        color: #e0e0e0;
    }
    dialog label {
        color: #e0e0e0;
    }
    dialog input {
        color: #e0e0e0;
    }
    .showCreateDialog {
        margin-bottom: 5px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 25px;
        height: 25px;
        background-color: #3c3c3c; /* Slightly lighter for hover contrast */
        text-align: center;
        font-size: 9px;
        color: #e0e0e0;
        transition: background-color 0.3s, color 0.3s;
        cursor: pointer;
        border-radius: 25%;

    }

    .showCreateDialog:hover {
        background-color: #575757; /* Hover effect */
        color: #fff;
    }
</style>