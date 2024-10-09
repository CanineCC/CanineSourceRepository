<script lang="ts">
    import Task from "./Task.svelte";
    import { createEventDispatcher } from "svelte";
    import { onMount } from "svelte";
    import type { BpnTask, BpnTransition, BpnFeatureDiagram, Position } from "../../BpnEngineClient/models";
  
    export let tasks: Array<BpnTask> = [];
    export let transitions: Array<BpnTransition> = [];
    export let diagram: BpnFeatureDiagram | undefined;
    export let width: number = 800;
    export let height: number = 600;
    export let readonly: boolean = false;
  
    let paths: Array<{ d: string; stroke: string; key: string }> = []; // Include a unique key for each path
    const dispatch = createEventDispatcher(); // Create event dispatcher

    // Function to handle drag and drop
    async function handleTaskDrag(event: any) {
      if (readonly) return;
      const { id, position } = event.detail;
      const task = diagram?.bpnPositions?.find(t => t.id === id);
      if (task) {
        task.position = position; // Update position of the task
        await preparePaths(); // Recalculate paths after moving a task

        dispatch('taskPositionChange', { taskId: id, position });
      }
    }
  
    // Function to export the SVG
    function exportAsSVG() {
      const svgElement = document.querySelector("svg")!;
      const svgData = new XMLSerializer().serializeToString(svgElement);
      const svgBlob = new Blob([svgData], { type: "image/svg+xml" });
      const downloadLink = document.createElement("a");
      downloadLink.href = URL.createObjectURL(svgBlob);
      downloadLink.download = "diagram.svg";
      downloadLink.click();
    }
  
    // Helper function to get the position of a task by id
    function getTaskPosition(taskId: string) : Position {
      const task = diagram?.bpnPositions?.find(t => t.id === taskId);
      return task && task.position ? task.position : { x: 0, y: 0 };
    }
  
    // Helper function to create SVG path data from waypoints
    function createPathData(waypoints: Array<{ x?: number; y?: number }>, from: { x?: number; y?: number }, to: { x?: number; y?: number }) {
      if (waypoints.length === 0) {
        return `M ${from.x},${from.y} L ${to.x},${to.y}`; // Straight line if no waypoints
      }
  
      // Start from the first task position
      let path = `M ${from.x},${from.y} `;
  
      // Iterate over waypoints to create lines
      waypoints.forEach(wp => {
        path += `L ${wp.x},${wp.y} `;
      });
  
      // End at the last task position
      path += `L ${to.x},${to.y}`;
      return path;
    }
  
    // Helper function to calculate the intersection point on the task box
    function calculateIntersection(from: Position, to: Position) {
        const taskWidth = 300;
        const taskHeight = 150;
        const offset = 5; // Offset for arrow to start/end outside the boxes

        // Get center positions
        const fromCenter = { x: (from.x ?? 0) + taskWidth / 2, y: (from.y ?? 0) + taskHeight / 2 };
        const toCenter = { x: (to.x ?? 0) + taskWidth / 2, y: (to.y ?? 0) + taskHeight / 2 };

        // Calculate deltas
        const deltaX = toCenter.x - fromCenter.x;
        const deltaY = toCenter.y - fromCenter.y;

        // Normalize the vector
        const length = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
        const unitX = deltaX / length;
        const unitY = deltaY / length;

        // Calculate intersection for from-task box
        let fromEdgeX = fromCenter.x;
        let fromEdgeY = fromCenter.y;

        // Determine the edge intersections
        if (Math.abs(deltaX) > Math.abs(deltaY)) { // More horizontal
            fromEdgeX = unitX > 0 ? (from.x ?? 0) + taskWidth + offset : (from.x ?? 0) - offset;
        } else { // More vertical
            fromEdgeY = unitY > 0 ? (from.y ?? 0) + taskHeight + offset : (from.y ?? 0) - offset;
        }

        // Calculate intersection for to-task box
        let toEdgeX = toCenter.x;
        let toEdgeY = toCenter.y;

        // Find intersection on the edges of the "to" box
        if (Math.abs(deltaX) > Math.abs(deltaY)) { // More horizontal
            toEdgeX = unitX > 0 ? (to.x ?? 0) - (offset * 3)  : (to.x ?? 0) + taskWidth + offset;
        } else { // More vertical
            toEdgeY = unitY > 0 ? (to.y ?? 0) - (offset * 3) : (to.y ?? 0) + taskHeight + offset;
        }
        return { fromEdge: { x: fromEdgeX, y: fromEdgeY }, toEdge: { x: toEdgeX, y: toEdgeY } };
    }

    // Prepare paths for SVG rendering
    async function preparePaths() {
    paths = []; // Reset paths
    for (const transition of diagram?.bpnConnectionWaypoints ?? []) {
        if (transition.fromBPN && transition.toBPN) {
        const fromPosition = getTaskPosition(transition.fromBPN);
        const toPosition = getTaskPosition(transition.toBPN);

        // Calculate the intersection points
        const { fromEdge, toEdge } = calculateIntersection(fromPosition, toPosition);

        const pathData = createPathData(transition.waypoints ?? [], fromEdge, toEdge);

        // Create a unique key based on fromBPN and toBPN
        const uniqueKey = `${transition.fromBPN}-${transition.toBPN}`;

        paths.push({ d: pathData, stroke: "black", key: uniqueKey });
        }
    }
    }
  
    // Run preparePaths when the component mounts
    onMount(async () => {
      await preparePaths();
    });
  </script>
  
  <svg width="{width}" height="{height}" xmlns="http://www.w3.org/2000/svg">
    <!-- Define the arrow marker -->
    <defs>
      <marker id="arrow" markerWidth="10" markerHeight="7" refX="0" refY="3.5" orient="auto">
        <polygon points="0 0, 10 3.5, 0 7" fill="black" />
      </marker>
    </defs>
  
    {#each paths as { d, stroke, key }}
      <path d={d} stroke={stroke} fill="transparent" marker-end="url(#arrow)" key={key} /> <!-- Add marker for arrows -->
    {/each}
  
    {#each tasks as task}
      <Task
        id={task.id ?? ""}
        name={task.name ?? ""}
        businessPurpose={task.businessPurpose ?? ""}
        position={getTaskPosition(task.id ?? "")}
        readonly={readonly}
        on:dragmove={handleTaskDrag}
      />
    {/each}
  </svg>
  
  <button on:click={exportAsSVG}>Download as SVG</button>
  
  <style>
    svg {
      border: 1px solid black;
    }
  </style>
  