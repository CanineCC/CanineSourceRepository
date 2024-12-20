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
 * @interface RemovePersonaBody
 */
export interface RemovePersonaBody {
    /**
     * 
     * @type {string}
     * @memberof RemovePersonaBody
     */
    personaId: string;
}

/**
 * Check if a given object implements the RemovePersonaBody interface.
 */
export function instanceOfRemovePersonaBody(value: object): value is RemovePersonaBody {
    if (!('personaId' in value) || value['personaId'] === undefined) return false;
    return true;
}

export function RemovePersonaBodyFromJSON(json: any): RemovePersonaBody {
    return RemovePersonaBodyFromJSONTyped(json, false);
}

export function RemovePersonaBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): RemovePersonaBody {
    if (json == null) {
        return json;
    }
    return {
        
        'personaId': json['personaId'],
    };
}

export function RemovePersonaBodyToJSON(value?: RemovePersonaBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'personaId': value['personaId'],
    };
}

