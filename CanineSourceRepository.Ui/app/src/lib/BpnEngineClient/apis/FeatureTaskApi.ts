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
  DurationClassification,
} from '../models/index';
import {
    DurationClassificationFromJSON,
    DurationClassificationToJSON,
} from '../models/index';

/**
 * 
 */
export class FeatureTaskApi extends runtime.BaseAPI {

    /**
     */
    async getTaskDurationClassificationRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<DurationClassification>>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/BpnEngine/v1/Task/DurationClassification`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(DurationClassificationFromJSON));
    }

    /**
     */
    async getTaskDurationClassification(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<DurationClassification>> {
        const response = await this.getTaskDurationClassificationRaw(initOverrides);
        return await response.value();
    }

}