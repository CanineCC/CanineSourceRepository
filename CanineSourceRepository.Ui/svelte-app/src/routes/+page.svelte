<script lang="ts">

   import Layout from '../components/Layout.svelte';
    let isLoggedIn = false; // Replace this with your actual login state logic   
    //export let name: string = 'World'; // TypeScript variable declaration
</script>

<Layout {isLoggedIn}>
    <h1>Welcome to SvelteKit</h1>

</Layout>
