// The template's type-aware rules: they read the data class the generated schemas put on every value.
import { ESLintUtils } from '@typescript-eslint/utils';
import ts from 'typescript';

const BRAND = '__dataClass';
const NO_DATA = ts.TypeFlags.Null | ts.TypeFlags.Undefined | ts.TypeFlags.Void | ts.TypeFlags.Never
  | ts.TypeFlags.StringLiteral | ts.TypeFlags.NumberLiteral | ts.TypeFlags.BooleanLiteral | ts.TypeFlags.BigIntLiteral
  | ts.TypeFlags.EnumLiteral | ts.TypeFlags.UniqueESSymbol;
const DATA = ts.TypeFlags.String | ts.TypeFlags.Number | ts.TypeFlags.Boolean | ts.TypeFlags.BigInt
  | ts.TypeFlags.TemplateLiteral | ts.TypeFlags.StringMapping | ts.TypeFlags.Any | ts.TypeFlags.Unknown | ts.TypeFlags.ESSymbol;

function brandOf(checker, type) {
  const prop = type.getProperty(BRAND);
  if (!prop) return undefined;
  const t = checker.getTypeOfSymbol(prop);
  return t.isStringLiteral() ? t.value : 'restricted';
}

/**
 * The first value in `type` whose class the boundary refuses, or undefined. A value carries data unless it is a
 * literal, null or undefined; data without a class is unclassified, which is restricted.
 */
function refused(checker, type, allowed, { returns = false } = {}, depth = 0, seen = new Set()) {
  if (depth > 8 || seen.has(type)) return undefined;
  seen.add(type);
  const brand = brandOf(checker, type);
  if (brand) return allowed.includes(brand) ? undefined : `a ${brand} value`;
  if (type.flags & NO_DATA) return undefined;
  if (type.isUnion()) {
    for (const member of type.types) {
      const r = refused(checker, member, allowed, { returns }, depth + 1, seen);
      if (r) return r;
    }
    return undefined;
  }
  if (type.flags & DATA) return 'an unclassified value';
  if (type.isIntersection()) {
    for (const member of type.types) {
      const r = refused(checker, member, allowed, { returns }, depth + 1, seen);
      if (r) return r;
    }
    return undefined;
  }
  const signatures = type.getCallSignatures();
  if (signatures.length) {
    if (!returns) return undefined;
    for (const s of signatures) {
      const r = refused(checker, s.getReturnType(), allowed, { returns }, depth + 1, seen);
      if (r) return r;
    }
    return undefined;
  }
  if (checker.isArrayType(type) || checker.isTupleType(type)) {
    for (const arg of checker.getTypeArguments(type)) {
      const r = refused(checker, arg, allowed, { returns }, depth + 1, seen);
      if (r) return r;
    }
    return undefined;
  }
  for (const prop of checker.getPropertiesOfType(type)) {
    const declaration = prop.valueDeclaration ?? prop.declarations?.[0];
    const propType = declaration ? checker.getTypeOfSymbolAtLocation(prop, declaration) : checker.getTypeOfSymbol(prop);
    if (propType.getCallSignatures().length) continue;
    const r = refused(checker, propType, allowed, { returns }, depth + 1, seen);
    if (r) return r;
  }
  return undefined;
}

const SAFE = ['public', 'internal'];

/** The calls and places where a value leaves for a boundary, and the classes each admits. */
const BOUNDARIES = {
  url: { allowed: SAFE, name: 'a URL', calls: new Set(['navigate', 'setSearchParams', 'setSearch', 'redirect', 'generatePath', 'createSearchParams', 'pushState', 'replaceState', 'assign', 'replace']), attributes: new Set(['to', 'href']) },
  storage: { allowed: SAFE, name: 'browser storage' },
  telemetry: { allowed: SAFE, name: 'telemetry', objects: new Set(['console']) },
};

function unwrap(node) {
  // A value serialized for string-only storage is judged before serialization.
  if (node.type === 'CallExpression' && node.arguments.length && ((node.callee.type === 'MemberExpression' && node.callee.object.name === 'JSON' && node.callee.property.name === 'stringify') || (node.callee.type === 'Identifier' && node.callee.name === 'String'))) return node.arguments[0];
  return node;
}

function calleeName(node) {
  if (node.callee.type === 'Identifier') return node.callee.name;
  if (node.callee.type === 'MemberExpression' && !node.callee.computed) return node.callee.property.name;
  return undefined;
}

