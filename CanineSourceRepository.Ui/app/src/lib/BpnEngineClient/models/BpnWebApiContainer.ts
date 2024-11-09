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
import type { FeatureDetails } from './FeatureDetails';
import {
    FeatureDetailsFromJSON,
    FeatureDetailsFromJSONTyped,
    FeatureDetailsToJSON,
} from './FeatureDetails';

/**
 * 
 * @export
 * @interface BpnWebApiContainer
 */
export interface BpnWebApiContainer {
    /**
     * 
     * @type {string}
     * @memberof BpnWebApiContainer
     */
    id: string;
    /**
     * 
     * @type {string}
     * @memberof BpnWebApiContainer
     */
    systemId: string;
    /**
     * 
     * @type {string}
     * @memberof BpnWebApiContainer
     */
    name: string;
    /**
     * 
     * @type {string}
     * @memberof BpnWebApiContainer
     */
    description: string;
    /**
     * 
     * @type {Date}
     * @memberof BpnWebApiContainer
     */
    lastUpdatedTimestamp: Date;
    /**
     * 
     * @type {Date}
     * @memberof BpnWebApiContainer
     */
    createdTimestamp: Date;
    /**
     * 
     * @type {Array<FeatureDetails>}
     * @memberof BpnWebApiContainer
     */
    features: Array<FeatureDetails>;
}

/**
 * Check if a given object implements the BpnWebApiContainer interface.
 */
export function instanceOfBpnWebApiContainer(value: object): value is BpnWebApiContainer {
    if (!('id' in value) || value['id'] === undefined) return false;
    if (!('systemId' in value) || value['systemId'] === undefined) return false;
    if (!('name' in value) || value['name'] === undefined) return false;
    if (!('description' in value) || value['description'] === undefined) return false;
    if (!('lastUpdatedTimestamp' in value) || value['lastUpdatedTimestamp'] === undefined) return false;
    if (!('createdTimestamp' in value) || value['createdTimestamp'] === undefined) return false;
    if (!('features' in value) || value['features'] === undefined) return false;
    return true;
}

export function BpnWebApiContainerFromJSON(json: any): BpnWebApiContainer {
    return BpnWebApiContainerFromJSONTyped(json, false);
}

export function BpnWebApiContainerFromJSONTyped(json: any, ignoreDiscriminator: boolean): BpnWebApiContainer {
    if (json == null) {
        return json;
    }
    return {
        
        'id': json['id'],
        'systemId': json['systemId'],
        'name': json['name'],
        'description': json['description'],
        'lastUpdatedTimestamp': (new Date(json['lastUpdatedTimestamp'])),
        'createdTimestamp': (new Date(json['createdTimestamp'])),
        'features': ((json['features'] as Array<any>).map(FeatureDetailsFromJSON)),
    };
}

export function BpnWebApiContainerToJSON(value?: BpnWebApiContainer | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'id': value['id'],
        'systemId': value['systemId'],
        'name': value['name'],
        'description': value['description'],
        'lastUpdatedTimestamp': ((value['lastUpdatedTimestamp']).toISOString()),
        'createdTimestamp': ((value['createdTimestamp']).toISOString()),
        'features': ((value['features'] as Array<any>).map(FeatureDetailsToJSON)),
    };
}
