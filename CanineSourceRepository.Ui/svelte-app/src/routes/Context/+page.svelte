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

	// Function to format date strings
	function formatDate(date: Date | undefined | null, now: Date): string {
		if (!date) return '';
		const diff = now.getTime() - date.getTime();
		const seconds = Math.floor(diff / 1000);
		const minutes = Math.floor(seconds / 60);
		const hours = Math.floor(minutes / 60);
		const days = Math.floor(hours / 24);
		const years = Math.floor(days / 365);

		if (seconds < 60) return `${seconds} seconds ago`;
		if (minutes < 60) return `${minutes} minutes ago`;
		if (hours < 24) return `${hours} hours ago`;
		if (days < 365) return `${days} days ago`;
		return `${years} years ago`;
	}
	function getDurationColor(duration: number | undefined): string {
		if (!duration) return '#000';
		const classification = durationClasses.find(
			(dc) => duration >= dc.fromMs! && duration <= dc.toMs!
		);
		return classification && classification.hexColor ? classification.hexColor : '#000'; // Default to black if no classification found
	}
	function getDurationText(duration: number | undefined): string {
		if (!duration) return '';
		const classification = durationClasses.find(
			(dc) => duration >= dc.fromMs! && duration <= dc.toMs!
		);
		return classification && classification.category ? classification.category : '---';
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
	//export let data: PageData;
	let isLoggedIn = false; // Replace this with your actual login state logic
</script>

<Layout {isLoggedIn}>
	<h1>Settings</h1>

	<div>
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
									class={`fas ${expandedContextRow === index ? 'fa-chevron-up' : 'fa-chevron-down'}`}
								></i>
							</button>
							{context.name}
						</td>
						{#await Promise.resolve(getFeatureSummaries(context)) then summary}
							<td class="number-column">{summary.totalInvocations}</td>
							<td class="number-column">{summary.totalErrors}</td>
							<td class="number-column">{summary.totalCompleted}</td>
							<td class="number-column">{summary.totalInProgress}</td>
							<td
								class="tooltip number-column"
								data-tooltip={getDurationText(summary.maxDuration)}
								style="color: {getDurationColor(summary.maxDuration)};"
							>
								{summary.maxDuration ? Math.round(summary.maxDuration) + ' ms' : '-'}
							</td>
							<td
								class="tooltip number-column"
								data-tooltip={getDurationText(summary.avgOfAverages)}
								style="color: {getDurationColor(summary.avgOfAverages)};"
							>
								{summary.avgOfAverages ? Math.round(summary.avgOfAverages) + ' ms' : '-'}
							</td>
							<td
								class="tooltip number-column"
								data-tooltip={getDurationText(summary.minDuration)}
								style="color: {getDurationColor(summary.minDuration)};"
							>
								{summary.minDuration ? Math.round(summary.minDuration) + ' ms' : '-'}
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
																			style="border: none; background: none; cursor: pointer;"
																		>
																			<i
																				class={`fas ${expandedFeatureRow === featureindex ? 'fa-chevron-up' : 'fa-chevron-down'}`}
																			></i>
																		</button>
																		{highestVersionFeature.name}
																	</td>
																	<td class="version-column">
																		(v{highestVersionFeature.version})
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
																	<td
																		class="tooltip number-column"
																		data-tooltip={getDurationText(
																			highestVersionFeature.stats.maxDurationMs
																		)}
																		style="color: {getDurationColor(
																			highestVersionFeature.stats.maxDurationMs
																		)};"
																	>
																		{highestVersionFeature.stats.maxDurationMs
																			? Math.round(highestVersionFeature.stats.maxDurationMs) +
																				' ms'
																			: '-'}
																	</td>
																	<td
																		class="tooltip number-column"
																		data-tooltip={getDurationText(
																			highestVersionFeature.stats.avgDurationMs
																		)}
																		style="color: {getDurationColor(
																			highestVersionFeature.stats.avgDurationMs
																		)};"
																	>
																		{highestVersionFeature.stats.avgDurationMs
																			? Math.round(highestVersionFeature.stats.avgDurationMs) +
																				' ms'
																			: '-'}
																	</td>
																	<td
																		class="tooltip number-column"
																		data-tooltip={getDurationText(
																			highestVersionFeature.stats.minDurationMs
																		)}
																		style="color: {getDurationColor(
																			highestVersionFeature.stats.minDurationMs
																		)};"
																	>
																		{highestVersionFeature.stats.minDurationMs
																			? Math.round(highestVersionFeature.stats.minDurationMs) +
																				' ms'
																			: '-'}
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
																									style="border: none; background: none; cursor: pointer;"
																								>
																									<i class="fas fa-edit"></i>
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
																							<td
																								class="tooltip number-column"
																								data-tooltip={getDurationText(
																									version.stats.maxDurationMs
																								)}
																								style="color: {getDurationColor(
																									version.stats.maxDurationMs
																								)};"
																							>
																								{version.stats.maxDurationMs
																									? Math.round(version.stats.maxDurationMs) + ' ms'
																									: '-'}
																							</td>
																							<td
																								class="tooltip number-column"
																								data-tooltip={getDurationText(
																									version.stats.avgDurationMs
																								)}
																								style="color: {getDurationColor(
																									version.stats.avgDurationMs
																								)};"
																							>
																								{version.stats.avgDurationMs
																									? Math.round(version.stats.avgDurationMs) + ' ms'
																									: '-'}
																							</td>
																							<td
																								class="tooltip number-column"
																								data-tooltip={getDurationText(
																									version.stats.minDurationMs
																								)}
																								style="color: {getDurationColor(
																									version.stats.minDurationMs
																								)};"
																							>
																								{version.stats.minDurationMs
																									? Math.round(version.stats.minDurationMs) + ' ms'
																									: '-'}
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
	</div>
</Layout>

<style>
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
		background-color: #f9f9f9;
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
		white-space: nowrap;
		z-index: 10;
	}

	.expanded-content {
		overflow: hidden; /* Prevents content from overflowing */
		transition: max-height 3.3s ease; /* Smooth transition */
		height: 0; /* Start collapsed */
	}
</style>