export const classifiedAtBoundary = {
  meta: { type: 'problem', messages: { refused: '{{what}} can\'t go into {{boundary}}: only public and internal data may, and a value takes its class only by being parsed through a generated schema or the client-state registry.' } },
  create(context) {
    const services = ESLintUtils.getParserServices(context);
    const checker = services.program.getTypeChecker();
    const typeOf = (node) => services.getTypeAtLocation(node);
    const judge = (node, boundary, options) => {
      const value = unwrap(node);
      if (value.type === 'TemplateLiteral') {
        for (const expression of value.expressions) judge(expression, boundary, options);
        return;
      }
      if (value.type === 'SpreadElement') return judge(value.argument, boundary, options);
      const what = refused(checker, typeOf(value), BOUNDARIES[boundary].allowed, options);
      if (what) context.report({ node: value, messageId: 'refused', data: { what: what[0].toUpperCase() + what.slice(1), boundary: BOUNDARIES[boundary].name } });
    };
    const objectTypeName = (node) => {
      const t = typeOf(node);
      return (t.aliasSymbol ?? t.symbol)?.getName();
    };
    return {
      JSXAttribute(node) {
        if (BOUNDARIES.url.attributes.has(node.name.name) && node.value?.type === 'JSXExpressionContainer' && node.value.expression.type !== 'JSXEmptyExpression') {
          judge(node.value.expression, 'url', { returns: true });
        }
      },
      CallExpression(node) {
        const name = calleeName(node);
        const callee = node.callee;
        if (!name) return;
        // URL: navigation, search params, and history.
        if (BOUNDARIES.url.calls.has(name)) {
          const object = callee.type === 'MemberExpression' ? objectTypeName(callee.object) : undefined;
          if (name === 'assign' || name === 'replace') { if (object !== 'Location') return; }
          if (name === 'pushState' || name === 'replaceState') { if (object !== 'History') return; node.arguments.slice(2).forEach((a) => judge(a, 'url', { returns: true })); return; }
          node.arguments.forEach((a) => judge(a, 'url', { returns: true }));
          return;
        }
        // Storage: Web Storage, the Cookie Store, IndexedDB and the Cache API.
        if (callee.type === 'MemberExpression') {
          const object = objectTypeName(callee.object);
          if (object === 'Storage' && name === 'setItem') return judge(node.arguments[1], 'storage');
          if (object === 'CookieStore' && name === 'set') return node.arguments.forEach((a) => judge(a, 'storage'));
          if (object === 'IDBObjectStore' && (name === 'put' || name === 'add')) return judge(node.arguments[0], 'storage');
          if (object === 'Cache' && name === 'put') return judge(node.arguments[1], 'storage');
          if (callee.object.type === 'Identifier' && BOUNDARIES.telemetry.objects.has(callee.object.name)) return node.arguments.forEach((a) => judge(a, 'telemetry'));
        }
        // A store persisted through zustand names what it persists, and that must be classified.
        if (name === 'persist' && node.arguments.length >= 2) {
          const options = node.arguments[1];
          const partialize = options.type === 'ObjectExpression' && options.properties.find((p) => p.type === 'Property' && p.key.name === 'partialize');
          if (!partialize) return context.report({ node: options, messageId: 'refused', data: { what: 'A store persisted without partialize', boundary: BOUNDARIES.storage.name } });
          return judge(partialize.value, 'storage', { returns: true });
        }
      },
      AssignmentExpression(node) {
        if (node.left.type !== 'MemberExpression' || node.left.computed) return;
        const object = objectTypeName(node.left.object);
        if (object === 'Document' && node.left.property.name === 'cookie') judge(node.right, 'storage');
        if (object === 'Location' && node.left.property.name === 'href') judge(node.right, 'url');
      },
    };
  },
};

function containsBrand(checker, type, depth = 0) {
  if (depth > 4) return false;
  if (brandOf(checker, type)) return true;
  if (type.isUnionOrIntersection()) return type.types.some((t) => containsBrand(checker, t, depth + 1));
  if (checker.isArrayType(type) || checker.isTupleType(type)) return checker.getTypeArguments(type).some((t) => containsBrand(checker, t, depth + 1));
  if (type.flags & ts.TypeFlags.Object) return checker.getPropertiesOfType(type).some((p) => containsBrand(checker, checker.getTypeOfSymbol(p), depth + 1));
  return false;
}

