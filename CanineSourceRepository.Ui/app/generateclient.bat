call rmdir .\node_modules\BpnEngineClient /S /Q
call npx @openapitools/openapi-generator-cli generate -i http://localhost:5228/swagger/engine_v1/swagger.json -g typescript-fetch  -o ./node_modules/BpnEngineClient --additional-properties=supportsES6=true,typescriptThreePlus=true,withOptionalNullable=false,nullSafeAdditionalProps=false,disallowAdditionalPropertiesIfNotPresent=true
