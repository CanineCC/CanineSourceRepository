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
import type { FeatureComponentDiagram } from './FeatureComponentDiagram';
import {
    FeatureComponentDiagramFromJSON,
    FeatureComponentDiagramFromJSONTyped,
    FeatureComponentDiagramToJSON,
} from './FeatureComponentDiagram';
import type { BpnTransition } from './BpnTransition';
import {
    BpnTransitionFromJSON,
    BpnTransitionFromJSONTyped,
    BpnTransitionToJSON,
} from './BpnTransition';
import type { BpnTask } from './BpnTask';
import {
    BpnTaskFromJSON,
    BpnTaskFromJSONTyped,
    BpnTaskToJSON,
} from './BpnTask';

/**
 * 
 * @export
 * @interface BpnDraftFeature
 */
export interface BpnDraftFeature {
    /**
     * 
     * @type {string}
     * @memberof BpnDraftFeature
     */
    id: string;
    /**
     * 
     * @type {FeatureComponentDiagram}
     * @memberof BpnDraftFeature
     */
    componentDiagram: FeatureComponentDiagram;
    /**
     * 
     * @type {string}
     * @memberof BpnDraftFeature
     */
    name: string;
    /**
     * 
     * @type {string}
     * @memberof BpnDraftFeature
     */
    objective: string;
    /**
     * 
     * @type {string}
     * @memberof BpnDraftFeature
     */
    flowOverview: string;
    /**
     * 
     * @type {Array<BpnTask>}
     * @memberof BpnDraftFeature
     */
    tasks: Array<BpnTask>;
    /**
     * 
     * @type {Array<BpnTransition>}
     * @memberof BpnDraftFeature
     */
    transitions: Array<BpnTransition>;
}

/**
 * Check if a given object implements the BpnDraftFeature interface.
 */
export function instanceOfBpnDraftFeature(value: object): value is BpnDraftFeature {
    if (!('id' in value) || value['id'] === undefined) return false;
    if (!('componentDiagram' in value) || value['componentDiagram'] === undefined) return false;
    if (!('name' in value) || value['name'] === undefined) return false;
    if (!('objective' in value) || value['objective'] === undefined) return false;
    if (!('flowOverview' in value) || value['flowOverview'] === undefined) return false;
    if (!('tasks' in value) || value['tasks'] === undefined) return false;
    if (!('transitions' in value) || value['transitions'] === undefined) return false;
    return true;
}

export function BpnDraftFeatureFromJSON(json: any): BpnDraftFeature {
    return BpnDraftFeatureFromJSONTyped(json, false);
}

export function BpnDraftFeatureFromJSONTyped(json: any, ignoreDiscriminator: boolean): BpnDraftFeature {
    if (json == null) {
        return json;
    }
    return {
        
        'id': json['id'],
        'componentDiagram': FeatureComponentDiagramFromJSON(json['componentDiagram']),
        'name': json['name'],
        'objective': json['objective'],
        'flowOverview': json['flowOverview'],
        'tasks': ((json['tasks'] as Array<any>).map(BpnTaskFromJSON)),
        'transitions': ((json['transitions'] as Array<any>).map(BpnTransitionFromJSON)),
    };
}

export function BpnDraftFeatureToJSON(value?: BpnDraftFeature | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'id': value['id'],
        'componentDiagram': FeatureComponentDiagramToJSON(value['componentDiagram']),
        'name': value['name'],
        'objective': value['objective'],
        'flowOverview': value['flowOverview'],
        'tasks': ((value['tasks'] as Array<any>).map(BpnTaskToJSON)),
        'transitions': ((value['transitions'] as Array<any>).map(BpnTransitionToJSON)),
    };
}

