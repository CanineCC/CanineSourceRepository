<script>
    export let isLoggedIn; // Pass this prop to check if the user is logged in
</script>


<header>
    <nav>
        <ul>
            <li><a href="/">Home</a></li>
            {#if isLoggedIn}
                <li><a href="/profile">Profile</a></li>
                <li><a href="/logout">Logout</a></li>
            {:else}
                <li><a href="/login">Login</a></li>
            {/if}
        </ul>
    </nav>
</header>

<div class="content-wrapper">
    <aside class="left-menu">
        <ul>
            <li><a href="#"><i class="fas fa-tachometer-alt"></i></a></li> <!-- Dashboard Icon -->
            <li><a href="#"><i class="fas fa-cogs"></i></a></li>           <!-- Settings Icon -->
            <li><a href="#"><i class="fas fa-user"></i></a></li>           <!-- Profile Icon -->
        </ul>
    </aside>

    <main class="main-content">
        <slot /> <!-- This is where the content of the page will be rendered -->
    </main>
</div>

<footer>
    <p>© 2024 Canine Development ApS</p>
</footer>

<style>/* Body should take full height */
  /* Body should take full height */
    html, body {
        height: 100%;
        margin: 0;
    }

    /* Container to manage content height and layout */
    /*.container {
        display: flex;
        flex-direction: column;
        min-height: 100vh;
    }*/

    /* Header: Fixed at the top */
    header {
        background: #B0C4DE;
        color: #fff;
        padding: 0em 2em;
        font-family: 'Arial', sans-serif;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        position: fixed;
        line-height: 25px;
        top: 0;
        left: 0;
        right: 0;
        z-index: 1000;
    }

    /* Navigation Menu */
    nav ul {
        list-style: none;
        padding: 0;
        display: flex;
        justify-content: center;
        align-items: center;
    }

    nav ul li {
        margin-right: 20px;
        font-size: 1.1em;
        transition: transform 0.2s ease-in-out;
    }

    nav ul li a {
        color: #fff;
        text-decoration: none;
        padding: 0.5em 1em;
        border-radius: 5px;
        transition: background-color 0.3s ease, color 0.3s ease;
    }

    nav ul li a:hover {
        background-color: rgba(255, 255, 255, 0.2);
        color: #f0f0f0;
        transform: translateY(-2px);
    }

    /* Main Content Layout */
    .content-wrapper {
        display: flex;
        flex: 1;
        padding-top: 65px; /* Adjust based on header height */
        padding-bottom: 65px; /* Adjust based on footer height */
    }

    /* Left Sidebar Menu */
    .left-menu {
        background-color: #f0f0f0;
        left:0px;
        padding: 0px;
        position: fixed;
        top: 55px; /* Below the header */
        bottom: 25px; /* Above the footer */
        overflow-y: auto;
    }

    .left-menu ul {
        list-style-type: none;
        padding: 0px 10px;
    }

    .left-menu ul li {
        margin: 10px 0;
    }

    .left-menu ul li a {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 50px;
        height: 50px;
        background-color: #e0e0e0;
        border-radius: 25%;
        text-align: center;
        font-size: 24px; /* Adjust icon size */
        color: #333;
        transition: background-color 0.3s, color 0.3s;

    }

    .left-menu ul li a:hover {
        background-color: #b0c4de;
    }

    /* Main Content */
    .main-content {
        margin-left: 95px; /* Make room for the left sidebar */
        flex: 1;
        padding: 20px;
        background-color: #fff;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    /* Footer: Fixed at the bottom */
    footer {
        background: #f8f8f8;
        text-align: center;
        line-height: 25px;
        font-family: 'Arial', sans-serif;
        color: #555;
        font-size: 0.75em;
        border-top: 1px solid #ddd;
        position: fixed;
        bottom: 0;
        left: 0;
        right: 0;
        z-index: 1000;
    }

    /* Responsive Design */
    @media (max-width: 768px) {
        .left-menu {
            position: static;
            width: 100%;
            margin-bottom: 20px;
        }

        .main-content {
            margin-left: 0;
        }

        nav ul {
            flex-direction: column;
        }
    }

    
</style>
