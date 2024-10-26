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

import { mapValues } from '../runtime';
import type { RecordDefinition } from './RecordDefinition';
import {
    RecordDefinitionFromJSON,
    RecordDefinitionFromJSONTyped,
    RecordDefinitionToJSON,
} from './RecordDefinition';

/**
 * 
 * @export
 * @interface AddRecordToTaskBody
 */
export interface AddRecordToTaskBody {
    /**
     * 
     * @type {string}
     * @memberof AddRecordToTaskBody
     */
    featureId: string;
    /**
     * 
     * @type {string}
     * @memberof AddRecordToTaskBody
     */
    taskId: string;
    /**
     * 
     * @type {RecordDefinition}
     * @memberof AddRecordToTaskBody
     */
    recordDefinition: RecordDefinition;
}

/**
 * Check if a given object implements the AddRecordToTaskBody interface.
 */
export function instanceOfAddRecordToTaskBody(value: object): value is AddRecordToTaskBody {
    if (!('featureId' in value) || value['featureId'] === undefined) return false;
    if (!('taskId' in value) || value['taskId'] === undefined) return false;
    if (!('recordDefinition' in value) || value['recordDefinition'] === undefined) return false;
    return true;
}

export function AddRecordToTaskBodyFromJSON(json: any): AddRecordToTaskBody {
    return AddRecordToTaskBodyFromJSONTyped(json, false);
}

export function AddRecordToTaskBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): AddRecordToTaskBody {
    if (json == null) {
        return json;
    }
    return {
        
        'featureId': json['featureId'],
        'taskId': json['taskId'],
        'recordDefinition': RecordDefinitionFromJSON(json['recordDefinition']),
    };
}

export function AddRecordToTaskBodyToJSON(value?: AddRecordToTaskBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'featureId': value['featureId'],
        'taskId': value['taskId'],
        'recordDefinition': RecordDefinitionToJSON(value['recordDefinition']),
    };
}
