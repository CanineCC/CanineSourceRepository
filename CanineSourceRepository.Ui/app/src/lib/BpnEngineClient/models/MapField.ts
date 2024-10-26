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
 * @interface MapField
 */
export interface MapField {
    /**
     * 
     * @type {string}
     * @memberof MapField
     */
    from?: string;
    /**
     * 
     * @type {string}
     * @memberof MapField
     */
    to?: string;
}

/**
 * Check if a given object implements the MapField interface.
 */
export function instanceOfMapField(value: object): value is MapField {
    return true;
}

export function MapFieldFromJSON(json: any): MapField {
    return MapFieldFromJSONTyped(json, false);
}

export function MapFieldFromJSONTyped(json: any, ignoreDiscriminator: boolean): MapField {
    if (json == null) {
        return json;
    }
    return {
        
        'from': json['from'] == null ? undefined : json['from'],
        'to': json['to'] == null ? undefined : json['to'],
    };
}

export function MapFieldToJSON(value?: MapField | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'from': value['from'],
        'to': value['to'],
    };
}

