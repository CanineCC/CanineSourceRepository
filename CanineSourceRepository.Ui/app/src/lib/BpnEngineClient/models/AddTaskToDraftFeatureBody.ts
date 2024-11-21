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
import type { Task } from './Task';
import {
    TaskFromJSON,
    TaskFromJSONTyped,
    TaskToJSON,
} from './Task';

/**
 * 
 * @export
 * @interface AddTaskToDraftFeatureBody
 */
export interface AddTaskToDraftFeatureBody {
    /**
     * 
     * @type {string}
     * @memberof AddTaskToDraftFeatureBody
     */
    featureId: string;
    /**
     * 
     * @type {Task}
     * @memberof AddTaskToDraftFeatureBody
     */
    task: Task;
}

/**
 * Check if a given object implements the AddTaskToDraftFeatureBody interface.
 */
export function instanceOfAddTaskToDraftFeatureBody(value: object): value is AddTaskToDraftFeatureBody {
    if (!('featureId' in value) || value['featureId'] === undefined) return false;
    if (!('task' in value) || value['task'] === undefined) return false;
    return true;
}

export function AddTaskToDraftFeatureBodyFromJSON(json: any): AddTaskToDraftFeatureBody {
    return AddTaskToDraftFeatureBodyFromJSONTyped(json, false);
}

export function AddTaskToDraftFeatureBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): AddTaskToDraftFeatureBody {
    if (json == null) {
        return json;
    }
    return {
        
        'featureId': json['featureId'],
        'task': TaskFromJSON(json['task']),
    };
}

export function AddTaskToDraftFeatureBodyToJSON(value?: AddTaskToDraftFeatureBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'featureId': value['featureId'],
        'task': TaskToJSON(value['task']),
    };
}

