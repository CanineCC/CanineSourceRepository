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
 * @interface DurationClassification
 */
export interface DurationClassification {
    /**
     * 
     * @type {number}
     * @memberof DurationClassification
     */
    fromMs?: number;
    /**
     * 
     * @type {number}
     * @memberof DurationClassification
     */
    toMs?: number;
    /**
     * 
     * @type {string}
     * @memberof DurationClassification
     */
    hexColor?: string;
    /**
     * 
     * @type {string}
     * @memberof DurationClassification
     */
    category?: string;
}

/**
 * Check if a given object implements the DurationClassification interface.
 */
export function instanceOfDurationClassification(value: object): value is DurationClassification {
    return true;
}

export function DurationClassificationFromJSON(json: any): DurationClassification {
    return DurationClassificationFromJSONTyped(json, false);
}

export function DurationClassificationFromJSONTyped(json: any, ignoreDiscriminator: boolean): DurationClassification {
    if (json == null) {
        return json;
    }
    return {
        
        'fromMs': json['fromMs'] == null ? undefined : json['fromMs'],
        'toMs': json['toMs'] == null ? undefined : json['toMs'],
        'hexColor': json['hexColor'] == null ? undefined : json['hexColor'],
        'category': json['category'] == null ? undefined : json['category'],
    };
}

export function DurationClassificationToJSON(value?: DurationClassification | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'fromMs': value['fromMs'],
        'toMs': value['toMs'],
        'hexColor': value['hexColor'],
        'category': value['category'],
    };
}
