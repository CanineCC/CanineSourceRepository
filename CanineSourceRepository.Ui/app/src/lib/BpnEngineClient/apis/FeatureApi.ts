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
  BpnFeatureRevision,
  BpnFeatureRevisionsStat,
  UpdateEnvironmentsOnFeatureBody,
} from '../models/index';
import {
    BpnFeatureRevisionFromJSON,
    BpnFeatureRevisionToJSON,
    BpnFeatureRevisionsStatFromJSON,
    BpnFeatureRevisionsStatToJSON,
    UpdateEnvironmentsOnFeatureBodyFromJSON,
    UpdateEnvironmentsOnFeatureBodyToJSON,
} from '../models/index';

export interface GetC4Level3DiagramSvgRequest {
    containerId: string;
}

export interface GetFeatureRevisionRequest {
    featureId: string;
    revision: number;
}

export interface GetFeatureRevisionStatsRequest {
    featureId: string;
    revision: number;
}

export interface UpdateEnvironmentsOnFeatureRequest {
    updateEnvironmentsOnFeatureBody: UpdateEnvironmentsOnFeatureBody;
}

/**
 * 
 */
export class FeatureApi extends runtime.BaseAPI {

    /**
     */
    async getC4Level3DiagramSvgRaw(requestParameters: GetC4Level3DiagramSvgRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<string>> {
        if (requestParameters['containerId'] == null) {
            throw new runtime.RequiredError(
                'containerId',
                'Required parameter "containerId" was null or undefined when calling getC4Level3DiagramSvg().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/BpnEngine/v1/Feature/DiagramSvg/{containerId}`.replace(`{${"containerId"}}`, encodeURIComponent(String(requestParameters['containerId']))),
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        if (this.isJsonMime(response.headers.get('content-type'))) {
            return new runtime.JSONApiResponse<string>(response);
        } else {
            return new runtime.TextApiResponse(response) as any;
        }
    }

    /**
     */
    async getC4Level3DiagramSvg(requestParameters: GetC4Level3DiagramSvgRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<string> {
        const response = await this.getC4Level3DiagramSvgRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async getFeatureRevisionRaw(requestParameters: GetFeatureRevisionRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<BpnFeatureRevision>> {
        if (requestParameters['featureId'] == null) {
            throw new runtime.RequiredError(
                'featureId',
                'Required parameter "featureId" was null or undefined when calling getFeatureRevision().'
            );
        }

        if (requestParameters['revision'] == null) {
            throw new runtime.RequiredError(
                'revision',
                'Required parameter "revision" was null or undefined when calling getFeatureRevision().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/BpnEngine/v1/Feature/{featureId}/{revision}`.replace(`{${"featureId"}}`, encodeURIComponent(String(requestParameters['featureId']))).replace(`{${"revision"}}`, encodeURIComponent(String(requestParameters['revision']))),
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => BpnFeatureRevisionFromJSON(jsonValue));
    }

    /**
     */
    async getFeatureRevision(requestParameters: GetFeatureRevisionRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<BpnFeatureRevision> {
        const response = await this.getFeatureRevisionRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async getFeatureRevisionStatsRaw(requestParameters: GetFeatureRevisionStatsRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<BpnFeatureRevisionsStat>> {
        if (requestParameters['featureId'] == null) {
            throw new runtime.RequiredError(
                'featureId',
                'Required parameter "featureId" was null or undefined when calling getFeatureRevisionStats().'
            );
        }

        if (requestParameters['revision'] == null) {
            throw new runtime.RequiredError(
                'revision',
                'Required parameter "revision" was null or undefined when calling getFeatureRevisionStats().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/BpnEngine/v1/Feature/Stats/{featureId}/{revision}`.replace(`{${"featureId"}}`, encodeURIComponent(String(requestParameters['featureId']))).replace(`{${"revision"}}`, encodeURIComponent(String(requestParameters['revision']))),
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => BpnFeatureRevisionsStatFromJSON(jsonValue));
    }

    /**
     */
    async getFeatureRevisionStats(requestParameters: GetFeatureRevisionStatsRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<BpnFeatureRevisionsStat> {
        const response = await this.getFeatureRevisionStatsRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async updateEnvironmentsOnFeatureRaw(requestParameters: UpdateEnvironmentsOnFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['updateEnvironmentsOnFeatureBody'] == null) {
            throw new runtime.RequiredError(
                'updateEnvironmentsOnFeatureBody',
                'Required parameter "updateEnvironmentsOnFeatureBody" was null or undefined when calling updateEnvironmentsOnFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/Feature/UpdateEnvironment`,
            method: 'PATCH',
            headers: headerParameters,
            query: queryParameters,
            body: UpdateEnvironmentsOnFeatureBodyToJSON(requestParameters['updateEnvironmentsOnFeatureBody']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async updateEnvironmentsOnFeature(requestParameters: UpdateEnvironmentsOnFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.updateEnvironmentsOnFeatureRaw(requestParameters, initOverrides);
    }

}
