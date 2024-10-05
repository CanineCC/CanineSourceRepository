<script lang="ts">
    //import type { PageData } from './$types';
    import Layout from '../../components/Layout.svelte';
    import { onMount } from 'svelte';
    import { ContextApi, ServerApi } from '../../BpnEngineClient/apis'; // Adjust the path accordingly
    import type { BpnContext, DurationClassification } from '../../BpnEngineClient/models'; // Adjust the path accordingly
    import { slide } from 'svelte/transition';
    import { goto } from '$app/navigation';

    let contexts: BpnContext[] = [];
    let durationClasses: DurationClassification[] = [];
    let expandedRow: number | null = null; // Track which row is expanded
  
    const contextApi = new ContextApi();
    const serverApi = new ServerApi();
  
    // Fetch data on component mount
    onMount(async () => {
        durationClasses = await serverApi.getDurationClassification();
        contexts = await contextApi.getAllContexts();
    });
  
    // Function to toggle the expanded row
    function toggleRow(index: number) {
      expandedRow = expandedRow === index ? null : index;
    }
  
    // Function to format date strings
    function formatDate(date: Date | undefined | null): string {
        if (!date) return "";
      const now = new Date();
      const diff = now.getTime() - date.getTime();
      const seconds = Math.floor(diff / 1000);
      const minutes = Math.floor(seconds / 60);
      const hours = Math.floor(minutes / 60);
      const days = Math.floor(hours / 24);
      const years = Math.floor(days / 365);
  
      if (seconds < 60) return `${seconds} seconds ago`;
      if (minutes < 60) return `${minutes} minutes ago`;
      if (hours < 24) return `${hours} hours ago`;
      if (days < 365) return `${days} days ago`;
      return `${years} years ago`;
    }
    function getDurationColor(duration: number | undefined): string {
        if (!duration) return "#000";
        const classification = durationClasses.find(dc =>  (duration >= dc.fromMs!) && (duration <= dc.toMs!));
        return classification && classification.hexColor ? classification.hexColor : '#000'; // Default to black if no classification found
    }
    function getDurationText(duration: number | undefined): string {
        if (!duration) return "";
      const classification = durationClasses.find(dc =>  (duration >= dc.fromMs!) && (duration <= dc.toMs!));
      return classification && classification.category ? classification.category : '---'; 
    }
    
