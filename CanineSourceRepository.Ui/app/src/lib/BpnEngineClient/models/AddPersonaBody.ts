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
 * @interface AddPersonaBody
 */
export interface AddPersonaBody {
    /**
     * 
     * @type {string}
     * @memberof AddPersonaBody
     */
    containerId: string;
    /**
     * 
     * @type {string}
     * @memberof AddPersonaBody
     */
    name: string;
    /**
     * 
     * @type {string}
     * @memberof AddPersonaBody
     */
    description: string;
    /**
     * 
     * @type {string}
     * @memberof AddPersonaBody
     */
    relationToSystem: string;
}

/**
 * Check if a given object implements the AddPersonaBody interface.
 */
export function instanceOfAddPersonaBody(value: object): value is AddPersonaBody {
    if (!('containerId' in value) || value['containerId'] === undefined) return false;
    if (!('name' in value) || value['name'] === undefined) return false;
    if (!('description' in value) || value['description'] === undefined) return false;
    if (!('relationToSystem' in value) || value['relationToSystem'] === undefined) return false;
    return true;
}

export function AddPersonaBodyFromJSON(json: any): AddPersonaBody {
    return AddPersonaBodyFromJSONTyped(json, false);
}

export function AddPersonaBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): AddPersonaBody {
    if (json == null) {
        return json;
    }
    return {
        
        'containerId': json['containerId'],
        'name': json['name'],
        'description': json['description'],
        'relationToSystem': json['relationToSystem'],
    };
}

export function AddPersonaBodyToJSON(value?: AddPersonaBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'containerId': value['containerId'],
        'name': value['name'],
        'description': value['description'],
        'relationToSystem': value['relationToSystem'],
    };
}
