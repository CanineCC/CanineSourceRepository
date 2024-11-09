<script lang="ts">
    import { createEventDispatcher } from 'svelte';
    import { onMount } from "svelte";
    import type {  TaskStats, DurationClassification } from "BpnEngineClient/models";
    import { formatDurationShort, formatDurationLong } from 'lib/Duration';
    import { FeatureTaskApi, FeatureApi, ServerApi  } from 'BpnEngineClient/apis'; // Adjust the path accordingly

    const featureTaskApi = new FeatureTaskApi();
    // Props
    export let id: string;
    export let name: string;
    export let businessPurpose: string;
    export let position: { x?: number, y?: number };
    export let readonly : boolean;
    export let stats : TaskStats | undefined;
  
    // Variables to track dragging state
    let dragging = false;
    let offsetX = 0;
    let offsetY = 0;
  
    // Create a dispatcher for emitting events
    const dispatch = createEventDispatcher();
  
    export let durationClasses: DurationClassification[] = []; // Receive as a prop

// Reactive statement to update color and text when data and duration are available
    function getColor(duration : number)
    {
      const classification = durationClasses.find(
            (dc) => duration >= dc.fromMs! && duration <= dc.toMs!
        );
        return classification?.hexColor || '#eee'; // Default to black if no classification found
    }

    onMount(async () => {
      durationClasses = await featureTaskApi.getTaskDurationClassification();
    });

    // Function to handle when dragging starts
    function startDrag(event: MouseEvent) {
      if (readonly) return;
      dragging = true;
      offsetX = event.clientX - (position.x??0);
      offsetY = event.clientY - (position.y??0);
    }
  
    // Function to handle dragging movement
    function drag(event: MouseEvent) {
      if (readonly) return;
      if (dragging) {
        position = {
          x: event.clientX - offsetX,
          y: event.clientY - offsetY
        };
        dispatch('dragmove', { id, position }); // Dispatch custom event with the new position
      }
    }
  
    // Function to handle when dragging stops
    function stopDrag(event: MouseEvent) {
      if (readonly) return;
      dragging = false;
      position = {
          x: event.clientX - offsetX,
          y: event.clientY - offsetY
        };

      dispatch('dragstopped', { id, position }); // Dispatch custom event with the new position
    }
  function handleClick() {
    dispatch('taskSelect', { id });
  }

  // Utility function to break text into lines
  function wrapText(text : string, maxWidth : number, lineHeight = 20) {
    const words = text.split(' ');
    let lines = [];
    let currentLine = words[0];

    for (let i = 1; i < words.length; i++) {
      const word = words[i];
      const width = getTextWidth(currentLine + ' ' + word);
      if (width < maxWidth) {
        currentLine += ' ' + word;
      } else {
        lines.push(currentLine);
        currentLine = word;
      }
    }
    lines.push(currentLine);
    return lines;
  }

  // Utility function to estimate the width of the text in the SVG (basic approximation)
  function getTextWidth(text : string) {
    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    const textElement = document.createElementNS("http://www.w3.org/2000/svg", "text");
    textElement.style.fontSize = '14px'; // Set the same font size as in the <text>
    textElement.textContent = text;
    svg.appendChild(textElement);
    document.body.appendChild(svg);
    const width = textElement.getBBox().width;
    document.body.removeChild(svg);
    return width;
  }
  let maxWidth = 300; // Set the max width for text wrapping
</script>
  
  <!-- SVG group for task box and text -->
{#if durationClasses.length > 0}
<!-- svelte-ignore a11y-mouse-events-have-key-events -->
<!-- svelte-ignore a11y-no-noninteractive-element-interactions -->

<g 
on:mousedown={startDrag} 
on:mousemove={drag} 
on:mouseup={stopDrag}  
on:click={handleClick} 
on:keypress={handleClick}
role="group" 
aria-label="Task: {name}"
>
<rect
x={position.x} y={position.y} width="300" height="150"
rx="15" ry="15"
fill="#B0C4DE" stroke="grey" />
<text  style="user-select: none;" x={(position.x??0) + 10} y={(position.y??0) + 20} font-size="14" font-weight="600">
{#each wrapText(name,maxWidth) as line, index}
  <tspan x={(position.x??0) + 10} dy={index === 0 ? 0 : 20}>{line}</tspan>
{/each}        
</text>
<text style="user-select: none;"  x={(position.x??0) + 10} y={(position.y??0) + 40} font-size="12">
  {#each wrapText(businessPurpose,maxWidth) as line, index}
  <tspan  x={(position.x??0) + 10} dy={index === 0 ? 0 : 20}>{line}</tspan>
{/each}        
</text>
{#if stats}
<text style="user-select: none;"  x={(position.x??0)} y={(position.y??0)+150} font-size="12">
    <tspan style='fill:{getColor(stats.maxDurationMs??0)};' x={(position.x??0)} dy={10}>
      Max: {stats.maxDurationMs ? formatDurationShort(stats.maxDurationMs) : '-'}
    </tspan>
    <tspan style='fill:{getColor(stats.avgDurationMs??0)};' x={(position.x??0)+150- (getTextWidth('Avg: ' + (stats.avgDurationMs ? formatDurationShort(stats.avgDurationMs) : '-'))/2)} dy={0}>
      Avg: {stats.avgDurationMs ? formatDurationShort(stats.avgDurationMs) : '-'}
    </tspan>
    <tspan style='fill:{getColor(stats.minDurationMs??0)};'x={(position.x??0)+300-getTextWidth('Min: ' + (stats.minDurationMs ? formatDurationShort(stats.minDurationMs) : '-'))} dy={0}>
      Min: {stats.minDurationMs ? formatDurationShort(stats.minDurationMs) : '-'}
    </tspan>
  </text>
{/if}        
</g>

{:else}
  <p>loading...</p>

{/if}
  <style>
    rect {
      cursor: move;
    }
  </style>
  