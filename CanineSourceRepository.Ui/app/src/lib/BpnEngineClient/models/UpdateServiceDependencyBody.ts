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
/**
 * 
 * @export
 * @interface UpdateServiceDependencyBody
 */
export interface UpdateServiceDependencyBody {
    /**
     * 
     * @type {string}
     * @memberof UpdateServiceDependencyBody
     */
    featureId: string;
    /**
     * 
     * @type {string}
     * @memberof UpdateServiceDependencyBody
     */
    taskId: string;
    /**
     * 
     * @type {string}
     * @memberof UpdateServiceDependencyBody
     */
    serviceDependency: string;
    /**
     * 
     * @type {string}
     * @memberof UpdateServiceDependencyBody
     */
    namedConfiguration: string;
}

/**
 * Check if a given object implements the UpdateServiceDependencyBody interface.
 */
export function instanceOfUpdateServiceDependencyBody(value: object): value is UpdateServiceDependencyBody {
    if (!('featureId' in value) || value['featureId'] === undefined) return false;
    if (!('taskId' in value) || value['taskId'] === undefined) return false;
    if (!('serviceDependency' in value) || value['serviceDependency'] === undefined) return false;
    if (!('namedConfiguration' in value) || value['namedConfiguration'] === undefined) return false;
    return true;
}

export function UpdateServiceDependencyBodyFromJSON(json: any): UpdateServiceDependencyBody {
    return UpdateServiceDependencyBodyFromJSONTyped(json, false);
}

export function UpdateServiceDependencyBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): UpdateServiceDependencyBody {
    if (json == null) {
        return json;
    }
    return {
        
        'featureId': json['featureId'],
        'taskId': json['taskId'],
        'serviceDependency': json['serviceDependency'],
        'namedConfiguration': json['namedConfiguration'],
    };
}

export function UpdateServiceDependencyBodyToJSON(value?: UpdateServiceDependencyBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'featureId': value['featureId'],
        'taskId': value['taskId'],
        'serviceDependency': value['serviceDependency'],
        'namedConfiguration': value['namedConfiguration'],
    };
}

