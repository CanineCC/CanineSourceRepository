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
 * @interface ComponentNoLongerConsumedByPersonaBody
 */
export interface ComponentNoLongerConsumedByPersonaBody {
    /**
     * 
     * @type {string}
     * @memberof ComponentNoLongerConsumedByPersonaBody
     */
    personaId: string;
    /**
     * 
     * @type {string}
     * @memberof ComponentNoLongerConsumedByPersonaBody
     */
    componentId: string;
}

/**
 * Check if a given object implements the ComponentNoLongerConsumedByPersonaBody interface.
 */
export function instanceOfComponentNoLongerConsumedByPersonaBody(value: object): value is ComponentNoLongerConsumedByPersonaBody {
    if (!('personaId' in value) || value['personaId'] === undefined) return false;
    if (!('componentId' in value) || value['componentId'] === undefined) return false;
    return true;
}

export function ComponentNoLongerConsumedByPersonaBodyFromJSON(json: any): ComponentNoLongerConsumedByPersonaBody {
    return ComponentNoLongerConsumedByPersonaBodyFromJSONTyped(json, false);
}

export function ComponentNoLongerConsumedByPersonaBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): ComponentNoLongerConsumedByPersonaBody {
    if (json == null) {
        return json;
    }
    return {
        
        'personaId': json['personaId'],
        'componentId': json['componentId'],
    };
}

export function ComponentNoLongerConsumedByPersonaBodyToJSON(value?: ComponentNoLongerConsumedByPersonaBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'personaId': value['personaId'],
        'componentId': value['componentId'],
    };
}
