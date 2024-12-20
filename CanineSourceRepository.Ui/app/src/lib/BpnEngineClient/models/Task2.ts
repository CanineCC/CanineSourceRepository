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
 * @interface Task2
 */
export interface Task2 {
    /**
     * 
     * @type {string}
     * @memberof Task2
     */
    id?: string;
    /**
     * 
     * @type {string}
     * @memberof Task2
     */
    serviceDependencyId?: string;
    /**
     * 
     * @type {string}
     * @memberof Task2
     */
    namedConfigurationName?: string;
}

/**
 * Check if a given object implements the Task2 interface.
 */
export function instanceOfTask2(value: object): value is Task2 {
    return true;
}

export function Task2FromJSON(json: any): Task2 {
    return Task2FromJSONTyped(json, false);
}

export function Task2FromJSONTyped(json: any, ignoreDiscriminator: boolean): Task2 {
    if (json == null) {
        return json;
    }
    return {
        
        'id': json['id'] == null ? undefined : json['id'],
        'serviceDependencyId': json['serviceDependencyId'] == null ? undefined : json['serviceDependencyId'],
        'namedConfigurationName': json['namedConfigurationName'] == null ? undefined : json['namedConfigurationName'],
    };
}

export function Task2ToJSON(value?: Task2 | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'id': value['id'],
        'serviceDependencyId': value['serviceDependencyId'],
        'namedConfigurationName': value['namedConfigurationName'],
    };
}

