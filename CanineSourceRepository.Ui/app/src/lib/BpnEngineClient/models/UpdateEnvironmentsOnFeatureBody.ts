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
import type { Environment } from './Environment';
import {
    EnvironmentFromJSON,
    EnvironmentFromJSONTyped,
    EnvironmentToJSON,
} from './Environment';

/**
 * 
 * @export
 * @interface UpdateEnvironmentsOnFeatureBody
 */
export interface UpdateEnvironmentsOnFeatureBody {
    /**
     * 
     * @type {string}
     * @memberof UpdateEnvironmentsOnFeatureBody
     */
    featureId: string;
    /**
     * 
     * @type {number}
     * @memberof UpdateEnvironmentsOnFeatureBody
     */
    featureVersion: number;
    /**
     * 
     * @type {Array<Environment>}
     * @memberof UpdateEnvironmentsOnFeatureBody
     */
    environment: Array<Environment>;
}

/**
 * Check if a given object implements the UpdateEnvironmentsOnFeatureBody interface.
 */
export function instanceOfUpdateEnvironmentsOnFeatureBody(value: object): value is UpdateEnvironmentsOnFeatureBody {
    if (!('featureId' in value) || value['featureId'] === undefined) return false;
    if (!('featureVersion' in value) || value['featureVersion'] === undefined) return false;
    if (!('environment' in value) || value['environment'] === undefined) return false;
    return true;
}

export function UpdateEnvironmentsOnFeatureBodyFromJSON(json: any): UpdateEnvironmentsOnFeatureBody {
    return UpdateEnvironmentsOnFeatureBodyFromJSONTyped(json, false);
}

export function UpdateEnvironmentsOnFeatureBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): UpdateEnvironmentsOnFeatureBody {
    if (json == null) {
        return json;
    }
    return {
        
        'featureId': json['featureId'],
        'featureVersion': json['featureVersion'],
        'environment': ((json['environment'] as Array<any>).map(EnvironmentFromJSON)),
    };
}

export function UpdateEnvironmentsOnFeatureBodyToJSON(value?: UpdateEnvironmentsOnFeatureBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'featureId': value['featureId'],
        'featureVersion': value['featureVersion'],
        'environment': ((value['environment'] as Array<any>).map(EnvironmentToJSON)),
    };
}

