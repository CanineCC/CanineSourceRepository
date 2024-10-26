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
  AddRecordToTaskBody,
  DeleteRecordOnTaskBody,
  UpdateCodeOnTaskBody,
  UpdateRecordOnTaskBody,
  UpdateServiceDependencyBody,
  UpdateTaskPurposeFeatureBody,
} from '../models/index';
import {
    AddRecordToTaskBodyFromJSON,
    AddRecordToTaskBodyToJSON,
    DeleteRecordOnTaskBodyFromJSON,
    DeleteRecordOnTaskBodyToJSON,
    UpdateCodeOnTaskBodyFromJSON,
    UpdateCodeOnTaskBodyToJSON,
    UpdateRecordOnTaskBodyFromJSON,
    UpdateRecordOnTaskBodyToJSON,
    UpdateServiceDependencyBodyFromJSON,
    UpdateServiceDependencyBodyToJSON,
    UpdateTaskPurposeFeatureBodyFromJSON,
    UpdateTaskPurposeFeatureBodyToJSON,
} from '../models/index';

export interface AddRecordToTaskFeatureRequest {
    addRecordToTaskBody: AddRecordToTaskBody;
}

export interface DeleteRecordOnTaskFeatureRequest {
    deleteRecordOnTaskBody: DeleteRecordOnTaskBody;
}

export interface UpdateCodeOnTaskFeatureRequest {
    updateCodeOnTaskBody: UpdateCodeOnTaskBody;
}

export interface UpdateRecordOnTaskFeatureRequest {
    updateRecordOnTaskBody: UpdateRecordOnTaskBody;
}

export interface UpdateServiceDependencyFeatureRequest {
    updateServiceDependencyBody: UpdateServiceDependencyBody;
}

export interface UpdateTaskPurposeFeatureRequest {
    updateTaskPurposeFeatureBody: UpdateTaskPurposeFeatureBody;
}

/**
 * 
 */
export class DraftFeatureTaskApi extends runtime.BaseAPI {

    /**
     */
    async addRecordToTaskFeatureRaw(requestParameters: AddRecordToTaskFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['addRecordToTaskBody'] == null) {
            throw new runtime.RequiredError(
                'addRecordToTaskBody',
                'Required parameter "addRecordToTaskBody" was null or undefined when calling addRecordToTaskFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/DraftFeature/AddRecordToTask`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: AddRecordToTaskBodyToJSON(requestParameters['addRecordToTaskBody']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async addRecordToTaskFeature(requestParameters: AddRecordToTaskFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.addRecordToTaskFeatureRaw(requestParameters, initOverrides);
    }

    /**
     */
    async deleteRecordOnTaskFeatureRaw(requestParameters: DeleteRecordOnTaskFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['deleteRecordOnTaskBody'] == null) {
            throw new runtime.RequiredError(
                'deleteRecordOnTaskBody',
                'Required parameter "deleteRecordOnTaskBody" was null or undefined when calling deleteRecordOnTaskFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/DraftFeature/DeleteRecordOnTask`,
            method: 'DELETE',
            headers: headerParameters,
            query: queryParameters,
            body: DeleteRecordOnTaskBodyToJSON(requestParameters['deleteRecordOnTaskBody']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async deleteRecordOnTaskFeature(requestParameters: DeleteRecordOnTaskFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.deleteRecordOnTaskFeatureRaw(requestParameters, initOverrides);
    }

    /**
     */
    async updateCodeOnTaskFeatureRaw(requestParameters: UpdateCodeOnTaskFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['updateCodeOnTaskBody'] == null) {
            throw new runtime.RequiredError(
                'updateCodeOnTaskBody',
                'Required parameter "updateCodeOnTaskBody" was null or undefined when calling updateCodeOnTaskFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/DraftFeature/UpdateCodeOnTask`,
            method: 'PUT',
            headers: headerParameters,
            query: queryParameters,
            body: UpdateCodeOnTaskBodyToJSON(requestParameters['updateCodeOnTaskBody']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async updateCodeOnTaskFeature(requestParameters: UpdateCodeOnTaskFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.updateCodeOnTaskFeatureRaw(requestParameters, initOverrides);
    }

    /**
     */
    async updateRecordOnTaskFeatureRaw(requestParameters: UpdateRecordOnTaskFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['updateRecordOnTaskBody'] == null) {
            throw new runtime.RequiredError(
                'updateRecordOnTaskBody',
                'Required parameter "updateRecordOnTaskBody" was null or undefined when calling updateRecordOnTaskFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/DraftFeature/UpdateRecordOnTask`,
            method: 'PUT',
            headers: headerParameters,
            query: queryParameters,
            body: UpdateRecordOnTaskBodyToJSON(requestParameters['updateRecordOnTaskBody']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async updateRecordOnTaskFeature(requestParameters: UpdateRecordOnTaskFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.updateRecordOnTaskFeatureRaw(requestParameters, initOverrides);
    }

    /**
     */
    async updateServiceDependencyFeatureRaw(requestParameters: UpdateServiceDependencyFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['updateServiceDependencyBody'] == null) {
            throw new runtime.RequiredError(
                'updateServiceDependencyBody',
                'Required parameter "updateServiceDependencyBody" was null or undefined when calling updateServiceDependencyFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/DraftFeature/UpdateServiceDependency`,
            method: 'PUT',
            headers: headerParameters,
            query: queryParameters,
            body: UpdateServiceDependencyBodyToJSON(requestParameters['updateServiceDependencyBody']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async updateServiceDependencyFeature(requestParameters: UpdateServiceDependencyFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.updateServiceDependencyFeatureRaw(requestParameters, initOverrides);
    }

    /**
     */
    async updateTaskPurposeFeatureRaw(requestParameters: UpdateTaskPurposeFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['updateTaskPurposeFeatureBody'] == null) {
            throw new runtime.RequiredError(
                'updateTaskPurposeFeatureBody',
                'Required parameter "updateTaskPurposeFeatureBody" was null or undefined when calling updateTaskPurposeFeature().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/BpnEngine/v1/DraftFeature/UpdateTaskPurposeFeature`,
            method: 'PUT',
            headers: headerParameters,
            query: queryParameters,
            body: UpdateTaskPurposeFeatureBodyToJSON(requestParameters['updateTaskPurposeFeatureBody']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async updateTaskPurposeFeature(requestParameters: UpdateTaskPurposeFeatureRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.updateTaskPurposeFeatureRaw(requestParameters, initOverrides);
    }

}