import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { describe, expect, it } from 'vitest';
import type { z } from 'zod';
import { GUID, INT32, kindsOf, parameterSchemaName } from '../scripts/generate-classes.mjs';
import * as generated from '../src/generated/classes.gen';

// The generated parameter schemas carry the server's constraints: for every parameter of every operation in the committed
// document, each candidate value is refused by the generated schema exactly when the document's own constraints refuse it.
// The document is the oracle; a constraint the generator drops lets a refused value through, and this fails.

type Schema = Record<string, unknown> & { $ref?: string; enum?: unknown[] };
interface Parameter { name: string; in: string; required?: boolean; schema?: Schema }
interface Operation { operationId?: string; parameters?: Parameter[] }

const document = JSON.parse(readFileSync(join(process.cwd(), 'openapi', 'v1.json'), 'utf8')) as {
  paths: Record<string, Record<string, Operation>>;
  components?: { schemas?: Record<string, Schema> };
};

const NIL = '00000000-0000-0000-0000-000000000000';
const candidates: unknown[] = [
  '', 'x', 'Name', 'StockOnHand', 'name', NIL, '0b6f4f2e-7c3d-4a51-9e2b-6f1a2c3d4e5f', '0B6F4F2E-7C3D-4A51-9E2B-6F1A2C3D4E5F', 'not-a-guid',
  '0', '1', '50', '51', '-1', '1.5', '2147483648', '"1"',
  0, 1, 50, 51, -1, 1.5, 2147483647, 2147483648, true, null,
];

function resolve(schema: Schema): Schema {
  return schema.$ref ? resolve(document.components!.schemas![schema.$ref.split('/').pop()!]!) : schema;
}

/** Whether the document's constraints admit the value. */
function admits(raw: Schema, value: unknown): boolean {
  const schema = resolve(raw);
  const known = ['type', 'format', 'pattern', 'enum', 'minimum', 'maximum', 'minLength', 'maxLength', 'x-data-class', 'default', 'description'];
  const unknown = Object.keys(schema).filter((k) => !known.includes(k));
  if (unknown.length) throw new Error(`The oracle doesn't know ${unknown.join(', ')}: teach it and the generator together.`);
  const kinds = kindsOf(schema);
  const format = schema['format'] as string | undefined;
  const [low, high] = format === 'int32' ? INT32 : [-Infinity, Infinity];
  const min = Math.max((schema['minimum'] as number | undefined) ?? -Infinity, low);
  const max = Math.min((schema['maximum'] as number | undefined) ?? Infinity, high);
  const numeric = kinds.includes('integer') || kinds.includes('number');
  return kinds.some((kind) => {
    switch (kind) {
      case 'null': return value === null;
      case 'boolean': return typeof value === 'boolean';
      case 'integer': return Number.isInteger(value) && (value as number) >= min && (value as number) <= max;
      case 'number': return typeof value === 'number' && value >= min && value <= max;
      case 'string': {
        if (typeof value !== 'string') return false;
        if (schema.enum) return schema.enum.includes(value);
        if (format === 'uuid' && !new RegExp(GUID).test(value)) return false;
        if (typeof schema['pattern'] === 'string' && !new RegExp(schema['pattern']).test(value)) return false;
        if (typeof schema['minLength'] === 'number' && value.length < schema['minLength']) return false;
        if (typeof schema['maxLength'] === 'number' && value.length > schema['maxLength']) return false;
        return !numeric || (Number(value) >= min && Number(value) <= max);
      }
      default: throw new Error(`The oracle doesn't know type ${kind}.`);
    }
  });
}

const parameters = Object.entries(document.paths).flatMap(([path, item]) =>
  Object.entries(item).flatMap(([method, operation]) =>
    (operation.parameters ?? []).map((parameter) => ({ label: `${method.toUpperCase()} ${path} ${parameter.in} ${parameter.name}`, operation, parameter }))));

describe('the generated parameter schemas', () => {
  it.each(parameters.map((p) => [p.label, p] as const))('%s refuses what the server refuses', (_, { operation, parameter }) => {
    const exported = (generated as Record<string, unknown>)[parameterSchemaName(operation.operationId!, parameter.in)] as z.ZodObject | undefined;
    expect(exported, `${parameterSchemaName(operation.operationId!, parameter.in)} is generated`).toBeDefined();
    const schema = exported!.shape[parameter.name] as z.ZodType;
    const disagreements = candidates
      .filter((value) => schema.safeParse(value).success !== admits(parameter.schema ?? {}, value))
      .map((value) => `${JSON.stringify(value)}: the document ${admits(parameter.schema ?? {}, value) ? 'admits' : 'refuses'} it, the generated schema doesn't`);
    expect(disagreements).toEqual([]);
  });

  it('refuses the nil GUID as an id, which the server could never find', () => {
    expect(generated.zCatalogGetProductPath.safeParse({ id: NIL }).success).toBe(false);
    expect(generated.zCatalogGetProductPath.safeParse({ id: '0b6f4f2e-7c3d-4a51-9e2b-6f1a2c3d4e5f' }).success).toBe(true);
  });
});
