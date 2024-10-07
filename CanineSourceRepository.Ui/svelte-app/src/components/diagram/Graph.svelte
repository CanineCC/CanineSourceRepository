<script lang="ts">
    import Task from "./Task.svelte";
    import type { BpnTask, BpnTransition, BpnFeatureDiagram, BpnPosition} from "../../BpnEngineClient/models"

    export let tasks : Array<BpnTask> = [];
    export let transitions : Array<BpnTransition> = [];
    export let diagram : BpnFeatureDiagram | undefined;
  
    
    // Function to handle drag and drop
    function handleTaskDrag(event:any) {
      const { id, newPosition } = event.detail;
      const task = diagram?.bpnPositions?.find(t => t.id === id);
      if (task) {
        task.position = newPosition;
      }
    }
  
    // Function to export the SVG
    function exportAsSVG() {
      const svgElement = document.querySelector("svg")!;
      const svgData = new XMLSerializer().serializeToString(svgElement);
      const svgBlob = new Blob([svgData], { type: "image/svg+xml;charset=utf-8" });
      const downloadLink = document.createElement("a");
      downloadLink.href = URL.createObjectURL(svgBlob);
      downloadLink.download = "diagram.svg";
      downloadLink.click();
    }
  </script>
  
  <svg width="800" height="600" xmlns="http://www.w3.org/2000/svg">
    {#each diagram?.bpnConnectionWaypoints??[] as transition}
      <!--<path
        d="M {transition.waypoints?[0].x} {transition.waypoints?[0].y}
           L {transition.waypoints?[1].x} {transition.waypoints?[1].y}"
        stroke="black" fill="transparent" />-->
    {/each}
  
    {#each tasks as task}
      <Task
        id={task.id??""}
        name={task.name??""}
        businessPurpose={task.businessPurpose??""}
        position={diagram?.bpnPositions?.find(t => t.id === task.id)?.position ?? { x:0, y:0}}
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
  