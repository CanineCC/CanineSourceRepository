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
 * @interface ComponentCosumedByPersonaBody
 */
export interface ComponentCosumedByPersonaBody {
    /**
     * 
     * @type {string}
     * @memberof ComponentCosumedByPersonaBody
     */
    personaId: string;
    /**
     * 
     * @type {string}
     * @memberof ComponentCosumedByPersonaBody
     */
    consumeDescription: string;
    /**
     * 
     * @type {string}
     * @memberof ComponentCosumedByPersonaBody
     */
    componentId: string;
}

/**
 * Check if a given object implements the ComponentCosumedByPersonaBody interface.
 */
export function instanceOfComponentCosumedByPersonaBody(value: object): value is ComponentCosumedByPersonaBody {
    if (!('personaId' in value) || value['personaId'] === undefined) return false;
    if (!('consumeDescription' in value) || value['consumeDescription'] === undefined) return false;
    if (!('componentId' in value) || value['componentId'] === undefined) return false;
    return true;
}

export function ComponentCosumedByPersonaBodyFromJSON(json: any): ComponentCosumedByPersonaBody {
    return ComponentCosumedByPersonaBodyFromJSONTyped(json, false);
}

export function ComponentCosumedByPersonaBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): ComponentCosumedByPersonaBody {
    if (json == null) {
        return json;
    }
    return {
        
        'personaId': json['personaId'],
        'consumeDescription': json['consumeDescription'],
        'componentId': json['componentId'],
    };
}

export function ComponentCosumedByPersonaBodyToJSON(value?: ComponentCosumedByPersonaBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'personaId': value['personaId'],
        'consumeDescription': value['consumeDescription'],
        'componentId': value['componentId'],
    };
}

