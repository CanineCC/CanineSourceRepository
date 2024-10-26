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
 * @interface CreateContextBody
 */
export interface CreateContextBody {
    /**
     * 
     * @type {string}
     * @memberof CreateContextBody
     */
    name: string;
}

/**
 * Check if a given object implements the CreateContextBody interface.
 */
export function instanceOfCreateContextBody(value: object): value is CreateContextBody {
    if (!('name' in value) || value['name'] === undefined) return false;
    return true;
}

export function CreateContextBodyFromJSON(json: any): CreateContextBody {
    return CreateContextBodyFromJSONTyped(json, false);
}

export function CreateContextBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): CreateContextBody {
    if (json == null) {
        return json;
    }
    return {
        
        'name': json['name'],
    };
}

export function CreateContextBodyToJSON(value?: CreateContextBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'name': value['name'],
    };
}
