call npx @openapitools/openapi-generator-cli generate -i http://localhost:5228/swagger/engine_v1/swagger.json -g typescript-fetch -o ./BpnEngineClient --additional-properties=supportsES6=true
call rmdir .\svelte-app\src\BpnEngineClient /S /Q /D
call xcopy .\BpnEngineClient .\svelte-app\src\BpnEngineClient /Y /S
call rmdir .\BpnEngineClient /S /Q /D
