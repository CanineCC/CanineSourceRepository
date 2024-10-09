<script lang="ts">
    import { createEventDispatcher } from 'svelte';
  
    // Props
    export let id: string;
    export let name: string;
    export let businessPurpose: string;
    export let position: { x?: number, y?: number };
    export let readonly : boolean;
  
    // Variables to track dragging state
    let dragging = false;
    let offsetX = 0;
    let offsetY = 0;
  
    // Create a dispatcher for emitting events
    const dispatch = createEventDispatcher();
  
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
    function stopDrag() {
      if (readonly) return;
      dragging = false;
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
  <g on:mousedown={startDrag} on:mousemove={drag} on:mouseup={stopDrag} role="group" aria-label="Task: {name}">
    <rect
      x={position.x} y={position.y} width="300" height="150"
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
  </g>
  
  <style>
    rect {
      cursor: move;
    }
  </style>
  