const isConst = (node) => node.typeAnnotation.type === 'TSTypeReference' && node.typeAnnotation.typeName.type === 'Identifier' && node.typeAnnotation.typeName.name === 'const';

/** A value takes its class only by being parsed: nothing asserts one, outside the generated code and the registry. */
export const noClassAssertion = {
  meta: { type: 'problem', messages: { assert: 'Parse the value through its generated schema or the client-state registry; don\'t assert a data class.' } },
  create(context) {
    const services = ESLintUtils.getParserServices(context);
    const checker = services.program.getTypeChecker();
    const check = (node) => {
      if (isConst(node)) return;
      const target = services.getTypeAtLocation(node.typeAnnotation);
      if (!containsBrand(checker, target)) return;
      const source = services.getTypeAtLocation(node.expression);
      if (!containsBrand(checker, source)) context.report({ node, messageId: 'assert' });
    };
    return { TSAsExpression: check, TSTypeAssertion: check };
  },
};

function declaredInGenerated(type, file = /[\\/]generated[\\/]types\.gen\.ts$/) {
  const symbol = type.aliasSymbol ?? type.symbol;
  return Boolean(symbol?.declarations?.some((d) => file.test(d.getSourceFile().fileName)));
}

function generatedTypes(type) {
  return type.isUnion() ? type.types.filter((t) => declaredInGenerated(t)) : declaredInGenerated(type) ? [type] : [];
}

/** A generated client's response is used as it is typed, or parsed: never asserted to another type. */
export const noResponseAssertion = {
  meta: { type: 'problem', messages: { assert: 'Don\'t assert the type of a generated response; parse it through its generated schema.' } },
  create(context) {
    const services = ESLintUtils.getParserServices(context);
    const check = (node) => {
      if (isConst(node)) return;
      if (generatedTypes(services.getTypeAtLocation(node.expression)).length) context.report({ node, messageId: 'assert' });
    };
    return { TSAsExpression: check, TSTypeAssertion: check };
  },
};

const PROBLEM_TYPES = new Set(['ProblemDetails', 'HttpValidationProblemDetails']);
const isProblem = (type) => generatedTypes(type).some((t) => PROBLEM_TYPES.has((t.aliasSymbol ?? t.symbol).getName()));

/** One parser reads problem details: nothing else reads a member of one. */
export const oneProblemParser = {
  meta: { type: 'problem', messages: { read: 'Read a problem through shared/lib/problem-details.ts, not directly.' } },
  create(context) {
    const services = ESLintUtils.getParserServices(context);
    return {
      MemberExpression(node) {
        if (isProblem(services.getTypeAtLocation(node.object))) context.report({ node, messageId: 'read' });
      },
      VariableDeclarator(node) {
        if (node.id.type === 'ObjectPattern' && node.init && isProblem(services.getTypeAtLocation(node.init))) context.report({ node, messageId: 'read' });
      },
    };
  },
};

/** A token never leaves the identity module: it exports nothing that is, holds or returns one. */
export const identityExportsNoToken = {
  meta: { type: 'problem', messages: { token: 'The identity module exports {{name}}, which exposes a token. Attach tokens in its interceptor only.' } },
  create(context) {
    const services = ESLintUtils.getParserServices(context);
    const checker = services.program.getTypeChecker();
    return {
      'Program:exit'(program) {
        const file = services.esTreeNodeToTSNodeMap.get(program);
        const module = checker.getSymbolAtLocation(file);
        if (!module) return;
        for (const exported of checker.getExportsOfModule(module)) {
          const declaration = exported.valueDeclaration ?? exported.declarations?.[0];
          const type = declaration ? checker.getTypeOfSymbolAtLocation(exported, declaration) : checker.getTypeOfSymbol(exported);
          const text = [type, ...type.getCallSignatures().map((s) => s.getReturnType())].map((t) => checker.typeToString(t)).join(' ');
          if (/token/i.test(exported.getName()) || /\b(AccessToken|IDToken|RefreshToken|AbstractToken|Tokens|TokensResult|RenewedTokens)\b/.test(text)) {
            context.report({ node: program, messageId: 'token', data: { name: exported.getName() } });
          }
        }
      },
    };
  },
};
