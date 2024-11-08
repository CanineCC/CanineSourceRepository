<script lang="ts">
	import Layout from '@/+layout.svelte';
	import { onMount, onDestroy } from 'svelte';
	import { writable } from 'svelte/store';
	import {  SystemApi, ContainerApi, ServerApi } from 'BpnEngineClient/apis';
	import type { BpnWebApiContainer, DurationClassification, FeatureRevisions } from 'BpnEngineClient/models';
	import { slide } from 'svelte/transition';
	import { goto } from '$app/navigation';
	import { formatDate	} from 'lib/Duration' 
	import FeatureDuration from 'components/FeatureDuration.svelte' 

	let webApiContainer: BpnWebApiContainer[] = [];
	let durationClasses: DurationClassification[] = [];
	let expandedContainerRow: number | null = null; // Track which row is expanded
	let expandedFeatureRow: number | null = null; // Track which row is expanded
	const containerApi = new ContainerApi();
	const serverApi = new ServerApi();
	const systemApi = new SystemApi();
	const currentTime = writable(new Date());

	let intervalId: any;
	onMount(async () => {
		durationClasses = await serverApi.getDurationClassification();
		let systems = await systemApi.getAllSystems();
		//systems[0].
		webApiContainer = await containerApi.getAllContexts();

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
	function toggleFeatureRow(index: number) {
		expandedFeatureRow = expandedFeatureRow === index ? null : index;
	}

	function getFeatureSummaries(container: BpnWebApiContainer) {
		let totalInvocations = 0;
		let totalErrors = 0;
		let totalCompleted = 0;
		let totalInProgress = 0;
		let maxDuration = 0;
		let minDuration = Infinity;
		let avgOfAverages = 0;
		let totalAvgDurations = 0;
		let avgCount = 0;
		let lastUsed: Date | undefined | null = null;

		container.features?.forEach((feature) => {
			feature.revisions.forEach((version) => {
				totalInvocations += version.stats.invocationCount ?? 0;
				totalErrors += version.stats.invocationErrorCount ?? 0;
				totalCompleted += version.stats.invocationCompletedCount ?? 0;
				totalInProgress += version.stats.invocationsInProgressCount ?? 0;

				if (version.stats.maxDurationMs && version.stats.maxDurationMs > maxDuration) {
					maxDuration = version.stats.maxDurationMs;
				}

				if (version.stats.minDurationMs && version.stats.minDurationMs < minDuration) {
					minDuration = version.stats.minDurationMs;
				}

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

	function editVersion(containerId: string, featureId: string, versionId: string) {
    if (versionId === "-1")
		  goto(`/draftfeature/edit/${containerId}/${featureId}`);
		else 
      goto(`/feature/edit/${containerId}/${featureId}/${versionId}`);
	}

	function getHighestVersion(versions: FeatureRevisions[]): FeatureRevisions {
		return versions.reduce((prev, current) => {
			const prevVersion = prev.revision || 0;
			const currentVersion = current.revision || 0;
			return prevVersion > currentVersion ? prev : current;
		}, versions[0]);
	}
</script>

<Layout>
		<h1>Containers</h1>
		<table>
			<thead>
				<tr>
					<th>Container</th>
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
				{#each webApiContainer as container, index}
					<tr>
						<td class="title-column">
							<button title="accordion" aria-label="accordion"
								on:click={() => toggleContainerRow(index)}
								style="border: none; background: none; cursor: pointer;"
							>
								<span class="actionRow">
									<i class={`fas ${expandedContainerRow === index ? 'fa-chevron-up' : 'fa-chevron-down'} light-icon`}></i>
									{container.name} <span class="technology">[WepApi]</span>
								</span>
							</button>
						</td>
						{#await Promise.resolve(getFeatureSummaries(container)) then summary}
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
												<th>Newest version</th>
												<th>Invocations</th>
												<th>Errors</th>
												<th>Completed</th>
												<th>Processing</th>
												<th>Max duration</th>
												<th>Avg duration</th>
												<th>Min duration</th>
												<th>Last Used</th>
												<th>Released</th>
											</tr>
										</thead>
										<tbody>
											{#if container && container.features}
												{#each container.features as feature, featureindex}
													{#await getHighestVersion(feature.revisions) then highestVersionFeature}
														<tr>
															<td class="title-column">
																<button title="accordion" aria-label="accordion"
																	on:click={() => toggleFeatureRow(featureindex)}
																	style="border: none; background: none; cursor: pointer; user-select: none;"
																>
																	<span class="actionRow">
																		<i class={`fas ${expandedFeatureRow === featureindex ? 'fa-chevron-up' : 'fa-chevron-down'} light-icon`}></i>
																		{highestVersionFeature.name} <span class="technology">[C#]</span>
																	</span>
																</button>
															</td>
															<td class="version-column">
																{highestVersionFeature.revision == -1
																? 'draft'
																: 'v' + highestVersionFeature.revision}
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
															<td
																	class="tooltip date-column"
																	data-tooltip={highestVersionFeature.stats.published}
															>
																{highestVersionFeature.revision == -1
																		? '-'
																		: $currentTime
																		? formatDate(highestVersionFeature.stats.published, $currentTime)
																		: '-'}
															</td>
														</tr>
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
																				<th>Released</th>
																			</tr>
																		</thead>
																		<tbody>
																		{#each feature.revisions as version}
																				<tr>
																					<td class="title-column">
																						<button title="accordion" aria-label="accordion"
																								on:click={() =>
																								editVersion(
																									container.id ?? '',
																									feature.id ?? '',
																									version.revision?.toString() ?? ''
																								)}
																								style="border: none; background: none; cursor: pointer; user-select: none;"
																						>
																							<span class="actionRow">
																								<i class="fas fa-edit light-icon"></i>
																								{version.name} <span class="technology">[C#]</span>
																							</span>
																						</button>
																					</td>
																					<td class="version-column">
																						{version.revision == -1
																							? 'draft'
																							: 'v' + version.revision}
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
																					<td
																							class="tooltip date-column"
																							data-tooltip={version.stats.published}
																					>
																						{version.revision == -1
																								? '-'
																								: $currentTime
																										? formatDate(version.stats.published, $currentTime)
																										: '-'}
																					</td>
																				</tr>
																		{/each}
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
