<script lang="ts">
  import type {BpnTask, NamedConfiguration, TestCase} from 'BpnEngineClient'; // Import your types
    import { DraftFeatureTaskApi, NamedConfigurationApi  } from 'BpnEngineClient/apis';
  import {onMount} from "svelte"; // Adjust the path accordingly
    export let task: BpnTask; // Accepts a BpnTask as a prop
    export let featureId: string;
    export let readonly: boolean = false;
    let newRecordTypeName : string = "";
    let selectedTestCaseId : string = "";
    let selectedTestCase : TestCase | null = null;
    $: selectedTestCase = task.testCases.find(test => test.id === selectedTestCaseId) || null;

    const namedConfigurationApi = new NamedConfigurationApi();
    const draftFeatureTaskApi = new DraftFeatureTaskApi();

    let namedConfigurations : Array<NamedConfiguration> = [];
  onMount(async () => {
    namedConfigurations = await namedConfigurationApi.getAllNamedConfigurations()

  });

  function updateDependency() {
    draftFeatureTaskApi.updateServiceDependencyFeature({updateServiceDependencyBody: { featureId: featureId, taskId: task.id!, serviceDependency: task.serviceDependency, namedConfiguration: task.namedConfiguration}});
  }
  function addRecordType() {
    draftFeatureTaskApi.addRecordToTaskFeature({ addRecordToTaskBody:{ featureId:featureId, taskId:task.id!, recordDefinition:{ name: newRecordTypeName, fields:[]} } });
    newRecordTypeName = "";
  }
  function removeRecordType(index: number) {
    draftFeatureTaskApi.deleteRecordOnTaskFeature({deleteRecordOnTaskBody: { featureId: featureId, taskId:task.id!, name: task.recordTypes[index].name}});
  }
  function updateRecordType(index :number) {

    draftFeatureTaskApi.updateRecordOnTaskFeature({updateRecordOnTaskBody: { featureId: featureId, taskId: task.id!, recordIndex: index , recordDefinition: task.recordTypes[index]}});
  }
  function addFieldToRecordType(index : number){
    task.recordTypes[index].fields.push({ name:"<new>", type:"string", isMandatory:false, isCollection :false });
    draftFeatureTaskApi.updateRecordOnTaskFeature({updateRecordOnTaskBody: { featureId: featureId, taskId: task.id!, recordIndex: index , recordDefinition: task.recordTypes[index]}});
  }

  //NEXT STEPS:
  // - ASSERTIONS
  // - DependencyInjection/Services list (include a mock for each) ==> Consider how to help for non-developers (SQL, EventSourced, JBOD/JSON, Binary)
  // - Named configuration (pr. service) / VAULT !
  // - DRAFT => DEVELOPMENT-ENV => TEST-ENV => STAG-ENV => PROD-ENV (version <Prod.Stag.Test.Development> ) (review/approval between each, different goal/objective)
  // - SOFTWARE SYSTEM (name, purpose) => CONTAINER (name, type ex. webapi) => CONTEXT => FEATURES/CODE ... C4

    // Dropdown options for serviceDependency (will come from actual services in future)
    //const serviceOptions = ["Database - postgresql", "Message Queue - RabbitMQ"];
    // Placeholder for named configuration (can later be fetched based on serviceDependency)
    //const namedConfigOptions = ["customer database", "inventory database"];
  
    let activeTab = 'overview'; 
    function setActiveTab(tab: string) {
      activeTab = tab;
    }

    async function saveFeaturePurpose() {
        await draftFeatureTaskApi.updateTaskPurposeFeature({updateTaskPurposeFeatureBody: {
            featureId: featureId, 
            taskId: task.id!,
            name: task.name,
            businessPurpose: task.businessPurpose,
            behavioralGoal: task.behavioralGoal
        }});
    }


    async function  addTestCase() {
      await draftFeatureTaskApi.addTestCase({addTestCaseToTaskBody: {
        featureId: featureId,
        taskId: task.id!,
        name: "<Test case>",
        input: "{}",
        asserts: []
      }});
    }
    async function  saveTestCase() {
      if (selectedTestCase == null) return;
      selectedTestCase.asserts.push({ field:"", operation: 'Equal', expectedValue: ""});
      await  draftFeatureTaskApi.updateTestCase({ updateTestCaseOnTaskBody: {
          featureId: featureId,
          taskId: task.id!,
          testCase: selectedTestCase
      } })

    }

    async function saveServiceDependency() {
        await draftFeatureTaskApi.updateServiceDependencyFeature({updateServiceDependencyBody: {
            featureId: featureId, 
            taskId: task.id!,
            serviceDependency: task.serviceDependency,
            namedConfiguration: task.namedConfiguration
        }});
    }
    //TODO featureTaskApi.
        //TODO featureTaskApi.AddAssertion
        //TODO featureTaskApi.UpdateAssertion
        //TODO featureTaskApi.DeleteAssertion
        //TODO featureTaskApi.UpdateCode
        //TODO: Add Assertion-list to task!

       /*await draftFeatureApi.updateDraftFeaturePurpose({ updateDraftFeaturePurposeBody : {
            featureId: featureId,
            name: task.name,
            objective: task.businessPurpose,
            flowOverview: task.behavioralGoal
         }});*/
        </script>
  
  <!-- Tab component with vertical layout -->
  <div class="tab-container">
    <div class="tabs">
      <button class={activeTab === 'overview' ? 'active' : ''} on:click={() => setActiveTab('overview')}>Overview</button>
      <button class={activeTab === 'data' ? 'active' : ''} on:click={() => setActiveTab('data')}>Data Structures</button>
      <button class={activeTab === 'inputoutput' ? 'active' : ''} on:click={() => setActiveTab('inputoutput')}>Input & output</button>
      <button class={activeTab === 'service-dependency' ? 'active' : ''} on:click={() => setActiveTab('service-dependency')}>Service dependency</button>
      <button class={activeTab === 'verification' ? 'active' : ''} on:click={() => setActiveTab('verification')}>Verification</button>
      <button class={activeTab === 'code' ? 'active' : ''} on:click={() => setActiveTab('code')}>Code</button>
    </div>
  
    <div class="tab-content">
      {#if activeTab === 'overview'}
        <div>
          <label for="task-name">Name:</label>
          <input id="task-name" readonly={readonly} type="text" bind:value={task.name} placeholder="Task Name">
        </div>
        <div>
          <label for="business-purpose">Business Purpose:</label>
          <textarea id="business-purpose" readonly={readonly} rows="5" bind:value={task.businessPurpose} placeholder="Business Purpose"></textarea>
        </div>
        <div>
          <label for="behavioral-goal">Behavioral Goal:</label>
          <textarea id="behavioral-goal" readonly={readonly} rows="5" bind:value={task.behavioralGoal} placeholder="Behavioral Goal"></textarea>
        </div>
        <a href="#top" title="Save" class="button" on:click={saveFeaturePurpose}><i class="fas fa-save "></i></a>

      {/if}
      {#if activeTab === 'verification'}
      <div>
        <div style="display: grid; grid-template-columns: auto 50px; gap: 12px;">
          <select id="testCases" bind:value={selectedTestCaseId}>
            <option>Select a Test case</option>
            {#each task.testCases as test}
                <option value={test.id}>{test.name}</option>
            {/each}
          </select>
          <a id="addTestCase" href="#addTestCase" title="Add test case"class="button" on:click={addTestCase}><i class="fas fa-plus "></i></a>
        </div>
        {#if selectedTestCase != null}
          <input type="text" bind:value={selectedTestCase.name} placeholder="Name">
          <input type="text" bind:value={selectedTestCase.input} placeholder="Input">
          {#each selectedTestCase.asserts as assert}
            {assert.field}
            {assert.operation.toString()}
            {assert.expectedValue}
          {/each}
          <a id="addAssert" href="#addAssert" title="Add assertion"class="button" on:click={saveTestCase}><i class="fas fa-save "></i></a>
        {/if}

      </div>
      {/if}
  
      {#if activeTab === 'code'}
        <p>Code editor will go here.</p>
      {/if}

      {#if activeTab === 'service-dependency'}
      <!--  <div>
            <label for="service-dependency">Service Dependency:</label>
            <select id="service-dependency" disabled={readonly} bind:value={task.serviceDependency}>
            <option disabled selected>Select a service</option>
            {#each serviceOptions as service}
                <option value={service}>{service}</option>
            {/each}
            </select>
        </div>-->
        <div>
            <label for="named-configuration">Named Configuration:</label>
            <select id="named-configuration" disabled={readonly} bind:value={task.namedConfigurationId}>
            <option disabled selected>Select a configuration</option>
            {#each namedConfigurations as config}
                <option value={config.id}>{config.name} ({config.serviceTypeName})</option>
            {/each}
            </select>
        </div>
        <a href="#top" title="Save" class="button" on:click={saveServiceDependency}><i class="fas fa-save "></i></a>
      {/if}
      
      {#if activeTab === 'inputoutput'}
      <div>
        <label for="input-record">Input Record:</label>
        <select disabled={readonly}  id="input-record" bind:value={task.input}>
          <option disabled selected>Select input</option>
          {#each task.recordTypes as recordType}
            <option value={recordType.name}>{recordType.name}</option>
          {/each}
        </select>

        <label for="output-record">Output Record:</label>
        <select disabled={readonly}  id="output-record" bind:value={task.output}>
          <option disabled selected>Select output</option>
          {#each task.recordTypes as recordType}
            <option value={recordType.name}>{recordType.name}</option>
          {/each}
        </select>
        {#if !readonly}
          <div style="display: flex;  padding: 12px 0; gap: 25px; flex-flow: row-reverse;">
            <a href="#editRecordType" title="Save"  class="button" on:click={updateDependency}><i class="fas fa-save "></i></a>
          </div>
          {/if}
      </div>
      {/if}

      {#if activeTab === 'data'}
        {#if !readonly}
          <h2>Create record type</h2>
          <div style="display: grid; grid-template-columns: auto 50px; gap: 12px;">
            <input type="text" bind:value={newRecordTypeName} placeholder="Record Type Name">
            <a id="addRecord" href="#addRecord" title="Add Record Type"class="button" on:click={addRecordType}><i class="fas fa-plus "></i></a>
          </div>
          <hr style="width: 95%; padding: 0; margin: 0 auto; background-color: #333;  border-color: #333;"/>
        {/if}
      <h2 id="editRecordType">Edit record type</h2>
      <table>
        <thead>
          <tr>
            <th >Record name</th>
            <th style="display: grid; grid-template-columns: auto auto 120px 120px; gap: 4px; padding: 0 2px;">
              <span>Field name</span>
              <span>Field type</span>
              <span>Mandatory</span>
              <span>Collection</span>
            </th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {#each task.recordTypes as recordType, i}
          <tr style="vertical-align: top;">
            <td>
              <input readonly={readonly}  type="text" bind:value={recordType.name} placeholder="Record Type Name">
            </td>
            <td>
              <ul>
                {#each recordType.fields as field}
                  <li style="display: grid; grid-template-columns: auto auto 120px 120px; gap: 4px; padding: 0 2px;">
                    <input readonly={readonly}  type="text" bind:value={field.name}>
                    <select disabled={readonly}  bind:value={field.type}>
                      <option disabled selected>Select data type</option>
                      {#each task.validDatatypes as dataType}
                        {#if dataType!=recordType.name}
                         <option value={dataType}>{dataType}</option>
                        {/if}
                      {/each}
                    </select>
                    <input type="checkbox" bind:checked={field.isMandatory} />
                    <input type="checkbox" bind:checked={field.isCollection} />
                  </li>
                {/each}
              {#if !readonly}
                <div style="display: flex;  gap: 25px; flex-flow: row-reverse;">
                  <a href="#editRecordType" title={`Add new field to '${recordType.name}'`}  class="button" on:click={() => { addFieldToRecordType(i);}}><i class="fas fa-plus "></i></a>
                </div>
              {/if}
          </ul>
            </td>
            <td style="width:145px">
              {#if !readonly}
                <div style="display: flex;  gap: 25px; flex-flow: row-reverse;">
                  <a href="#editRecordType" title={`Save '${recordType.name}'`}  class="button" on:click={() => { updateRecordType(i);}}><i class="fas fa-save "></i></a>
                  <a href="#editRecordType" title={`Delete '${recordType.name}'`} class="button" on:click={() => { removeRecordType(i);}}><i class="fas fa-trash "></i></a>
                </div>
              {/if}
            </td>

          </tr>
        {/each}
        </tbody>
      </table>
    {/if}
  
    </div>
  </div>
  
  <style>
    .tab-container {
      display: flex; /* Use flexbox for layout */
      width: 100%; /* Full width for the container */
      border: 1px solid #3c3c3c; /* Border for the overall container */
      border-radius: 5px; /* Rounded corners */
      overflow: hidden; /* Hide overflow */
    }
  
    .tabs {
      display: flex; /* Use flexbox for horizontal button layout */
      flex-direction: column; /* Stack buttons vertically */
      width: 150px; /* Fixed width for the tab column */
      background: #3c3c3c; /* Background for tab buttons */
      border-right: 1px solid #3c3c3c; /* Border between tabs and content */
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
      gap:25px;
      display: grid;
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
  