<script lang="ts">
	//import type { PageData } from './$types';
	import Layout from '../../components/Layout.svelte';
	import { onMount, onDestroy } from 'svelte';
	import { writable } from 'svelte/store';
	import { ContextApi, ServerApi } from '../../BpnEngineClient/apis'; // Adjust the path accordingly
	import type {
		BpnContext,
		DurationClassification,
		FeatureVersion
	} from '../../BpnEngineClient/models'; // Adjust the path accordingly
	import { slide } from 'svelte/transition';
	import { goto } from '$app/navigation';
	import {formatDurationShort,formatDurationLong, formatDate	} from '../../lib/Duration' 
	import FeatureDuration from '../../lib/FeatureDuration.svelte' 

	let contexts: BpnContext[] = [];
	let durationClasses: DurationClassification[] = [];
	let expandedContextRow: number | null = null; // Track which row is expanded
	let expandedFeatureRow: number | null = null; // Track which row is expanded

	const contextApi = new ContextApi();
	const serverApi = new ServerApi();
	const currentTime = writable(new Date());

	// Fetch data on component mount
	let intervalId: number | undefined;
	onMount(async () => {
		durationClasses = await serverApi.getDurationClassification();
		contexts = await contextApi.getAllContexts();

		intervalId = setInterval(() => {
			currentTime.set(new Date());
		}, 1000);
	});
	onDestroy(() => {
		clearInterval(intervalId); // Clear interval on component destroy
	});
	// Function to toggle the expanded row
	function toggleContextRow(index: number) {
		expandedContextRow = expandedContextRow === index ? null : index;
	}
	function toggleFeatureRow(index: number) {
		expandedFeatureRow = expandedFeatureRow === index ? null : index;
	}


	// Calculate summaries for each context
	function getFeatureSummaries(context: BpnContext) {
		let totalInvocations = 0;
		let totalErrors = 0;
		let totalCompleted = 0;
		let totalInProgress = 0;
		let maxDuration = 0;
		let minDuration = Infinity;
		let avgOfAverages = 0;
		let totalAvgDurations = 0;
		let avgCount = 0;
		let lastUsed: Date | undefined | null = undefined;

		context.features?.forEach((feature) => {
			if (!feature.versions) return;
			feature.versions.forEach((version) => {
				if (!version.stats) return;

				totalInvocations += version.stats.invocationCount ?? 0;
				totalErrors += version.stats.invocationErrorCount ?? 0;
				totalCompleted += version.stats.invocationCompletedCount ?? 0;
				totalInProgress += version.stats.invocationsInProgressCount ?? 0;

				// Find max duration
				if (version.stats.maxDurationMs && version.stats.maxDurationMs > maxDuration) {
					maxDuration = version.stats.maxDurationMs;
				}

				// Find min duration
				if (version.stats.minDurationMs && version.stats.minDurationMs < minDuration) {
					minDuration = version.stats.minDurationMs;
				}

				// Calculate avg of averages
				if (version.stats.avgDurationMs) {
					totalAvgDurations += version.stats.avgDurationMs;
					avgCount++;
				}

				if (!lastUsed || (version.stats.lastUsed && version.stats.lastUsed > lastUsed)) {
					lastUsed = version.stats.lastUsed;
				}
			});
		});

		avgOfAverages = totalAvgDurations / avgCount || 0;

		return {
			totalInvocations,
			totalErrors,
			totalCompleted,
			totalInProgress,
			maxDuration,
			minDuration: minDuration === Infinity ? 0 : minDuration,
			avgOfAverages,
			lastUsed
		};
	}

	function editVersion(contextId: string, featureId: string, versionId: string) {
    console.log(versionId);
    if (versionId === "-1")
		  goto(`/DraftFeature/Edit/${contextId}/${featureId}`);
		else 
      goto(`/Feature/Edit/${contextId}/${featureId}/${versionId}`);
	}

	function getHighestVersion(versions: FeatureVersion[] | undefined): FeatureVersion | undefined {
		if (!versions || versions.length === 0) return undefined;
		return versions.reduce((prev, current) => {
			const prevVersion = prev.version || 0;
			const currentVersion = current.version || 0;
			return prevVersion > currentVersion ? prev : current;
		}, versions[0]);
	}
	let isLoggedIn = false; // Replace this with your actual login state logic
</script>

