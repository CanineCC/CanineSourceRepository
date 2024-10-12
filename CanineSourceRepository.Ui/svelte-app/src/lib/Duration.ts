

export function formatDate(date: Date | undefined | null, now: Date): string {
    if (!date) return '';
    const diff = now.getTime() - date.getTime();
    const seconds = Math.floor(diff / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);
    const years = Math.floor(days / 365);

    if (seconds < 60) return `${seconds} seconds ago`;
    if (minutes < 60) return `${minutes} minutes ago`;
    if (hours < 24) return `${hours} hours ago`;
    if (days < 365) return `${days} days ago`;
    return `${years} years ago`;
}
export function formatDurationShort(ms: number): string {
    // Define time units in milliseconds
    if (ms === 0) return '';
    const units = [
        { name: 'Y', factor: 365 * 24 * 60 * 60 * 1000 },
        { name: 'M', factor: 30 * 24 * 60 * 60 * 1000 },
        { name: 'd', factor: 24 * 60 * 60 * 1000 },
        { name: 'h', factor: 60 * 60 * 1000 },
        { name: 'm', factor: 60 * 1000 },
        { name: 's', factor: 1000 },
        { name: 'ms', factor: 1 },
        { name: 'μs', factor: 1e-3 },
        { name: 'ns', factor: 1e-6 }
    ];

    for (const unit of units) {
        if (ms >= unit.factor || unit.name === 'ns') {
            const value = ms / unit.factor;
            const roundedValue = Math.round(value);
            const prefix = Math.abs(roundedValue - value) > 0 ? "~" : "";
            return `${prefix}${roundedValue} ${unit.name}`;
        }
    }
    return "0 ms"; // fallback in case ms is exactly 0
}
export function formatDurationLong(ms: number): string {
    if (ms === 0) return '';
    const units = [
        { name: 'year', plural: 'years', factor: 365 * 24 * 60 * 60 * 1000 },
        { name: 'month', plural: 'months', factor: 30 * 24 * 60 * 60 * 1000 },
        { name: 'day', plural: 'days', factor: 24 * 60 * 60 * 1000 },
        { name: 'hour', plural: 'hours', factor: 60 * 60 * 1000 },
        { name: 'minute', plural: 'minutes', factor: 60 * 1000 },
        { name: 'second', plural: 'seconds', factor: 1000 },
        { name: 'millisecond', plural: 'milliseconds', factor: 1 },
        { name: 'microsecond', plural: 'microseconds', factor: 1e-3 },
        { name: 'nanosecond', plural: 'nanoseconds', factor: 1e-6 }
    ];

    let result = [];
    let remaining = ms;

    for (const unit of units) {
        const value = Math.floor(remaining / unit.factor);
        if (value > 0) {
            result.push(`${value} ${value > 1 ? unit.plural : unit.name}`);
            remaining -= value * unit.factor;
        }
        if (result.length === 2) break; // Stop after two most significant parts
    }

    return result.join(" and ") || "0 milliseconds";
}

/*
// Example usage:
console.log(formatDurationLong(86401.5)); // "1 minute and 26 seconds"


// Example usage:
console.log(formatDurationShort(0.1)); // "100 μs"
console.log(formatDurationShort(1.0)); // "1 ms"
console.log(formatDurationShort(1000)); // "1 s"
console.log(formatDurationShort(1500)); // "~2 s"
console.log(formatDurationShort(1.5)); // "~2 ms"

console.log(formatDurationLong(1.5)); // "1 millisecond and 500 microseconds"
console.log(formatDurationLong(86401.5)); // "1 day and 1 second"
*/