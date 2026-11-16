// Types for the generator's exports that the tests read.
export declare const GUID: string;
export declare const INT32: [number, number];
export declare function parameterSchemaName(operationId: string, location: string): string;
export declare function kindsOf(schema: Record<string, unknown>): string[];
export declare function generateClasses(document: unknown): string;
