// Writes src/generated/classes.gen.ts from the server's OpenAPI document: for every schema and every operation's
// parameters, a Zod schema whose output carries each value's data class (x-data-class) as a brand the type checker
// reads, and which refuses what the server refuses: every constraint keyword the document carries is translated, and a
// keyword the generator can't translate stops generation rather than being dropped. A value the document leaves
// unclassified is restricted. Run by `npm run generate`, after openapi-ts.
import { readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

const CLASSES = ['public', 'internal', 'confidential', 'restricted'];

/** The keywords a schema may carry: structure, the constraints translated below, and annotations that constrain nothing. */
const STRUCTURE = ['type', 'properties', 'required', 'items', 'additionalProperties', '$ref'];
const CONSTRAINTS = ['format', 'pattern', 'enum', 'minimum', 'maximum', 'minLength', 'maxLength', 'maxItems'];
const ANNOTATIONS = ['x-data-class', 'default', 'description', 'title', 'deprecated', 'readOnly', 'writeOnly', 'example', 'examples'];
const FORMATS = ['uuid', 'int32', 'int64', 'float', 'double'];

/** The shape of a GUID as the server writes and parses it; format uuid in the document. */
export const GUID = '^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$';
export const INT32 = [-2147483648, 2147483647];

/** The export name of an operation's parameter schema for one location, such as zCatalogGetProductPath. */
export function parameterSchemaName(operationId, location) {
  const ident = (text) => text.replace(/[^A-Za-z0-9_$]/g, '_');
  return `z${ident(operationId.replace(/(^|[._-])(\w)/g, (_, __, c) => c.toUpperCase()))}${location[0].toUpperCase()}${location.slice(1)}`;
}

/** The kinds a schema's values may take: its declared types, or those of its enum's values. */
export function kindsOf(schema) {
  const declared = [schema.type ?? (schema.properties ? 'object' : schema.items ? 'array' : undefined)].flat().filter(Boolean);
  if (declared.length || !schema.enum) return declared;
  return [...new Set(schema.enum.map((v) => (v === null ? 'null' : typeof v === 'number' ? 'number' : typeof v)))];
}

function refuseUntranslated(schema, where) {
  const allowed = schema.$ref ? ['$ref', ...ANNOTATIONS] : [...STRUCTURE, ...CONSTRAINTS, ...ANNOTATIONS];
  for (const key of Object.keys(schema)) {
    if (!allowed.includes(key)) {
      throw new Error(`${where}: "${key}" is a constraint generate-classes.mjs doesn't carry. Teach it, so the client's schema refuses what the server refuses.`);
    }
  }
  if (schema.format && !FORMATS.includes(schema.format)) {
    throw new Error(`${where}: format "${schema.format}" is a constraint generate-classes.mjs doesn't carry. Teach it.`);
  }
}

export function generateClasses(document) {
  const schemas = document.components?.schemas ?? {};
  const name = (ref) => ref.split('/').pop();
  const ident = (text) => text.replace(/[^A-Za-z0-9_$]/g, '_');
  const zodName = (schema) => `z${ident(schema)}`;
  const classOf = (schema) => (CLASSES.includes(schema['x-data-class']) ? schema['x-data-class'] : 'restricted');
  const regex = (pattern) => `.regex(new RegExp(${JSON.stringify(pattern)}))`;

  /** A number's bounds: the schema's own, inside its format's range. */
  function bounds(schema) {
    const [low, high] = schema.format === 'int32' ? INT32 : [-Infinity, Infinity];
    return [Math.max(schema.minimum ?? -Infinity, low), Math.min(schema.maximum ?? Infinity, high)];
  }

  function zod(schema, where) {
    refuseUntranslated(schema, where);
    if (schema.$ref) return `z.lazy(() => ${zodName(name(schema.$ref))})`;
    const types = kindsOf(schema);
    const nullable = types.includes('null');
    const kinds = types.filter((t) => t !== 'null');
    const numeric = kinds.includes('integer') || kinds.includes('number');
    const [low, high] = bounds(schema);
    const leaf = (kind) => {
      switch (kind) {
        case 'string': {
          if (schema.enum) return `z.enum([${schema.enum.filter((v) => typeof v === 'string').map((v) => JSON.stringify(v)).join(', ')}])`;
          let out = 'z.string()';
          if (schema.format === 'uuid') out += regex(GUID);
          if (schema.pattern) out += regex(schema.pattern);
          if (schema.minLength !== undefined) out += `.min(${schema.minLength})`;
          if (schema.maxLength !== undefined) out += `.max(${schema.maxLength})`;
          // A number the server also accepts as text, as in a query string, is held to the same bounds.
          if (numeric && (Number.isFinite(low) || Number.isFinite(high))) out += `.refine((v) => Number(v) >= ${low} && Number(v) <= ${high})`;
          return out;
        }
        case 'integer':
        case 'number': {
          let out = kind === 'integer' ? 'z.number().int()' : 'z.number()';
          if (Number.isFinite(low)) out += `.min(${low})`;
          if (Number.isFinite(high)) out += `.max(${high})`;
          return out;
        }
        case 'boolean': return 'z.boolean()';
        default: return undefined;
      }
    };
    let out;
    if (schema.additionalProperties && typeof schema.additionalProperties === 'object' && !schema.properties) {
      out = `z.record(z.string(), ${zod(schema.additionalProperties, `${where}{}`)})`;
    } else if (kinds.includes('object') || schema.properties) {
      const required = new Set(schema.required ?? []);
      const props = Object.entries(schema.properties ?? {}).map(([key, value]) => {
        const inner = zod(value, `${where}.${key}`);
        return `  ${JSON.stringify(key)}: ${required.has(key) ? inner : `${inner}.optional()`},`;
      });
      out = `z.object({\n${props.join('\n')}\n})`;
    } else if (kinds.includes('array')) {
      const items = zod(schema.items ?? {}, `${where}[]`);
      out = schema.maxItems ? `z.array(${items}).max(${schema.maxItems})` : `z.array(${items})`;
    } else {
      const leaves = kinds.map(leaf).filter(Boolean);
      const base = leaves.length === 0 ? 'z.unknown()' : leaves.length === 1 ? leaves[0] : `z.union([${leaves.join(', ')}])`;
      out = `classified(${JSON.stringify(classOf(schema))}, ${base})`;
    }
    return nullable ? `${out}.nullable()` : out;
  }

  const lines = [
    '// This file is auto-generated by scripts/generate-classes.mjs from openapi/v1.json. Never hand-edit it.',
    "import { z } from 'zod';",
    '',
    "export type DataClass = 'public' | 'internal' | 'confidential' | 'restricted';",
    '',
    '/** A value of a given data class. Only a schema in this file, or the client-state registry, gives a value its class. */',
    'export type Classified<T, C extends DataClass> = T & { readonly __dataClass: C };',
    '',
    'function classified<C extends DataClass, S extends z.ZodType>(dataClass: C, schema: S) {',
    '  return schema.transform((value) => value as Classified<z.output<S>, C>);',
    '}',
    '',
  ];
  for (const [schemaName, schema] of Object.entries(schemas)) {
    lines.push(`export const ${zodName(schemaName)} = ${zod(schema, `#/components/schemas/${schemaName}`)};`);
    lines.push(`export type ${ident(schemaName)}Classified = z.output<typeof ${zodName(schemaName)}>;`, '');
  }
  // The problem types the document marks user-facing (x-problem-types): only their titles are shown to a user.
  const userFacing = (document['x-problem-types'] ?? []).filter((p) => p?.userFacing === true).map((p) => JSON.stringify(p.type));
  lines.push('/** The problem types whose title a user may see; every other problem is shown generically. */');
  lines.push(`export const userFacingProblemTypes: ReadonlySet<string> = new Set([${userFacing.join(', ')}]);`, '');
  for (const [path, item] of Object.entries(document.paths ?? {})) {
    for (const [method, operation] of Object.entries(item)) {
      if (!operation?.operationId || !Array.isArray(operation.parameters)) continue;
      for (const location of ['path', 'query', 'header']) {
        const params = operation.parameters.filter((p) => p.in === location);
        if (!params.length) continue;
        const props = params.map((p) => {
          const inner = zod(p.schema ?? {}, `${method.toUpperCase()} ${path} ${location} ${p.name}`);
          return `  ${JSON.stringify(p.name)}: ${p.required ? inner : `${inner}.optional()`},`;
        });
        lines.push(`/** ${method.toUpperCase()} ${path}: its ${location} parameters. */`, `export const ${parameterSchemaName(operation.operationId, location)} = z.object({\n${props.join('\n')}\n});`, '');
      }
    }
  }
  return lines.join('\n');
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const root = fileURLToPath(new URL('..', import.meta.url));
  const document = JSON.parse(readFileSync(join(root, 'openapi/v1.json'), 'utf8'));
  writeFileSync(join(root, 'src/generated/classes.gen.ts'), generateClasses(document));
}
