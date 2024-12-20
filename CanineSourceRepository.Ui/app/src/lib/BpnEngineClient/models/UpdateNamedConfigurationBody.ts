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
import type { Scope } from './Scope';
import {
    ScopeFromJSON,
    ScopeFromJSONTyped,
    ScopeToJSON,
} from './Scope';

/**
 * 
 * @export
 * @interface UpdateNamedConfigurationBody
 */
export interface UpdateNamedConfigurationBody {
    /**
     * 
     * @type {string}
     * @memberof UpdateNamedConfigurationBody
     */
    namedConfigurationId: string;
    /**
     * 
     * @type {string}
     * @memberof UpdateNamedConfigurationBody
     */
    name: string;
    /**
     * 
     * @type {string}
     * @memberof UpdateNamedConfigurationBody
     */
    description: string;
    /**
     * 
     * @type {Scope}
     * @memberof UpdateNamedConfigurationBody
     */
    scope: Scope;
    /**
     * 
     * @type {{ [key: string]: string; }}
     * @memberof UpdateNamedConfigurationBody
     */
    _configuration: { [key: string]: string; };
}



/**
 * Check if a given object implements the UpdateNamedConfigurationBody interface.
 */
export function instanceOfUpdateNamedConfigurationBody(value: object): value is UpdateNamedConfigurationBody {
    if (!('namedConfigurationId' in value) || value['namedConfigurationId'] === undefined) return false;
    if (!('name' in value) || value['name'] === undefined) return false;
    if (!('description' in value) || value['description'] === undefined) return false;
    if (!('scope' in value) || value['scope'] === undefined) return false;
    if (!('_configuration' in value) || value['_configuration'] === undefined) return false;
    return true;
}

export function UpdateNamedConfigurationBodyFromJSON(json: any): UpdateNamedConfigurationBody {
    return UpdateNamedConfigurationBodyFromJSONTyped(json, false);
}

export function UpdateNamedConfigurationBodyFromJSONTyped(json: any, ignoreDiscriminator: boolean): UpdateNamedConfigurationBody {
    if (json == null) {
        return json;
    }
    return {
        
        'namedConfigurationId': json['namedConfigurationId'],
        'name': json['name'],
        'description': json['description'],
        'scope': ScopeFromJSON(json['scope']),
        '_configuration': json['configuration'],
    };
}

export function UpdateNamedConfigurationBodyToJSON(value?: UpdateNamedConfigurationBody | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'namedConfigurationId': value['namedConfigurationId'],
        'name': value['name'],
        'description': value['description'],
        'scope': ScopeToJSON(value['scope']),
        'configuration': value['_configuration'],
    };
}

