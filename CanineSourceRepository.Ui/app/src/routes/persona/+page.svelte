<script lang="ts">
    import {PersonaApi} from 'BpnEngineClient/apis';
  //  import type {BpnSolution, BpnSystem, BpnWebApiContainer} from "BpnEngineClient";
    import Layout from '@/+layout.svelte';
    import {onDestroy, onMount} from "svelte";
    import type {Persona4} from "BpnEngineClient";
//    import {joinGroupView, leaveGroupView, onGroupUpdate} from "signalRService";
    const personaApi = new PersonaApi();
    let personas : Array<Persona4> = [];

    onMount(async () => {
        personas = await personaApi.getAllPersonas();
        //onGroupUpdate(callback);
        //await joinGroupView("bpnsystem");
        //await fetchDetails();
    });
    onDestroy(() => {
        //clearInterval(intervalId);
        //leaveGroupView("bpnsystem");
    });

</script>

<Layout>
    <h1>Personas</h1>

    <p style="color: red">TODO: ADD persona</p>
    <p style="color: red">TODO: REMOVE persona</p>

    <p style="color: red">TODO: TABLE: [Delete btn][Persona.Name]|[System.Name]|[Components>name]</p>
    {#each personas as persona}
        <p>{persona.name}</p>
    {/each}
</Layout>

