<script lang="ts">
    export let title: string;
    export let isOpen: boolean = false;
  
    function toggleAccordion() {
      isOpen = !isOpen;
    }
  </script>
  
  <style>
    .accordion {
      border: 1px solid #3c3c3c;
      border-radius: 4px;
      margin-bottom: 1em;
      width: 100%;
    }
  
    .accordion-header {
      cursor: pointer;
      padding: 1em;
      background-color: #3c3c3c;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
  
    .accordion-body {
      padding: 1em;
      transition: transform 0.2s ease-in-out;
    }
  
    .arrow {
        user-select: none;
        transition: transform 0.2s ease-in-out;
    }
  </style>
  
  <div class="accordion">
    <!-- svelte-ignore a11y-no-noninteractive-element-interactions -->
    <div class="accordion-header" on:click={toggleAccordion} on:keydown={toggleAccordion} role="group">
      <span>{title}</span>
      <span class="arrow" style="transform:rotate({isOpen ? 90 : 0}deg)">▶</span>
    </div>
    <div class="accordion-body" style="display:{isOpen ? 'block' : 'none'}">
      <slot></slot>
    </div>
  </div>
  