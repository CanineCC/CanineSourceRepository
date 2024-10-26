/* tslint:disable */
/* eslint-disable */
/**
 * BpnEngine API V1
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */


import * as runtime from '../runtime';
import type {
  PositionUpdatedOnDraftFeatureBody,
  WaypointUpdatedOnDraftFeatureBody,
} from '../models/index';
import {
    PositionUpdatedOnDraftFeatureBodyFromJSON,
    PositionUpdatedOnDraftFeatureBodyToJSON,
    WaypointUpdatedOnDraftFeatureBodyFromJSON,
    WaypointUpdatedOnDraftFeatureBodyToJSON,
} from '../models/index';

export interface PositionUpdatedOnDraftFeatureRequest {
    positionUpdatedOnDraftFeatureBody: PositionUpdatedOnDraftFeatureBody;
}

export interface PositionsUpdatedOnDraftFeatureRequest {
    positionUpdatedOnDraftFeatureBody: Array<PositionUpdatedOnDraftFeatureBody>;
}

export interface WaypointUpdatedOnDraftFeatureRequest {
    waypointUpdatedOnDraftFeatureBody: WaypointUpdatedOnDraftFeatureBody;
}

/**
 * 
 */
export class DraftFeatureDiagramApi extends runtime.BaseAPI {

    /**
     */
    async positionUpdatedOnDraftFeatureRaw(requestParameters: PositionUpdatedOnDraftFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['positionUpdatedOnDraftFeatureBody'] == null) {
            throw new runtime.RequiredError(
                'positionUpdatedOnDraftFeatureBody',
                'Required parameter "positionUpdatedOnDraftFeatureBody" was null or undefined when calling positionUpdatedOnDraftFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/DraftFeature/Diagram/PositionUpdated`,
            method: 'PATCH',
            headers: headerParameters,
            query: queryParameters,
            body: PositionUpdatedOnDraftFeatureBodyToJSON(requestParameters['positionUpdatedOnDraftFeatureBody']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async positionUpdatedOnDraftFeature(requestParameters: PositionUpdatedOnDraftFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.positionUpdatedOnDraftFeatureRaw(requestParameters, initOverrides);
    }

    /**
     */
    async positionsUpdatedOnDraftFeatureRaw(requestParameters: PositionsUpdatedOnDraftFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['positionUpdatedOnDraftFeatureBody'] == null) {
            throw new runtime.RequiredError(
                'positionUpdatedOnDraftFeatureBody',
                'Required parameter "positionUpdatedOnDraftFeatureBody" was null or undefined when calling positionsUpdatedOnDraftFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/DraftFeature/Diagram/PositionsUpdated`,
            method: 'PATCH',
            headers: headerParameters,
            query: queryParameters,
            body: requestParameters['positionUpdatedOnDraftFeatureBody']!.map(PositionUpdatedOnDraftFeatureBodyToJSON),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async positionsUpdatedOnDraftFeature(requestParameters: PositionsUpdatedOnDraftFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.positionsUpdatedOnDraftFeatureRaw(requestParameters, initOverrides);
    }

    /**
     */
    async waypointUpdatedOnDraftFeatureRaw(requestParameters: WaypointUpdatedOnDraftFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['waypointUpdatedOnDraftFeatureBody'] == null) {
            throw new runtime.RequiredError(
                'waypointUpdatedOnDraftFeatureBody',
                'Required parameter "waypointUpdatedOnDraftFeatureBody" was null or undefined when calling waypointUpdatedOnDraftFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/DraftFeature/Diagram/WaypointUpdated`,
            method: 'PATCH',
            headers: headerParameters,
            query: queryParameters,
            body: WaypointUpdatedOnDraftFeatureBodyToJSON(requestParameters['waypointUpdatedOnDraftFeatureBody']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async waypointUpdatedOnDraftFeature(requestParameters: WaypointUpdatedOnDraftFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.waypointUpdatedOnDraftFeatureRaw(requestParameters, initOverrides);
    }

}