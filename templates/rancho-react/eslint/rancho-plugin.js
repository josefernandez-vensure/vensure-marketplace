import { classifiedAtBoundary, identityExportsNoToken, noClassAssertion, noResponseAssertion, oneProblemParser } from './rancho-typed.js';

// The template's own lint rules: the checks no off-the-shelf rule makes.

const UI_PACKAGES = /^(@prismhr\/components-react|@syncfusion\/)/;

function isGeneratedSource(source) {
  return /(^|\/)generated(\/|$)/.test(source);
}

function importedFrom(scope, name) {
  for (let s = scope; s; s = s.upper) {
    const variable = s.set.get(name);
    if (!variable) continue;
    const def = variable.defs[0];
    return def && def.type === 'ImportBinding' ? def.parent.source.value : undefined;
  }
  return undefined;
}

/** Server data is loaded through TanStack Query, never by an effect. */
const noFetchInEffect = {
  meta: { type: 'problem', messages: { effect: 'Load server data through TanStack Query, not inside {{hook}}.' } },
  create(context) {
    const effects = [];
    return {
      CallExpression(node) {
        const callee = node.callee;
        const name = callee.type === 'Identifier' ? callee.name : callee.type === 'MemberExpression' ? callee.property.name : undefined;
        if (name === 'useEffect' || name === 'useLayoutEffect') {
          effects.push({ node, hook: name });
          return;
        }
        const inside = effects.find((e) => e.node.arguments[0] && node.range[0] >= e.node.arguments[0].range[0] && node.range[1] <= e.node.arguments[0].range[1]);
        if (!inside || callee.type !== 'Identifier') return;
        const source = importedFrom(context.sourceCode.getScope(node), callee.name);
        if (callee.name === 'fetch' || (source && isGeneratedSource(source))) {
          context.report({ node, messageId: 'effect', data: { hook: inside.hook } });
        }
      },
      'CallExpression:exit'(node) {
        if (effects.length && effects[effects.length - 1].node === node) effects.pop();
      },
    };
  },
};

const KEY_METHODS = new Set(['getQueryData', 'setQueryData', 'getQueryState', 'ensureQueryData', 'invalidateQueries', 'refetchQueries', 'removeQueries', 'resetQueries', 'cancelQueries', 'setQueriesData', 'getQueriesData']);

/** A query key comes from its resource's key factory, a module named <resource>-keys.ts. */
const queryKeyFromFactory = {
  meta: { type: 'problem', messages: { literal: 'Take this query key from the resource\'s key factory (a *-keys.ts module), not an array literal.' } },
  create(context) {
    if (/-keys\.ts$/.test(context.filename)) return {};
    return {
      'Property[key.name="queryKey"] > ArrayExpression.value'(node) {
        context.report({ node, messageId: 'literal' });
      },
      CallExpression(node) {
        const callee = node.callee;
        if (callee.type === 'MemberExpression' && KEY_METHODS.has(callee.property.name) && node.arguments[0]?.type === 'ArrayExpression') {
          context.report({ node: node.arguments[0], messageId: 'literal' });
        }
      },
    };
  },
};

function isConstant(node) {
  if (node.type === 'Literal') return true;
  if (node.type === 'TemplateLiteral') return node.expressions.length === 0;
  if (node.type === 'UnaryExpression') return isConstant(node.argument);
  return false;
}

/** Styling is the UI library's: no static style object. A computed value is allowed. */
const noStaticStyle = {
  meta: { type: 'problem', messages: { style: 'Style with the UI library or the layout partial, not a static style object.' } },
  create(context) {
    return {
      'JSXAttribute[name.name="style"] > JSXExpressionContainer > ObjectExpression'(node) {
        const values = node.properties.filter((p) => p.type === 'Property').map((p) => p.value);
        if (values.length === node.properties.length && values.every(isConstant)) context.report({ node, messageId: 'style' });
      },
    };
  },
};

/** There is no second design system: nothing re-exports a UI-library component. */
const noUiReexport = {
  meta: { type: 'problem', messages: { reexport: 'Use the UI library directly; don\'t re-export it.' } },
  create(context) {
    return {
      'ExportNamedDeclaration[source], ExportAllDeclaration'(node) {
        if (UI_PACKAGES.test(node.source.value)) context.report({ node, messageId: 'reexport' });
      },
      'ExportNamedDeclaration[source=null] ExportSpecifier'(node) {
        if (node.exportKind === 'type' || node.parent.exportKind === 'type') return;
        const source = importedFrom(context.sourceCode.getScope(node), node.local.name);
        if (source && UI_PACKAGES.test(source)) context.report({ node, messageId: 'reexport' });
      },
    };
  },
};

/** A list never pages itself to the end: no function that calls fetchNextPage and then itself. */
const noPagingToTheEnd = {
  meta: { type: 'problem', messages: { recurse: 'Don\'t page through a whole collection; let the user page, or have the server compute it.' } },
  create(context) {
    const stack = [];
    const enter = (node) => stack.push({ node, name: node.id?.name ?? (node.parent.type === 'VariableDeclarator' ? node.parent.id.name : undefined), pages: false, self: false });
    const exit = () => {
      const f = stack.pop();
      if (f.pages && f.self) context.report({ node: f.node, messageId: 'recurse' });
    };
    return {
      FunctionDeclaration: enter,
      FunctionExpression: enter,
      ArrowFunctionExpression: enter,
      'FunctionDeclaration:exit': exit,
      'FunctionExpression:exit': exit,
      'ArrowFunctionExpression:exit': exit,
      CallExpression(node) {
        const f = stack[stack.length - 1];
        if (!f) return;
        const callee = node.callee;
        const name = callee.type === 'Identifier' ? callee.name : callee.type === 'MemberExpression' ? callee.property.name : undefined;
        if (name === 'fetchNextPage') f.pages = true;
        if (name && name === f.name) f.self = true;
      },
    };
  },
};

export default {
  meta: { name: 'rancho' },
  rules: {
    'no-fetch-in-effect': noFetchInEffect,
    'query-key-from-factory': queryKeyFromFactory,
    'no-static-style': noStaticStyle,
    'no-ui-reexport': noUiReexport,
    'no-paging-to-the-end': noPagingToTheEnd,
    'classified-at-boundary': classifiedAtBoundary,
    'no-class-assertion': noClassAssertion,
    'no-response-assertion': noResponseAssertion,
    'one-problem-parser': oneProblemParser,
    'identity-exports-no-token': identityExportsNoToken,
  },
};
