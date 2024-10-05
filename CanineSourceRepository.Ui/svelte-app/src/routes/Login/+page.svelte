<script lang="ts">
    import type { PageData } from './$types';
    import Layout from '../../components/Layout.svelte';

    export let data: PageData;
    let isLoggedIn = false; // Replace this with your actual login state logic   
</script>

<Layout {isLoggedIn}>
    <h1>Login</h1>
</Layout>
