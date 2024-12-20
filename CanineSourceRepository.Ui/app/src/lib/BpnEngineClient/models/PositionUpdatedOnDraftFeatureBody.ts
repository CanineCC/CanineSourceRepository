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
import type { Position } from './Position';
import {
    PositionFromJSON,
    PositionFromJSONTyped,
    PositionToJSON,
} from './Position';

/**
 * 
 * @export
 * @interface PositionUpdatedOnDraftFeatureBody
 */
export interface PositionUpdatedOnDraftFeatureBody {
    /**
     * 
     * @type {string}
     * @memberof PositionUpdatedOnDraftFeatureBody
     */
    featureId: string;
    /**
     * 
     * @type {string}
     * @memberof PositionUpdatedOnDraftFeatureBody
     */
    taskId: string;
    /**
     * 
     * @type {Position}
     * @memberof PositionUpdatedOnDraftFeatureBody
     */
    position: Position;
}

/**
 * Check if a given object implements the PositionUpdatedOnDraftFeatureBody interface.
 */
export function instanceOfPositionUpdatedOnDraftFeatureBody(value: object): value is PositionUpdatedOnDraftFeatureBody {
    if (!('featureId' in value) || value['featureId'] === undefined) return false;
    if (!('taskId' in value) || value['taskId'] === undefined) return false;
    if (!('position' in value) || value['position'] === undefined) return false;
    return true;
}

export function PositionUpdatedOnDraftFeatureBodyFromJSON(json: any): PositionUpdatedOnDraftFeatureBody {
    return PositionUpdatedOnDraftFeatureBodyFromJSONTyped(json, false);
}

export function PositionUpdatedOnDraftFeatureBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): PositionUpdatedOnDraftFeatureBody {
    if (json == null) {
        return json;
    }
    return {
        
        'featureId': json['featureId'],
        'taskId': json['taskId'],
        'position': PositionFromJSON(json['position']),
    };
}

export function PositionUpdatedOnDraftFeatureBodyToJSON(value?: PositionUpdatedOnDraftFeatureBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'featureId': value['featureId'],
        'taskId': value['taskId'],
        'position': PositionToJSON(value['position']),
    };
}

