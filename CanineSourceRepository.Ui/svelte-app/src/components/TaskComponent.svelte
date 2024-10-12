<script lang="ts">
    import { onMount } from 'svelte';
    import type { BpnTask, RecordDefinition, Environment } from '../BpnEngineClient'; // Import your types
  
    export let task: BpnTask; // Accepts a BpnTask as a prop
  
    // Helper to initialize recordTypes if it's undefined
    $: task.recordTypes = task.recordTypes || [];
  
    // Add a new record definition to the task
    function addRecordType() {
      if (task.recordTypes)
        task.recordTypes.push({ name: '', fields: [] });
    }
  
    // Remove a record definition
    function removeRecordType(index: number) {
      if (task.recordTypes)
        task.recordTypes.splice(index, 1);
    }
  
    // Dropdown options for serviceDependency (will come from actual services in future)
    const serviceOptions = ["Database - postgresql", "Message Queue - RabbitMQ"];
  
    // Placeholder for named configuration (can later be fetched based on serviceDependency)
    const namedConfigOptions = ["customer database", "inventory database"];
  
    let activeTab = 'overview'; // Default active tab
  
    // Function to set active tab
    function setActiveTab(tab: string) {
      activeTab = tab;
    }
  </script>
  
  <!-- Tab component with vertical layout -->
  <div class="tab-container">
    <div class="tabs">
      <button class={activeTab === 'overview' ? 'active' : ''} on:click={() => setActiveTab('overview')}>Overview</button>
      <button class={activeTab === 'data' ? 'active' : ''} on:click={() => setActiveTab('data')}>Data Structures</button>
      <button class={activeTab === 'verification' ? 'active' : ''} on:click={() => setActiveTab('verification')}>Verification</button>
      <button class={activeTab === 'code' ? 'active' : ''} on:click={() => setActiveTab('code')}>Code Viewer</button>
    </div>
  
    <div class="tab-content">
      {#if activeTab === 'overview'}
        <h3>Overview</h3>
        <div>
          <label for="task-name">Name:</label>
          <input id="task-name" type="text" bind:value={task.name} placeholder="Task Name">
        </div>
        <div>
          <label for="business-purpose">Business Purpose:</label>
          <textarea id="business-purpose" rows="5" bind:value={task.businessPurpose} placeholder="Business Purpose"></textarea>
        </div>
        <div>
          <label for="behavioral-goal">Behavioral Goal:</label>
          <textarea id="behavioral-goal" rows="5" bind:value={task.behavioralGoal} placeholder="Behavioral Goal"></textarea>
        </div>
        <div>
          <label for="service-dependency">Service Dependency:</label>
          <select id="service-dependency" bind:value={task.serviceDependency}>
            <option disabled selected>Select a service</option>
            {#each serviceOptions as service}
              <option value={service}>{service}</option>
            {/each}
          </select>
        </div>
        <div>
          <label for="named-configuration">Named Configuration:</label>
          <select id="named-configuration" bind:value={task.namedConfiguration}>
            <option disabled selected>Select a configuration</option>
            {#each namedConfigOptions as config}
              <option value={config}>{config}</option>
            {/each}
          </select>
        </div>
      {/if}
  
      {#if activeTab === 'data'}
        <h3>Data Structures</h3>
        <button on:click={addRecordType}>Add Record Type</button>
  
        <ul>
          {#if task.recordTypes}
          {#each task.recordTypes as recordType, i}
            <li>
              <input type="text" bind:value={recordType.name} placeholder="Record Type Name">
              <button on:click={() => removeRecordType(i)}>Delete</button>
              <ul>
                {#if recordType.fields}
                {#each recordType.fields as field}
                  <li>{field.name} ({field.type})</li>
                {/each}
                {/if}
              </ul>
            </li>
          {/each}
          {/if}
        </ul>
  
        <div>
          <h4>Select Input/Output Record Type</h4>
          <label for="input-record">Input Record:</label>
          <select id="input-record" bind:value={task.input}>
            <option disabled selected>Select input</option>
            {#if task.recordTypes}
            {#each task.recordTypes as recordType}
              <option value={recordType.name}>{recordType.name}</option>
            {/each}
            {/if}
          </select>
  
          <label for="output-record">Output Record:</label>
          <select id="output-record" bind:value={task.output}>
            <option disabled selected>Select output</option>
            {#if task.recordTypes}
            {#each task.recordTypes as recordType}
              <option value={recordType.name}>{recordType.name}</option>
            {/each}
            {/if}
          </select>
        </div>
      {/if}
  
      {#if activeTab === 'verification'}
        <h3>Verification</h3>
        <p>BDD test cases will be listed here.</p>
      {/if}
  
      {#if activeTab === 'code'}
        <h3>Code Viewer</h3>
        <p>Code editor will go here.</p>
      {/if}
    </div>
  </div>
  
  <style>
    .tab-container {
      display: flex; /* Use flexbox for layout */
      width: 100%; /* Full width for the container */
      border: 1px solid #ccc; /* Border for the overall container */
      border-radius: 5px; /* Rounded corners */
      overflow: hidden; /* Hide overflow */
    }
  
    .tabs {
      display: flex; /* Use flexbox for horizontal button layout */
      flex-direction: column; /* Stack buttons vertically */
      width: 150px; /* Fixed width for the tab column */
      background: #3c3c3c; /* Background for tab buttons */
      border-right: 1px solid #ccc; /* Border between tabs and content */
      padding: 10px; /* Padding inside tab column */
    }
  
    .tabs button {
      padding: 10px;
      color: #e0e0e0;
      background: transparent; /* Transparent background */
      border: none; /* No border */
      text-align: left; /* Align text to the left */
      cursor: pointer; /* Pointer cursor on hover */
      margin: 5px 0; /* Margin between buttons */
      transition: background 0.3s; /* Smooth background transition */
    }
  
    .tabs button.active {
      background: #707070; /* Highlight active tab button */
      font-weight: bold; /* Make active tab text bold */
    }
  
    .tabs button:hover {
      background: #575757; /* Hover effect for buttons */
    }
  
    .tab-content {
      flex: 1; /* Take the remaining space */
      padding: 20px; /* Padding inside content area */
      overflow-y: auto; /* Vertical scroll for content if needed */
    }

    button {
      margin-top: 10px;
      padding: 5px 10px;
      cursor: pointer; /* Pointer cursor */
    }
  
    ul {
      list-style-type: none; /* Remove default list styles */
      padding: 0; /* Remove padding */
    }
  
    ul li {
      margin-bottom: 10px; /* Space between list items */
    }
  </style>
  