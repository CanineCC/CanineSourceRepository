<script lang="ts">
    import ServerStatus from 'components/ServerStatus.svelte';
    import { onMount } from 'svelte';
    import { isAppInitialized  } from 'lib/stores'
    import { startConnection, stopConnection } from 'signalRService'

    let initialized = false;
    isAppInitialized.subscribe(value => {
        initialized = value;
    });

    onMount(() => {
        if (!initialized) {
            console.log("App loaded for the first time, running startup logic");
            startConnection();
            isAppInitialized.set(true);
        }

        const handleVisibilityChange = () => {
            if (document.hidden) {
                stopConnection(); // Close WebSocket if the page is hidden
            } else {
                startConnection(); // Reopen WebSocket if the page is visible
            }
        };

        document.addEventListener('visibilitychange', handleVisibilityChange);
    });
</script>

<header>
    <nav>
        <ul style="margin:14px 0">
            <li><a href="/">Home</a></li>
            <li><a href="/profile">Profile</a></li>
            <li><a href="/logout">Logout</a></li>
        </ul>
    </nav>
</header>

<div class="content-wrapper">
    <aside class="left-menu">
        <ul>
            <li><a href="/dashboard" title="Dashboard" aria-label="Dashboard" ><i class="fas fa-chart-line"></i></a></li>
            <li><a href="/systemhealth" title="System health" aria-label="System health"><i class="fas fa-gauge-simple"></i></a></li>
            <li><a href="/develop" title="Develop" aria-label="Develop"><i class="fas fa-code"></i></a></li>
            <li><a href="/documentation" title="C4 documentation" aria-label="Documentation"><i class="fas fa-book"></i></a></li>
            <li><a href="/profile" title="Profile" aria-label="Profile"><i class="fas fa-user"></i></a></li>
        </ul>
    </aside>

    <main class="main-content">
        <slot /> <!-- This is where the content of the page will be rendered -->
    </main>
</div>

<footer>
    <div style="display: inline-flex; align-items: center; column-gap:25px;">
        <p class="copyright" title="Canine Source Repository © 2024 Canine Development ApS">© 2024 Canine Development ApS</p>
        <ServerStatus />
    </div>
</footer>

<style>
    /* Header: Fixed at the top */
    header {
        user-select: none;
        background: #3C6275; /* Darker background for header */
        color: #e0e0e0; /* Light text */
        padding: 0em 2em;
        font-family: 'Arial', sans-serif;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.5);
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
        color: #e0e0e0;
        text-decoration: none;
        padding: 0.5em 1em;
        border-radius: 5px;
        transition: background-color 0.3s ease, color 0.3s ease;
    }

    nav ul li a:hover {
        background-color: rgba(255, 255, 255, 0.1);
        color: #fff;
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
        background-color: #2b2b2b; /* Darker sidebar */
        user-select: none;
        left: 0px;
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
        background-color: #3c3c3c; /* Slightly lighter for hover contrast */
        border-radius: 25%;
        text-align: center;
        font-size: 24px;
        color: #e0e0e0;
        transition: background-color 0.3s, color 0.3s;
    }

    .left-menu ul li a:hover {
        background-color: #575757; /* Hover effect */
        color: #fff;
    }

    /* Main Content */
    .main-content {
        margin-left: 95px; /* Make room for the left sidebar */
        flex: 1;
        width: calc(100% - 135px);
        padding: 20px;
        background-color: #1e1e1e; /* Dark background for content */
        color: #e0e0e0; /* Light text for content */
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.5);
    }

    /* Footer: Fixed at the bottom */
    footer {
        background: #20353f;
        color: #e0e0e0;
        text-align: center;
        line-height: 25px;
        border-top: 1px solid #333;
        position: fixed;
        bottom: 0;
        left: 0;
        right: 0;
        z-index: 1000;
        height: 50px;
    }

    .copyright {
        cursor: pointer;
        color: #bbb;
        user-select: none;
        font-size: 0.75em;
    }

    .copyright:hover {
        color: #fff;
        font-size: 0.8em;
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
