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
import type { BpnFeatureDiagram } from './BpnFeatureDiagram';
import {
    BpnFeatureDiagramFromJSON,
    BpnFeatureDiagramFromJSONTyped,
    BpnFeatureDiagramToJSON,
} from './BpnFeatureDiagram';
import type { BpnTransition } from './BpnTransition';
import {
    BpnTransitionFromJSON,
    BpnTransitionFromJSONTyped,
    BpnTransitionToJSON,
} from './BpnTransition';
import type { Environment } from './Environment';
import {
    EnvironmentFromJSON,
    EnvironmentFromJSONTyped,
    EnvironmentToJSON,
} from './Environment';
import type { BpnTask } from './BpnTask';
import {
    BpnTaskFromJSON,
    BpnTaskFromJSONTyped,
    BpnTaskToJSON,
} from './BpnTask';

/**
 * 
 * @export
 * @interface BpnFeatureVersion
 */
export interface BpnFeatureVersion {
    /**
     * 
     * @type {BpnFeatureDiagram}
     * @memberof BpnFeatureVersion
     */
    diagram?: BpnFeatureDiagram;
    /**
     * 
     * @type {string}
     * @memberof BpnFeatureVersion
     */
    name?: string;
    /**
     * 
     * @type {string}
     * @memberof BpnFeatureVersion
     */
    objective?: string;
    /**
     * 
     * @type {string}
     * @memberof BpnFeatureVersion
     */
    flowOverview?: string;
    /**
     * 
     * @type {string}
     * @memberof BpnFeatureVersion
     */
    releasedBy?: string;
    /**
     * 
     * @type {Date}
     * @memberof BpnFeatureVersion
     */
    releasedDate?: Date | null;
    /**
     * 
     * @type {number}
     * @memberof BpnFeatureVersion
     */
    revision?: number;
    /**
     * 
     * @type {Array<BpnTask>}
     * @memberof BpnFeatureVersion
     */
    tasks?: Array<BpnTask>;
    /**
     * 
     * @type {Array<BpnTransition>}
     * @memberof BpnFeatureVersion
     */
    transitions?: Array<BpnTransition>;
    /**
     * 
     * @type {Array<Environment>}
     * @memberof BpnFeatureVersion
     */
    targetEnvironments?: Array<Environment>;
}

/**
 * Check if a given object implements the BpnFeatureVersion interface.
 */
export function instanceOfBpnFeatureVersion(value: object): value is BpnFeatureVersion {
    return true;
}

export function BpnFeatureVersionFromJSON(json: any): BpnFeatureVersion {
    return BpnFeatureVersionFromJSONTyped(json, false);
}

export function BpnFeatureVersionFromJSONTyped(json: any, ignoreDiscriminator: boolean): BpnFeatureVersion {
    if (json == null) {
        return json;
    }
    return {
        
        'diagram': json['diagram'] == null ? undefined : BpnFeatureDiagramFromJSON(json['diagram']),
        'name': json['name'] == null ? undefined : json['name'],
        'objective': json['objective'] == null ? undefined : json['objective'],
        'flowOverview': json['flowOverview'] == null ? undefined : json['flowOverview'],
        'releasedBy': json['releasedBy'] == null ? undefined : json['releasedBy'],
        'releasedDate': json['releasedDate'] == null ? undefined : (new Date(json['releasedDate'])),
        'revision': json['revision'] == null ? undefined : json['revision'],
        'tasks': json['tasks'] == null ? undefined : ((json['tasks'] as Array<any>).map(BpnTaskFromJSON)),
        'transitions': json['transitions'] == null ? undefined : ((json['transitions'] as Array<any>).map(BpnTransitionFromJSON)),
        'targetEnvironments': json['targetEnvironments'] == null ? undefined : ((json['targetEnvironments'] as Array<any>).map(EnvironmentFromJSON)),
    };
}

export function BpnFeatureVersionToJSON(value?: BpnFeatureVersion | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'diagram': BpnFeatureDiagramToJSON(value['diagram']),
        'name': value['name'],
        'objective': value['objective'],
        'flowOverview': value['flowOverview'],
        'releasedBy': value['releasedBy'],
        'releasedDate': value['releasedDate'] == null ? undefined : ((value['releasedDate'] as any).toISOString()),
        'revision': value['revision'],
        'tasks': value['tasks'] == null ? undefined : ((value['tasks'] as Array<any>).map(BpnTaskToJSON)),
        'transitions': value['transitions'] == null ? undefined : ((value['transitions'] as Array<any>).map(BpnTransitionToJSON)),
        'targetEnvironments': value['targetEnvironments'] == null ? undefined : ((value['targetEnvironments'] as Array<any>).map(EnvironmentToJSON)),
    };
}
