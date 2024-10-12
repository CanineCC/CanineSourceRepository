<script lang="ts">
    import { onMount } from 'svelte';
    import { ServerApi } from '../BpnEngineClient/apis'; // Adjust the path accordingly
    import type { DurationClassification } from '../BpnEngineClient/models'; // Adjust the path accordingly
    import { formatDurationShort, formatDurationLong } from '../lib/Duration';
  
    export let duration: number | undefined = undefined;
  
    const serverApi = new ServerApi();
    let durationClasses: DurationClassification[] = [];
    let isLoaded = false; // Tracks if durationClasses have been loaded
    let color = '#eee'; // Default color
    let text = ''; // Default category text
  
    // Fetch the duration classifications on mount
    onMount(async () => {
      durationClasses = await serverApi.getDurationClassification();
      isLoaded = true; // Mark that the data has been loaded
    });
  
    // Reactive statement to update color and text when data and duration are available
    $: if (isLoaded && duration !== undefined) {
      const classification = durationClasses.find(
        (dc) => duration >= dc.fromMs! && duration <= dc.toMs!
      );
      color = classification?.hexColor || '#eee'; // Default to black if no classification found
      text = classification?.category || '---'; // Default to '---' if no category found
    }
  </script>
  
  <span
    class="tooltip"
    data-tooltip={text + "\n" + formatDurationLong(duration ?? 0)}
    style="color: {isLoaded ? color : '#eee'};">
    {duration ? formatDurationShort(duration) : '-'}
  </span>
  
  <style>
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
      white-space: pre; /* Interpret \n as a line break */
      text-align: center;
      z-index: 10;
    }
  </style>
  