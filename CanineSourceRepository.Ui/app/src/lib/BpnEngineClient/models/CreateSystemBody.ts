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
 * @interface CreateSystemBody
 */
export interface CreateSystemBody {
    /**
     * 
     * @type {string}
     * @memberof CreateSystemBody
     */
    solutionId: string;
    /**
     * 
     * @type {string}
     * @memberof CreateSystemBody
     */
    name: string;
}

/**
 * Check if a given object implements the CreateSystemBody interface.
 */
export function instanceOfCreateSystemBody(value: object): value is CreateSystemBody {
    if (!('solutionId' in value) || value['solutionId'] === undefined) return false;
    if (!('name' in value) || value['name'] === undefined) return false;
    return true;
}

export function CreateSystemBodyFromJSON(json: any): CreateSystemBody {
    return CreateSystemBodyFromJSONTyped(json, false);
}

export function CreateSystemBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): CreateSystemBody {
    if (json == null) {
        return json;
    }
    return {
        
        'solutionId': json['solutionId'],
        'name': json['name'],
    };
}

export function CreateSystemBodyToJSON(value?: CreateSystemBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'solutionId': value['solutionId'],
        'name': value['name'],
    };
}

