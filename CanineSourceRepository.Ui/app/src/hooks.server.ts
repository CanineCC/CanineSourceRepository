import type { Handle } from '@sveltejs/kit';

export const handle: Handle = async ({ event, resolve }) => {
    // Convert the request URL to lowercase to achieve case-insensitivity
    const originalUrl = event.url.pathname;
    const lowerCaseUrl = originalUrl.toLowerCase();

    // Set the new URL in the event
    event.url.pathname = lowerCaseUrl;

    // Call the resolve function to continue processing the request
    const response = await resolve(event);

    return response;
};
