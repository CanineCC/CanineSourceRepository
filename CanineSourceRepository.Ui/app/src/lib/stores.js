// src/stores.js
import { writable } from 'svelte/store';

// This store will persist across navigations
export const isAppInitialized = writable(false);
