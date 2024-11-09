<script lang="ts">
	import Layout from '@/+layout.svelte';
	import { onMount, onDestroy } from 'svelte';
	import { writable } from 'svelte/store';
	import {  SystemApi, ContainerApi, ServerApi } from 'BpnEngineClient/apis';
	import type { BpnWebApiContainer } from 'BpnEngineClient/models';
	import { slide } from 'svelte/transition';
	import { goto } from '$app/navigation';
	import { formatDate	} from 'lib/Duration' 

	let webApiContainer: BpnWebApiContainer[] = [];
	let expandedContainerRow: number | null = null; // Track which row is expanded
	let expandedFeatureRow: number | null = null; // Track which row is expanded
	const containerApi = new ContainerApi();
	const serverApi = new ServerApi();
	const systemApi = new SystemApi();
	const currentTime = writable(new Date());

	let intervalId: any;
	onMount(async () => {
		let systems = await systemApi.getAllSystems();
		webApiContainer = await containerApi.getAllContainers();//rename: getAllContainers - also getContainersBySystem

		webApiContainer = webApiContainer.map(container => ({
			...container, // Spread the original container to keep other properties intact
			features: container.features.map(feature => ({
				...feature, // Spread the original feature to keep other properties intact
				revisions: feature.revisions.filter(revision => revision.revision === -1) // Filter out revisions where revision !== -1
			}))
		}));

		intervalId = setInterval(() => {
			currentTime.set(new Date());
		}, 1000);
	});
	onDestroy(() => {
		clearInterval(intervalId); 
	});

	function toggleContainerRow(index: number) {
		expandedContainerRow = expandedContainerRow === index ? null : index;
	}



	function editVersion(containerId: string, featureId: string) {
    	  goto(`/draftfeature/edit/${containerId}/${featureId}`);
	}

</script>

<Layout>
	<h1>Containers</h1>
	<table>
		<thead>
			<tr>
				<th>Container</th>
				<th>Last updated</th>
				<th>Created</th>
			</tr>
		</thead>
		<tbody>
			{#each webApiContainer as container, index}
				<tr>
					<td class="title-column">
						<button title="Container" aria-label="Container"
							on:click={() => toggleContainerRow(index)}
							style="border: none; background: none; cursor: pointer;"
						>
							<span class="actionRow">
								<i class={`fas ${expandedContainerRow === index ? 'fa-chevron-up' : 'fa-chevron-down'} light-icon`}></i>
								{container.name} <span class="technology">[WepApi]</span>
							</span>
						</button>
					</td>
					<td class="tooltip date-column" data-tooltip={container.lastUpdatedTimestamp}>
						{$currentTime ? formatDate(container.lastUpdatedTimestamp, $currentTime) : '-'}
					</td>
					<td class="tooltip" data-tooltip={container.createdTimestamp}>
						{$currentTime ? formatDate(container.createdTimestamp, $currentTime) : '-'}
					</td>
				</tr>
				{#if expandedContainerRow === index}
					<tr class="expandable-row">
						<td colspan="11" style="padding:25px 50px; border:0;">
							<div transition:slide>
								<table>
									<thead>
										<tr>
											<th>Feature</th>
											<th>Last Used</th>
										</tr>
									</thead>
									<tbody>
										{#if container && container.features}
											{#each container.features as feature, featureindex}
												{#each feature.revisions as version}
														<tr>
															<td class="title-column">
																<button title="edit" aria-label="edit"
																		on:click={() =>
																		editVersion(
																			container.id ?? '',
																			feature.id ?? '',
																		)}
																		style="border: none; background: none; cursor: pointer; user-select: none;"
																>
																	<span class="actionRow">
																		<i class="fas fa-edit"></i>
																		{version.name} <span class="technology">[C#]</span>
																	</span>
																</button>
															</td>

															<td
																class="tooltip date-column"
																data-tooltip={version.stats.lastUsed}
															>
																{$currentTime
																	? formatDate(version.stats.lastUsed, $currentTime)
																	: '-'}
															</td>
														</tr>
												{/each}
												<p style="color: red">TODO: ADD FEATURE</p>
												<p style="color: red">TODO: Release draft = approved draft for release, then release drafts here, so all features in a container have the same revision (show pre-release changelog and color code drafts to indicate updates)</p>
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
</Layout>

<style>
	.light-icon {
		color: #fff; /* or any light color */
	}
	table {
		width: 100%;
		border-collapse: collapse;
	}

	th,
	td {
		padding: 8px 12px;
		border: 1px solid #ddd;
	}
	.hidden {
		display: none;
	}
	.version-column {
		text-align: left;
		max-width: 100px;
		width: 100px;
		min-width: 100px;
		box-sizing: border-box;
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
		width: 380px;
		min-width: 380px;
		box-sizing: border-box;
	}
	.date-column {
		max-width: 200px;
		width: 200px;
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
		width: auto;
		white-space:  pre; /* Interpret \n as a line break */
		text-align: center;
		z-index: 10;
	}

	.expanded-content {
		overflow: hidden; /* Prevents content from overflowing */
		transition: max-height 3.3s ease; /* Smooth transition */
		height: 0; /* Start collapsed */
	}
	.technology {
		color: #555;
	}
	.actionRow {
		color: #e0e0e0;
		font-family: sans-serif;
		font-size: 12pt;
	}
</style>
