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
import type { Stats } from './Stats';
import {
    StatsFromJSON,
    StatsFromJSONTyped,
    StatsToJSON,
} from './Stats';
import type { TaskStats } from './TaskStats';
import {
    TaskStatsFromJSON,
    TaskStatsFromJSONTyped,
    TaskStatsToJSON,
} from './TaskStats';

/**
 * 
 * @export
 * @interface BpnFeatureRevisionsStat
 */
export interface BpnFeatureRevisionsStat {
    /**
     * 
     * @type {string}
     * @memberof BpnFeatureRevisionsStat
     */
    id: string;
    /**
     * 
     * @type {Stats}
     * @memberof BpnFeatureRevisionsStat
     */
    featureStats: Stats;
    /**
     * 
     * @type {Stats}
     * @memberof BpnFeatureRevisionsStat
     */
    revisionStats: Stats;
    /**
     * 
     * @type {Array<TaskStats>}
     * @memberof BpnFeatureRevisionsStat
     */
    taskStats: Array<TaskStats>;
}

/**
 * Check if a given object implements the BpnFeatureRevisionsStat interface.
 */
export function instanceOfBpnFeatureRevisionsStat(value: object): value is BpnFeatureRevisionsStat {
    if (!('id' in value) || value['id'] === undefined) return false;
    if (!('featureStats' in value) || value['featureStats'] === undefined) return false;
    if (!('revisionStats' in value) || value['revisionStats'] === undefined) return false;
    if (!('taskStats' in value) || value['taskStats'] === undefined) return false;
    return true;
}

export function BpnFeatureRevisionsStatFromJSON(json: any): BpnFeatureRevisionsStat {
    return BpnFeatureRevisionsStatFromJSONTyped(json, false);
}

export function BpnFeatureRevisionsStatFromJSONTyped(json: any, ignoreDiscriminator: boolean): BpnFeatureRevisionsStat {
    if (json == null) {
        return json;
    }
    return {
        
        'id': json['id'],
        'featureStats': StatsFromJSON(json['featureStats']),
        'revisionStats': StatsFromJSON(json['revisionStats']),
        'taskStats': ((json['taskStats'] as Array<any>).map(TaskStatsFromJSON)),
    };
}

export function BpnFeatureRevisionsStatToJSON(value?: BpnFeatureRevisionsStat | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'id': value['id'],
        'featureStats': StatsToJSON(value['featureStats']),
        'revisionStats': StatsToJSON(value['revisionStats']),
        'taskStats': ((value['taskStats'] as Array<any>).map(TaskStatsToJSON)),
    };
}

