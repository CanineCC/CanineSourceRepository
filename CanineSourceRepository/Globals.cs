﻿global using Marten;
global using Marten.Events;
global using Marten.Events.Aggregation;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Collections.Immutable;
global using System.Reflection;
global using System.Text;
global using System.Text.RegularExpressions;
global using Microsoft.AspNetCore.Mvc;
global using System.Diagnostics;
global using Npgsql;

global using CanineSourceRepository.BusinessProcessNotation.BpnContext;
global using CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
global using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnContext;
global using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature;
global using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask;
global using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureDiagram;

global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.AddRecordToTaskFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.DeleteRecordOnTaskFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.UpdateCodeOnTaskFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.UpdateRecordOnTaskFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.UpdateServiceDependencyFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnContext.CreateContextFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.AddDraftFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.ReleaseFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.UpdateDraftFeaturePurposeFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.UpdateEnvironmentsOnFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.PositionUpdatedOnDraftFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.RemoveTransitionFromDraftFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.ResetDraftFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.WaypointUpdatedOnDraftFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.AddTaskToDraftFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.AddTransitionToDraftFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature.RemoveTaskFromDraftFeatureFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.UpdateTaskPurposeFeature;
