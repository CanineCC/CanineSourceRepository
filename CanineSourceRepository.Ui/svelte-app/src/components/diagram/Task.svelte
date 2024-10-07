<script lang="ts">
    import { createEventDispatcher } from 'svelte';
  
    // Props
    export let id: string;
    export let name: string;
    export let businessPurpose: string;
    export let position: { x?: number, y?: number };
  
    // Variables to track dragging state
    let dragging = false;
    let offsetX = 0;
    let offsetY = 0;
  
    // Create a dispatcher for emitting events
    const dispatch = createEventDispatcher();
  
    // Function to handle when dragging starts
    function startDrag(event: MouseEvent) {
      dragging = true;
      offsetX = event.clientX - (position.x??0);
      offsetY = event.clientY - (position.y??0);
    }
  
    // Function to handle dragging movement
    function drag(event: MouseEvent) {
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
      dragging = false;
    }
  </script>
  
  <!-- SVG group for task box and text -->
  <g on:mousedown={startDrag} on:mousemove={drag} on:mouseup={stopDrag} role="group" aria-label="Task: {name}">
    <rect
      x={position.x} y={position.y} width="100" height="50"
      fill="lightblue" stroke="black" />
    <text x={position.x??0 + 10} y={position.y??0 + 20} font-size="12">{name}</text>
    <text x={position.x??0 + 10} y={position.y??0 + 40} font-size="10">{businessPurpose}</text>
  </g>
  
  <style>
    rect {
      cursor: move;
    }
  </style>
  