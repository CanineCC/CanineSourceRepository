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
import type { RecordDefinition } from './RecordDefinition';
import {
    RecordDefinitionFromJSON,
    RecordDefinitionFromJSONTyped,
    RecordDefinitionToJSON,
} from './RecordDefinition';

/**
 * 
 * @export
 * @interface ApiInputTask
 */
export interface ApiInputTask {
    /**
     * 
     * @type {string}
     * @memberof ApiInputTask
     */
    id: string;
    /**
     * 
     * @type {string}
     * @memberof ApiInputTask
     */
    name: string;
    /**
     * 
     * @type {string}
     * @memberof ApiInputTask
     */
    businessPurpose: string;
    /**
     * 
     * @type {string}
     * @memberof ApiInputTask
     */
    behavioralGoal: string;
    /**
     * 
     * @type {string}
     * @memberof ApiInputTask
     */
    input?: string | null;
    /**
     * 
     * @type {string}
     * @memberof ApiInputTask
     */
    output?: string | null;
    /**
     * 
     * @type {string}
     * @memberof ApiInputTask
     */
    serviceDependency: string;
    /**
     * 
     * @type {string}
     * @memberof ApiInputTask
     */
    namedConfiguration: string;
    /**
     * 
     * @type {Array<RecordDefinition>}
     * @memberof ApiInputTask
     */
    recordTypes: Array<RecordDefinition>;
    /**
     * 
     * @type {Array<string>}
     * @memberof ApiInputTask
     */
    validDatatypes: Array<string>;
    /**
     * 
     * @type {Array<string>}
     * @memberof ApiInputTask
     */
    accessScopes: Array<string>;
}

/**
 * Check if a given object implements the ApiInputTask interface.
 */
export function instanceOfApiInputTask(value: object): value is ApiInputTask {
    if (!('id' in value) || value['id'] === undefined) return false;
    if (!('name' in value) || value['name'] === undefined) return false;
    if (!('businessPurpose' in value) || value['businessPurpose'] === undefined) return false;
    if (!('behavioralGoal' in value) || value['behavioralGoal'] === undefined) return false;
    if (!('serviceDependency' in value) || value['serviceDependency'] === undefined) return false;
    if (!('namedConfiguration' in value) || value['namedConfiguration'] === undefined) return false;
    if (!('recordTypes' in value) || value['recordTypes'] === undefined) return false;
    if (!('validDatatypes' in value) || value['validDatatypes'] === undefined) return false;
    if (!('accessScopes' in value) || value['accessScopes'] === undefined) return false;
    return true;
}

export function ApiInputTaskFromJSON(json: any): ApiInputTask {
    return ApiInputTaskFromJSONTyped(json, false);
}

export function ApiInputTaskFromJSONTyped(json: any, ignoreDiscriminator: boolean): ApiInputTask {
    if (json == null) {
        return json;
    }
    return {
        
        'id': json['id'],
        'name': json['name'],
        'businessPurpose': json['businessPurpose'],
        'behavioralGoal': json['behavioralGoal'],
        'input': json['input'] == null ? undefined : json['input'],
        'output': json['output'] == null ? undefined : json['output'],
        'serviceDependency': json['serviceDependency'],
        'namedConfiguration': json['namedConfiguration'],
        'recordTypes': ((json['recordTypes'] as Array<any>).map(RecordDefinitionFromJSON)),
        'validDatatypes': json['validDatatypes'],
        'accessScopes': json['accessScopes'],
    };
}

export function ApiInputTaskToJSON(value?: ApiInputTask | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'id': value['id'],
        'name': value['name'],
        'businessPurpose': value['businessPurpose'],
        'behavioralGoal': value['behavioralGoal'],
        'input': value['input'],
        'output': value['output'],
        'serviceDependency': value['serviceDependency'],
        'namedConfiguration': value['namedConfiguration'],
        'recordTypes': ((value['recordTypes'] as Array<any>).map(RecordDefinitionToJSON)),
        'validDatatypes': value['validDatatypes'],
        'accessScopes': value['accessScopes'],
    };
}

