<script lang="ts">
	import Layout from '@/+layout.svelte';
	import CreateSystemDialog from "./createSystemDialog.svelte";
	import CreateContainerDialog from './createContainerDialog.svelte';
	import CreateDraftFeatureDialog from './createDraftFeatureDialog.svelte';
	import { onMount, onDestroy } from 'svelte';
	import { writable } from 'svelte/store';
	import {  SystemApi, ContainerApi/*, ServerApi, DraftFeatureApi*/, SolutionApi } from 'BpnEngineClient/apis';
	import type {BpnSystem,/* BpnWebApiContainer*/} from 'BpnEngineClient/models';
	import { slide } from 'svelte/transition';
	import { goto } from '$app/navigation';
	import { formatDate	} from 'lib/Duration'
	import {
		joinGroupView,
		leaveGroupView,
		onGroupUpdate
	} from 'signalRService';

	let systemId : string = "";
	let solutionId : string = "";
	let systems : BpnSystem[] = [];
	let expandedContainerRow: number | null = null; // Track which row is expanded
	const containerApi = new ContainerApi();
	const solutionApi = new SolutionApi();
	const systemApi = new SystemApi();
	const currentTime = writable(new Date());

	let intervalId: any;
	onMount(async () => {
		let solutions = await solutionApi.getAllSolutions();
		solutionId = solutions[0].id;//TODO: select from dropdown?
		intervalId = setInterval(() => { currentTime.set(new Date()); }, 1000);


		onGroupUpdate(callback);
		await joinGroupView("bpnsystem");
		await fetchDetails();
	});
	onDestroy(() => {
		clearInterval(intervalId);
		leaveGroupView("bpnsystem");
	});

	async function  fetchDetails()
	{
		systems = await systemApi.getAllSystems();
		systemId = systems[0].id;
	}
	const callback = (name: string, id: string, message: string) => {
		console.log(id);
		console.log(name);
		console.log(message);
		fetchDetails();
	};

	async function loadContainers(systemId : string) {
		if (systemId === "") return [];
		let webApiContainer = await containerApi.getAllContainersBySystem({systemId:systemId});
		webApiContainer = webApiContainer.map(container => ({
			...container, // Spread the original container to keep other properties intact
			features: container.features.map(feature => ({
				...feature, // Spread the original feature to keep other properties intact
				revisions: feature.revisions.filter(revision => revision.revision === -1) // Filter out revisions where revision !== -1
			}))
		}));
		return webApiContainer;
	}
	function toggleContainerRow(index: number) {
		expandedContainerRow = expandedContainerRow === index ? null : index;
	}

</script>

<Layout>
<div class="tabs">
	<ul>
		{#each systems as system}
			<li>
				<div on:click={() => {systemId = system.id}}>{system.name}</div>
			</li>
		{/each}
		<li>
			<CreateSystemDialog solutionId={solutionId} ></CreateSystemDialog>
		</li>
	</ul>
</div>
{#await loadContainers(systemId) then containers}
<div style="display: flex; align-items: center; gap: 15px">
	<h1>Containers</h1>
	<CreateContainerDialog systemId={systemId} />
</div>

<table>
	<thead>
		<tr>
			<th>Container</th>
			<th>Last updated</th>
			<th>Created</th>
		</tr>
	</thead>
	<tbody>
		{#each containers as container, index}
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
								<th>Component</th>
								<th>Last update</th>
							</tr>
						</thead>
						<tbody>
							{#if container && container.features}
								{#each container.features as feature}
									{#each feature.revisions as version}
											<tr>
												<td class="title-column">
													<button title={"Edit " + version.name} aria-label={"Edit " + version.name}
															on:click={() => goto(`/draftfeature/edit/${container.id}/${feature.id}`)}
															style="border: none; background: none; cursor: pointer; user-select: none;"
													>
														<span class="actionRow">
															<i class="fas fa-edit"></i>
															{version.name} <span class="technology">[C#]</span>
														</span>
													</button>
												</td>

												<td class="tooltip date-column" data-tooltip={version.stats.lastUsed}>
													{$currentTime
														? formatDate(version.stats.lastUsed, $currentTime)
														: '-'}
												</td>
											</tr>
									{/each}
								{/each}
							{/if}
							<tr>
								<td colspan="2">
									<CreateDraftFeatureDialog bpnContextId={container.id} />
								</td>
							</tr>
						</tbody>
					</table>
				</div>
			</td>
		</tr>
		{/if}
		{/each}
	</tbody>
</table>
{/await}
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


	/* Left Sidebar Menu */
	.tabs {
		user-select: none;
	}

	.tabs ul {
		display: flex;
		list-style-type: none;
		padding: 0;
	}

	.tabs ul li {
		margin: 0;
	}

	.tabs ul li div {
		display: flex;
		align-items: center;
		justify-content: center;
		min-width: 50px;
		max-width: 250px;
		padding: 0 15px;
		margin:  0 1px;
		width: auto;
		height: 50px;
		background-color: #3c3c3c; /* Slightly lighter for hover contrast */
		text-align: center;
		font-size: 18px;
		color: #e0e0e0;
		transition: background-color 0.3s, color 0.3s;
		cursor: pointer;
		border-top-left-radius: 15px;
		border-top-right-radius: 15px;
	}

	.tabs ul li div:hover {
		background-color: #575757; /* Hover effect */
		color: #fff;
	}
</style>
