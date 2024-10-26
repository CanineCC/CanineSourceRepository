import adapter from '@sveltejs/adapter-auto';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// Consult https://svelte.dev/docs/kit/integrations
	// for more information about preprocessors
	preprocess: vitePreprocess(),

	kit: {
		// adapter-auto only supports some environments, see https://svelte.dev/docs/kit/adapter-auto for a list.
		// If your environment is not supported, or you settled on a specific environment, switch out the adapter.
		// See https://svelte.dev/docs/kit/adapters for more information about adapters.
		adapter: adapter(),

		alias: {
			// this will match a file
			'@': 'src',
			'signalRService': 'src/lib/signalRService',
			'BpnEngineClient': 'src/lib/BpnEngineClient',
			'lib': 'src/lib',
			'components': 'src/components',

			// this will match a directory and its contents
			// (`my-directory/x` resolves to `path/to/my-directory/x`)

			// an alias ending /* will only match
			// the contents of a directory, not the directory itself
			//'my-directory/*': 'path/to/my-directory/*'
		}
	}
};

export default config;
