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
import type { SystemDetails } from './SystemDetails';
import {
    SystemDetailsFromJSON,
    SystemDetailsFromJSONTyped,
    SystemDetailsToJSON,
} from './SystemDetails';

/**
 * 
 * @export
 * @interface BpnSolution
 */
export interface BpnSolution {
    /**
     * 
     * @type {string}
     * @memberof BpnSolution
     */
    id: string;
    /**
     * 
     * @type {string}
     * @memberof BpnSolution
     */
    name: string;
    /**
     * 
     * @type {string}
     * @memberof BpnSolution
     */
    description: string;
    /**
     * 
     * @type {string}
     * @memberof BpnSolution
     */
    c4SystemDiagramSvg: string;
    /**
     * 
     * @type {Date}
     * @memberof BpnSolution
     */
    createdTimestamp: Date;
    /**
     * 
     * @type {Date}
     * @memberof BpnSolution
     */
    lastUpdatedTimestamp: Date;
    /**
     * 
     * @type {Array<SystemDetails>}
     * @memberof BpnSolution
     */
    systems: Array<SystemDetails>;
}

/**
 * Check if a given object implements the BpnSolution interface.
 */
export function instanceOfBpnSolution(value: object): value is BpnSolution {
    if (!('id' in value) || value['id'] === undefined) return false;
    if (!('name' in value) || value['name'] === undefined) return false;
    if (!('description' in value) || value['description'] === undefined) return false;
    if (!('c4SystemDiagramSvg' in value) || value['c4SystemDiagramSvg'] === undefined) return false;
    if (!('createdTimestamp' in value) || value['createdTimestamp'] === undefined) return false;
    if (!('lastUpdatedTimestamp' in value) || value['lastUpdatedTimestamp'] === undefined) return false;
    if (!('systems' in value) || value['systems'] === undefined) return false;
    return true;
}

export function BpnSolutionFromJSON(json: any): BpnSolution {
    return BpnSolutionFromJSONTyped(json, false);
}

export function BpnSolutionFromJSONTyped(json: any, ignoreDiscriminator: boolean): BpnSolution {
    if (json == null) {
        return json;
    }
    return {
        
        'id': json['id'],
        'name': json['name'],
        'description': json['description'],
        'c4SystemDiagramSvg': json['c4SystemDiagramSvg'],
        'createdTimestamp': (new Date(json['createdTimestamp'])),
        'lastUpdatedTimestamp': (new Date(json['lastUpdatedTimestamp'])),
        'systems': ((json['systems'] as Array<any>).map(SystemDetailsFromJSON)),
    };
}

export function BpnSolutionToJSON(value?: BpnSolution | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'id': value['id'],
        'name': value['name'],
        'description': value['description'],
        'c4SystemDiagramSvg': value['c4SystemDiagramSvg'],
        'createdTimestamp': ((value['createdTimestamp']).toISOString()),
        'lastUpdatedTimestamp': ((value['lastUpdatedTimestamp']).toISOString()),
        'systems': ((value['systems'] as Array<any>).map(SystemDetailsToJSON)),
    };
}