// Calculate summaries for each context
    function getFeatureSummaries(context: BpnContext) {
        let totalInvocations = 0;
        let totalErrors = 0;
        let totalCompleted = 0;
        let totalInProgress = 0;
        let maxDuration = 0;
        let minDuration = Infinity;
        let avgOfAverages = 0;
        let totalAvgDurations = 0;
        let avgCount = 0;
        let lastUsed:Date | undefined | null = undefined;

        context.features?.forEach(feature => {
            if (!feature.versions) return;
        feature.versions.forEach(version => {
            if (!version.stats) return;

            totalInvocations += version.stats.invocationCount??0;
            totalErrors += version.stats.invocationErrorCount??0;
            totalCompleted += version.stats.invocationCompletedCount??0;
            totalInProgress += version.stats.invocationsInProgressCount??0;
            
            // Find max duration
            if (version.stats.maxDurationMs && version.stats.maxDurationMs > maxDuration) {
            maxDuration = version.stats.maxDurationMs;
            }
            
            // Find min duration
            if (version.stats.minDurationMs && version.stats.minDurationMs < minDuration) {
            minDuration = version.stats.minDurationMs;
            }
            
            // Calculate avg of averages
            if (version.stats.avgDurationMs){
                totalAvgDurations += version.stats.avgDurationMs;
                avgCount++;
            }

            if (!lastUsed || (version.stats.lastUsed && version.stats.lastUsed > lastUsed))
            {
                lastUsed = version.stats.lastUsed;
            }
        });
        });

        avgOfAverages = totalAvgDurations / avgCount || 0;

        return {
        totalInvocations,
        totalErrors,
        totalCompleted,
        totalInProgress,
        maxDuration,
        minDuration: minDuration === Infinity ? 0 : minDuration,
        avgOfAverages,
        lastUsed
        };
    }

    function editVersion(contextId: string, featureId: string, versionId: string) {
        goto(`/Feature/Edit/${contextId}/${featureId}/${versionId}`);
    }
    //export let data: PageData;
    let isLoggedIn = false; // Replace this with your actual login state logic   
  </script>

  <style>
    table {
      width: 100%;
      border-collapse: collapse;
    }
    
    th, td {
      padding: 8px 12px;
      border: 1px solid #ddd;
    }
  
    .expandable-row {
      background-color: #f9f9f9;
    }
  
    .hidden {
      display: none;
    }
    .number-column {
        text-align: right; /* Right-align text in number columns */
        max-width: 100px;
        width: 100px;
        min-width: 100px;
        box-sizing: border-box;
    }  
    .title-column {
        max-width: 380px;
        width:380px;
        min-width: 380px;
        box-sizing: border-box;
    }  
    .date-column {
        max-width: 200px;
        width:200px;
        min-width: 200px;
        box-sizing: border-box;
    }  
    
    .tooltip {
      position: relative;
      cursor: pointer;
    }
  
    .tooltip:hover::after {
      content: attr(data-tooltip);
      position: absolute;
      left: 50%;
      transform: translateX(-50%);
      bottom: 100%;
      background-color: #333;
      color: white;
      padding: 5px;
      border-radius: 3px;
      white-space: nowrap;
      z-index: 10;
    }


  .expanded-content {
    overflow: hidden; /* Prevents content from overflowing */
    transition: max-height 3.3s ease; /* Smooth transition */
    height: 0; /* Start collapsed */
  }

   </style>
  
  <Layout {isLoggedIn}>
    <h1>Settings</h1>

  <div>
    <h1>Contexts Table</h1>
    <table>
      <thead>
        <tr>
          <th>Context</th>
          <th>Invocations</th>
          <th>Errors</th>
          <th>Completed</th>
          <th>Processing</th>
          <th>Max duration</th>
          <th>Avg duration</th>
          <th>Min duration</th>
          <th>Last used</th>

          <th>Last updated</th>
          <th>Created</th>

        </tr>
      </thead>
      <tbody>
        {#each contexts as context, index}
          <tr>
            <td class="title-column">
                <button on:click={() => toggleRow(index)} style="border: none; background: none; cursor: pointer;">
                    <i class={`fas ${expandedRow === index ? 'fa-chevron-up' : 'fa-chevron-down'}`}></i>
                </button>
                {context.name}
            </td>

            {#await Promise.resolve(getFeatureSummaries(context)) then summary}
                <td class="number-column">{summary.totalInvocations}</td>
                <td class="number-column">{summary.totalErrors}</td>
                <td class="number-column">{summary.totalCompleted}</td>
                <td class="number-column">{summary.totalInProgress}</td>
                <td class="tooltip number-column" data-tooltip={getDurationText(summary.maxDuration)} style="color: {getDurationColor(summary.maxDuration)};">
                    {summary.maxDuration ? Math.round(summary.maxDuration) + ' ms' : '-'} 
                </td>
                <td class="tooltip number-column" data-tooltip={getDurationText(summary.avgOfAverages)} style="color: {getDurationColor(summary.avgOfAverages)};">
                    {summary.avgOfAverages ? Math.round(summary.avgOfAverages) + ' ms' : '-'} 
                </td>
                <td class="tooltip number-column" data-tooltip={getDurationText(summary.minDuration)} style="color: {getDurationColor(summary.minDuration)};">
                    {summary.minDuration ? Math.round(summary.minDuration) + ' ms' : '-'} 
                </td>
                <td class="tooltip date-column" data-tooltip={summary.lastUsed}>
                    {formatDate(summary.lastUsed)}
                  </td>
            {/await}          


            <td class="tooltip date-column" data-tooltip={context.lastUpdatedTimestamp}>
                {formatDate(context.lastUpdatedTimestamp)}
              </td>
              <td class="tooltip" data-tooltip={context.createdTimestamp}>
                {formatDate(context.createdTimestamp)}
              </td>
          </tr>
          {#if expandedRow === index}
            <tr class="expandable-row">
              <td colspan="11" style="padding:0; border:0;">
                <div transition:slide>
                <table>
                  <thead>
                    <tr>
                      <th>Feature name</th>
                      <th>Invocations</th>
                      <th>Errors</th>
                      <th>Completed</th>
                      <th>Processing</th>
                      <th>Max duration</th>
                      <th>Avg duration</th>
                      <th>Min duration</th>

                      <th>Last Used</th>
                      <th>Version</th>

                    </tr>
                  </thead>
                  <tbody>
                    {#if context && context.features} 
                    {#each context.features as feature}
                      {#if feature && feature.versions} 
                      {#each feature.versions as version}
                        {#if version.stats} 
                            <tr>
                                <td class="title-column">
                                    
                                    <button on:click={() => editVersion(context.id??"", feature.id??"", version.version?.toString() ?? "")} style="border: none; background: none; cursor: pointer;">
                                        <i class="fas fa-edit"></i>
                                    </button>
                                    {version.name}
                                
                                </td>
                                <td class="number-column">{version.stats.invocationCount}</td>
                                <td class="number-column">{version.stats.invocationErrorCount}</td>
                                <td class="number-column">{version.stats.invocationCompletedCount}</td>
                                <td class="number-column">{version.stats.invocationsInProgressCount}</td>
                                <td class="tooltip number-column" data-tooltip={getDurationText(version.stats.maxDurationMs)} style="color: {getDurationColor(version.stats.maxDurationMs)};">
                                    {version.stats.maxDurationMs ? Math.round(version.stats.maxDurationMs) + ' ms' : '-'} 
                                </td>
                                <td class="tooltip number-column" data-tooltip={getDurationText(version.stats.avgDurationMs)} style="color: {getDurationColor(version.stats.avgDurationMs)};">
                                    {version.stats.avgDurationMs ? Math.round(version.stats.avgDurationMs) + ' ms' : '-'} 
                                </td>
                                <td class="tooltip number-column" data-tooltip={getDurationText(version.stats.minDurationMs)} style="color: {getDurationColor(version.stats.minDurationMs)};">
                                    {version.stats.minDurationMs ? Math.round(version.stats.minDurationMs) + ' ms' : '-'} 
                                </td>
                                <td class="tooltip date-column" data-tooltip={version.stats.lastUsed}>
                                    {formatDate(version.stats.lastUsed)}
                                </td>
                                <td>{version.version == -1 ? "draft" : "v"+version.version}</td>
                            </tr>
                        {/if}
                      {/each}
                      {/if}
                    {/each}
                    {/if}
                    </tbody>
                </table>
                </div>
            </td>
        </tr>
        {/if}
        {/each}
      </tbody>
    </table>
  </div>
</Layout>
  