<Layout {isLoggedIn}>
		<h1>Contexts Table</h1>
		<table>
			<thead>
				<tr>
					<th>Context</th>
					<th>Invocations</th>
					<th>Errors</th>
					<th>Completed</th>
					<th>Processing</th>
					<th>Max duration</th>
					<th>Avg duration</th>
					<th>Min duration</th>
					<th>Last used</th>

					<th>Last updated</th>
					<th>Created</th>
				</tr>
			</thead>
			<tbody>
				{#each contexts as context, index}
					<tr>
						<td class="title-column">
							<button
								on:click={() => toggleContextRow(index)}
								style="border: none; background: none; cursor: pointer;"
							>
								<i
									class={`fas ${expandedContextRow === index ? 'fa-chevron-up' : 'fa-chevron-down'} light-icon`}
								></i>
							</button>
							{context.name}
						</td>
						{#await Promise.resolve(getFeatureSummaries(context)) then summary}
							<td class="number-column">{summary.totalInvocations}</td>
							<td class="number-column">{summary.totalErrors}</td>
							<td class="number-column">{summary.totalCompleted}</td>
							<td class="number-column">{summary.totalInProgress}</td>
							<td class="number-column">
								<FeatureDuration duration={summary.maxDuration}   durationClasses={durationClasses}/>
							</td>
							<td class="number-column">
								<FeatureDuration duration={summary.avgOfAverages}   durationClasses={durationClasses}/>
							</td>
							<td class="number-column">
								<FeatureDuration duration={summary.minDuration}  durationClasses={durationClasses} />
							</td>
							<td class="tooltip date-column" data-tooltip={summary.lastUsed}>
								{$currentTime ? formatDate(summary.lastUsed, $currentTime) : '-'}
							</td>
						{/await}
						<td class="tooltip date-column" data-tooltip={context.lastUpdatedTimestamp}>
							{$currentTime ? formatDate(context.lastUpdatedTimestamp, $currentTime) : '-'}
						</td>
						<td class="tooltip" data-tooltip={context.createdTimestamp}>
							{$currentTime ? formatDate(context.createdTimestamp, $currentTime) : '-'}
						</td>
					</tr>
					{#if expandedContextRow === index}
						<tr class="expandable-row">
							<td colspan="11" style="padding:25px 50px; border:0;">
								<div transition:slide>
									<table>
										<thead>
											<tr>
												<th>Feature</th>
												<th>Newest version</th>
												<th>Invocations</th>
												<th>Errors</th>
												<th>Completed</th>
												<th>Processing</th>
												<th>Max duration</th>
												<th>Avg duration</th>
												<th>Min duration</th>
												<th>Last Used</th>
											</tr>
										</thead>
										<tbody>
											{#if context && context.features}
												{#each context.features as feature, featureindex}
													{#await getHighestVersion(feature.versions) then highestVersionFeature}
														{#if highestVersionFeature}
															{#if highestVersionFeature.stats}
																<tr>
																	<td class="title-column">
																		<button
																			on:click={() => toggleFeatureRow(featureindex)}
																			style="border: none; background: none; cursor: pointer; user-select: none;"
																		>
																			<i
																				class={`fas ${expandedFeatureRow === featureindex ? 'fa-chevron-up' : 'fa-chevron-down'} light-icon`}
																			></i>
																		</button>
																		{highestVersionFeature.name}
																	</td>
																	<td class="version-column">
																		{highestVersionFeature.version == -1
																		? 'draft'
																		: 'v' + highestVersionFeature.version}
																	</td>
																	<td class="number-column"
																		>{highestVersionFeature.stats.invocationCount}</td
																	>
																	<td class="number-column"
																		>{highestVersionFeature.stats.invocationErrorCount}</td
																	>
																	<td class="number-column"
																		>{highestVersionFeature.stats.invocationCompletedCount}</td
																	>
																	<td class="number-column"
																		>{highestVersionFeature.stats.invocationsInProgressCount}</td
																	>
																	<td class="number-column">
																		<FeatureDuration duration={highestVersionFeature.stats.maxDurationMs}  durationClasses={durationClasses}  />
																	</td>
																	<td class="number-column">
																		<FeatureDuration duration={highestVersionFeature.stats.avgDurationMs}   durationClasses={durationClasses} />
																	</td>
																	<td class="number-column">
																		<FeatureDuration duration={highestVersionFeature.stats.minDurationMs}   durationClasses={durationClasses} />
																	</td>
																	<td
																		class="tooltip date-column"
																		data-tooltip={highestVersionFeature.stats.lastUsed}
																	>
																		{$currentTime
																			? formatDate(
																					highestVersionFeature.stats.lastUsed,
																					$currentTime
																				)
																			: '-'}
																	</td>
																</tr>
															{/if}
														{/if}
													{/await}

													{#if expandedFeatureRow === featureindex}
														<tr class="expandable-row">
															<td colspan="11" style="padding:0px; border:0;">
																<div transition:slide>
																	<table>
																		<thead>
																			<tr>
																				<th>Name</th>
																				<th>Version</th>
																				<th>Invocations</th>
																				<th>Errors</th>
																				<th>Completed</th>
																				<th>Processing</th>
																				<th>Max duration</th>
																				<th>Avg duration</th>
																				<th>Min duration</th>

																				<th>Last Used</th>
																			</tr>
																		</thead>
																		<tbody>
																			{#if feature && feature.versions}
																				{#each feature.versions as version}
																					{#if version.stats}
																						<tr>
																							<td class="title-column">
																								{version.name}
																							</td>
																							<td class="version-column">
																								<button
																									on:click={() =>
																										editVersion(
																											context.id ?? '',
																											feature.id ?? '',
																											version.version?.toString() ?? ''
																										)}
																									style="border: none; background: none; cursor: pointer; user-select: none;"
																								>
																									<i class="fas fa-edit light-icon"></i>
																								</button>
																								{version.version == -1
																									? 'draft'
																									: 'v' + version.version}
																							</td>
																							<td class="number-column"
																								>{version.stats.invocationCount}</td
																							>
																							<td class="number-column"
																								>{version.stats.invocationErrorCount}</td
																							>
																							<td class="number-column"
																								>{version.stats.invocationCompletedCount}</td
																							>
																							<td class="number-column"
																								>{version.stats.invocationsInProgressCount}</td
																							>
																							<td class="number-column">
																								<FeatureDuration duration={version.stats.maxDurationMs}   durationClasses={durationClasses}/>
																							</td>
																							<td class="number-column">
																								<FeatureDuration duration={version.stats.avgDurationMs}   durationClasses={durationClasses}/>
																							</td>
																							<td class="number-column">
																								<FeatureDuration duration={version.stats.minDurationMs}  durationClasses={durationClasses} />
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
																					{/if}
																				{/each}
																			{/if}
																		</tbody>
																	</table>
																</div>
															</td>
														</tr>
													{/if}
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

	.expandable-row {
		/*background-color: #f9f9f9;*/
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
</style>
