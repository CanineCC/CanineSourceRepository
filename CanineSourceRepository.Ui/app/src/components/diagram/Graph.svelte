<script lang="ts">
    import Task from "./Task.svelte";
    import { createEventDispatcher } from "svelte";
    import { onMount } from "svelte";
    import type { BpnTask, BpnFeatureDiagram, Position, TaskStats, PositionUpdatedOnDraftFeatureBody } from "BpnEngineClient/models";
    import { DraftFeatureDiagramApi  } from 'BpnEngineClient/apis'; // Adjust the path accordingly

    const draftFeatureDiagramApi = new DraftFeatureDiagramApi();

    export let featureId: string;
    export let tasks: Array<BpnTask> = [];
//    export let transitions: Array<BpnTransition> = [];
    export let diagram: BpnFeatureDiagram | undefined;
    export let readonly: boolean = false;
    export let showDownload: boolean = true;

    export let taskStats: Array<TaskStats> = [];

    let width: number = 800;
    let height: number = 600;
    let paths: Array<{ d: string; stroke: string; key: string }> = []; // Include a unique key for each path
    let taskPositions: PositionUpdatedOnDraftFeatureBody[] = [];

    const dispatch = createEventDispatcher(); // Create event dispatcher
    async function handleTaskStopDrag(event :any)
    {
      if (readonly) return;
      const { id, position } = event.detail;
      calcSize();

      // Find the index of the object with the same taskId
      const index = taskPositions.findIndex(task => task.taskId === id);

      if (index !== -1) {
          // If taskId exists in the array, update the position
          taskPositions[index].position = position;
      } else {
          // If taskId doesn't exist, add a new object to the array
          taskPositions.push({ featureId, taskId: id, position });
      }

      dispatch('taskPositionChange', { taskId: id, position });
    }

    async function saveFeaturePositions() {
        if (readonly) return;
        await draftFeatureDiagramApi.positionsUpdatedOnDraftFeature({ positionUpdatedOnDraftFeatureBody : taskPositions});
    }

    async function handleTaskDrag(event: any) {
      if (readonly) return;
      const { id, position } = event.detail;
      const task = diagram?.bpnPositions?.find(t => t.id === id);
      if (task) {
        task.position = position; // Update position of the task
        await preparePaths(); // Recalculate paths after moving a task

      } else {
        calcSize();
      } 
    }
  
    function handleTaskSelect(event: any) {
      const { id } = event.detail;
      dispatch('taskSelect', { taskId: id }); // Dispatch a new custom event
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
    function getTaskStats(taskId: string) : TaskStats | undefined {
      const task = taskStats?.find(t => t.task === taskId);
      console.log("Finding stats for: " + taskId);
      console.log(task);
      return task;
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

          paths.push({ d: pathData, stroke: "grey", key: uniqueKey });
          }
      }
    }
    function calcSize()
    {
      var arr = diagram?.bpnPositions ?? [{ id:"", position:{x:0, y:0}}];
      const maxXObj = arr.reduce((max, current) => {
          const currentX = current.position?.x;
          const maxX = max.position?.x;

          // Only compare if both x values are defined, otherwise skip
          if (currentX === undefined) {
            return max; // Skip current if x is undefined
          }

          if (maxX === undefined || currentX > maxX) {
            return current; // Update max if current has a larger or first defined x
          }

          return max; // Otherwise, keep the current max
        }, arr[0]);
        const maxYObj = arr.reduce((max, current) => {
          const currentY = current.position?.y;
          const maxY = max.position?.y;

          // Only compare if both x values are defined, otherwise skip
          if (currentY === undefined) {
            return max; // Skip current if x is undefined
          }

          if (maxY === undefined || currentY > maxY) {
            return current; // Update max if current has a larger or first defined x
          }

          return max; // Otherwise, keep the current max
        }, arr[0]);


      width =readonly ? maxXObj.position.x +325 : Math.max(1000, (maxXObj.position.x) + 325);
      height =readonly ? maxYObj.position.y +175 : Math.max(500, (maxYObj.position.y) + 175);
    }
  
    // Run preparePaths when the component mounts
    onMount(async () => {
      await preparePaths();
      calcSize();
    });
  </script>
  
<div style="position:relative">
    <svg style="width:{width}px; height:{height}px" xmlns="http://www.w3.org/2000/svg">
      <!-- Define the arrow marker -->
      <defs>
        <marker id="arrow" markerWidth="10" markerHeight="7" refX="0" refY="3.5" orient="auto">
          <polygon points="0 0, 10 3.5, 0 7" fill="grey" />
        </marker>
      </defs>
    
      {#each paths as { d, stroke }}
        <path d={d} stroke={stroke} fill="transparent" marker-end="url(#arrow)" /> <!-- Add marker for arrows -->
      {/each}
    
      {#each tasks as task}
        <Task
          id={task.id ?? ""}
          name={task.name ?? ""}
          businessPurpose={task.businessPurpose ?? ""}
          position={getTaskPosition(task.id ?? "")}
          readonly={readonly}
          stats={getTaskStats(task.id ?? "")}
          service={task.namedConfiguration !== "" ? task.namedConfiguration + " (" + task.serviceDependency + ")" : ""}
          on:dragmove={handleTaskDrag}
          on:dragstopped={handleTaskStopDrag}
          on:taskSelect={handleTaskSelect}
        />
      {/each}
    </svg>

    {#if showDownload}
      <a href="#top" title="Download svg" class="download button" on:click={exportAsSVG}><i class="fas fa-download "></i></a>
    {/if}
    {#if readonly===false}
      <a href="#top" title="Save diagram" class="save button" on:click={saveFeaturePositions}><i class="fas fa-save "></i></a>
    {/if}
  </div>    
  <style>
    .download {
      position: absolute;
      top:10px;
      right:10px;
    }
    .save {
      position: absolute;
      top:10px;
      right:70px;
    }
  
  </style>
  