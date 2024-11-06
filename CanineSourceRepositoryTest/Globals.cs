﻿global using System.Reflection;

global using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level2_Container;
global using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component;
global using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code;
global using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.Snippets;
global using static CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component.BpnFeatureDiagram;
global using static CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component.BpnFeatureProjection;
global using static CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code.BpnTask;

global using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnContext;
global using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnDraftFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.AddRecordToTaskFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.DeleteRecordOnTaskFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.UpdateCodeOnTaskFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.UpdateRecordOnTaskFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnTask.UpdateServiceDependencyFeature;
global using static CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.FeaturesForBpnContext.CreateContainerFeature;
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
