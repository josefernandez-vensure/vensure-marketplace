import js from '@eslint/js';
import comments from '@eslint-community/eslint-plugin-eslint-comments';
import boundaries from 'eslint-plugin-boundaries';
import reactHooks from 'eslint-plugin-react-hooks';
import tseslint from 'typescript-eslint';
import rancho from './eslint/rancho-plugin.js';

// The project's lists: what the checks below name. Change them here, never by suppressing a rule.
const IDENTITY_MODULE = 'src/shared/lib/identity.tsx';
const STYLES_MODULE = 'src/app/styles.ts';
const IDENTITY_PACKAGES = ['@vensure-devops-qa/vensureone-iam-react', 'jwt-decode'];
const PROBLEM_PARSER = 'src/shared/lib/problem-details.ts';
const CLIENT_STATE_REGISTRY = 'src/shared/lib/client-state.ts';
// Claims that carry another credential inside a token: never read.
const NESTED_CREDENTIAL_CLAIMS = /^(idp_access_token|idp_refresh_token|access_token|refresh_token)$/;
const IDENTITY_STORAGE_KEYS = /^prismone-(iam-storage|cache-storage|iam-original-uri)$/;
const OTHER_FORM_LIBRARIES = ['formik', 'react-final-form', 'final-form', '@tanstack/react-form', 'react-hook-form-mui', '@rjsf/core'];
// UI-class packages: the UI library's own dependencies, and every other presentation package. The application imports
// only the UI library, and Syncfusion's grid, which the UI library's grid components host.
const UI_CLASS_PACKAGES = [
  '@syncfusion/*', '!@syncfusion/ej2-react-grids', '@tiptap/*', '@dnd-kit/*', 'react-imask', 'tinycolor2', '@azure/msal-*',
  '@mui/*', '@emotion/*', 'styled-components', 'antd', '@chakra-ui/*', '@radix-ui/*', '@headlessui/*', 'bootstrap',
  'react-bootstrap', 'tailwindcss', 'clsx', 'classnames', 'framer-motion', 'motion', 'lucide-react', 'react-icons',
  '@fortawesome/*',
];

const restrictedImports = ({ identity = false, stores = false, styles = false, shared = false, components = false } = {}) => [
  'error',
  {
    paths: [
      { name: 'react-router-dom', message: 'Routing is react-router. react-router-dom is installed only for the identity library.' },
      ...(identity ? [] : IDENTITY_PACKAGES.map((name) => ({ name, message: `Only ${IDENTITY_MODULE} uses the identity library.` }))),
      ...OTHER_FORM_LIBRARIES.map((name) => ({ name, message: 'Forms are React Hook Form with Zod.' })),
      { name: '@prismhr/components-react', importNames: ['Validators', 'runValidators', 'ValidatorFn'], message: 'Validate with the form\'s Zod schema, not the UI library\'s validators.' },
      ...(stores ? [] : [
        { name: 'zustand', importNames: ['create', 'createStore'], message: 'A store lives in a feature\'s stores/ or in shared/stores/.' },
        { name: 'zustand/vanilla', importNames: ['createStore'], message: 'A store lives in a feature\'s stores/ or in shared/stores/.' },
        { name: 'zustand/traditional', importNames: ['createWithEqualityFn'], message: 'A store lives in a feature\'s stores/ or in shared/stores/.' },
      ]),
      ...(shared ? [{ name: '@prismhr/components-react', message: 'shared/ uses no UI library: features and app/ use it directly.' }] : []),
    ],
    patterns: [
      { group: UI_CLASS_PACKAGES, message: 'Present through the UI library, not another presentation package.' },
      ...(styles ? [] : [{ group: ['*.css', '*.scss', '*.sass', '*.less'], message: `Stylesheets enter only through ${STYLES_MODULE}.` }]),
      ...(shared ? [{ group: ['@syncfusion/*'], message: 'shared/ uses no UI library.' }] : []),
      ...(components ? [{ group: ['**/generated/*', '**/generated'], allowTypeImports: true, message: 'A component reaches the server through a hook in its feature\'s api/.' }] : []),
    ],
  },
];

const AUTHORIZATION = [
  { selector: 'Property[key.name=/^authorization$/i]', message: 'Only the identity module\'s interceptor attaches the token.' },
  { selector: 'Property[key.value=/^authorization$/i]', message: 'Only the identity module\'s interceptor attaches the token.' },
  { selector: 'CallExpression[callee.property.name=/^(set|append)$/][arguments.0.value=/^authorization$/i]', message: 'Only the identity module\'s interceptor attaches the token.' },
];
const IDENTITY_STORE = [
  { selector: `Literal[value=${IDENTITY_STORAGE_KEYS}]`, message: 'The identity library\'s session store is its own: don\'t read or write it.' },
  { selector: 'CallExpression[callee.object.name=/^(localStorage|sessionStorage)$/][callee.property.name="clear"]', message: 'Clear only what this application wrote; the identity library\'s session store is shared.' },
];
const PAGING = [
  { selector: ':matches(ForStatement, ForInStatement, ForOfStatement, WhileStatement, DoWhileStatement) CallExpression[callee.property.name="fetchNextPage"]', message: 'Don\'t page through a whole collection in a loop.' },
  { selector: ':matches(ForStatement, ForInStatement, ForOfStatement, WhileStatement, DoWhileStatement) CallExpression[callee.name="fetchNextPage"]', message: 'Don\'t page through a whole collection in a loop.' },
];
const MOCKS = [
  { selector: 'CallExpression[callee.object.name="vi"][callee.property.name=/^(mock|doMock)$/][arguments.0.value=/generated/]', message: 'Mock the network with MSW, never the generated client.' },
  { selector: 'CallExpression[callee.object.name="vi"][callee.property.name=/^(mock|doMock)$/] > ImportExpression[source.value=/generated/]', message: 'Mock the network with MSW, never the generated client.' },
];
const NESTED_CREDENTIALS = [
  { selector: `MemberExpression[property.name=${NESTED_CREDENTIAL_CLAIMS}]`, message: 'Never read a credential nested inside a token.' },
  { selector: `MemberExpression[property.value=${NESTED_CREDENTIAL_CLAIMS}]`, message: 'Never read a credential nested inside a token.' },
];
const restrictedSyntax = ({ identity = false } = {}) => ['error', ...(identity ? [] : [...AUTHORIZATION, ...IDENTITY_STORE]), ...NESTED_CREDENTIALS, ...PAGING, ...MOCKS];

const FETCH = { message: 'Reach the server through the generated client, in a feature\'s api/.' };
const restrictedGlobals = ['error', { name: 'fetch', ...FETCH }, { name: 'XMLHttpRequest', ...FETCH }];
const restrictedProperties = ['error', ...['window', 'globalThis', 'self'].map((object) => ({ object, property: 'fetch', ...FETCH }))];

export default tseslint.config(
  { ignores: ['src/generated/**', 'dist/**', 'coverage/**', 'node_modules/**'] },
  {
    files: ['src/**/*.{ts,tsx}', 'tests/**/*.{ts,tsx}'],
    extends: [js.configs.recommended, ...tseslint.configs.recommendedTypeChecked],
    languageOptions: { parserOptions: { projectService: true, tsconfigRootDir: import.meta.dirname } },
    plugins: { boundaries, 'eslint-comments': comments, 'react-hooks': reactHooks, rancho },
    settings: {
      'boundaries/elements': [
        { type: 'app', pattern: 'src/app' },
        { type: 'feature', pattern: 'src/features/*', capture: ['feature'] },
        { type: 'shared', pattern: 'src/shared' },
        { type: 'generated', pattern: 'src/generated' },
      ],
      'import/resolver': { typescript: { project: './tsconfig.json' } },
    },
    linterOptions: { reportUnusedDisableDirectives: 'error' },
    rules: {
      ...reactHooks.configs.recommended.rules,
      // TypeScript stays strict: no any, and no suppression but a @ts-expect-error that names its issue.
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/ban-ts-comment': ['error', {
        'ts-ignore': true,
        'ts-nocheck': true,
        'ts-check': false,
        'ts-expect-error': { descriptionFormat: '^: .*(#\\d+|[A-Z][A-Z0-9]+-\\d+|https?://\\S+)' },
      }],
      // A lint suppression names its rules and says why.
      'eslint-comments/no-unlimited-disable': 'error',
      'eslint-comments/require-description': ['error', { ignore: ['eslint-enable', 'eslint-env', 'exported', 'global', 'globals'] }],
      // Dependencies flow shared -> features -> app; a feature never imports another.
      'boundaries/dependencies': ['error', {
        default: 'disallow',
        policies: [
          { from: { element: { type: 'app' } }, allow: { to: { element: { types: ['app', 'feature', 'shared', 'generated'] } } } },
          { from: { element: { type: 'feature' } }, allow: { to: { element: { types: ['shared', 'generated'] } } } },
          { from: { element: { type: 'feature' } }, allow: { to: { element: { type: 'feature', captured: { feature: '{{ from.element.captured.feature }}' } } } } },
          { from: { element: { type: 'shared' } }, allow: { to: { element: { types: ['shared', 'generated'] } } } },
          { from: { element: { type: 'generated' } }, allow: { to: { element: { type: 'generated' } } } },
        ],
      }],
      'no-restricted-imports': restrictedImports(),
      'no-restricted-syntax': restrictedSyntax(),
      'no-restricted-globals': restrictedGlobals,
      'no-restricted-properties': restrictedProperties,
      'rancho/no-fetch-in-effect': 'error',
      'rancho/query-key-from-factory': 'error',
      'rancho/no-static-style': 'error',
      'rancho/no-ui-reexport': 'error',
      'rancho/no-paging-to-the-end': 'error',
      // Data classes: what reaches the URL, browser storage or telemetry is public or internal, by the generated schemas.
      'rancho/classified-at-boundary': 'error',
      'rancho/no-class-assertion': 'error',
      'rancho/no-response-assertion': 'error',
      'rancho/one-problem-parser': 'error',
    },
  },
  // A flat-config block replaces a rule's options for the files it matches, so each scope restates its whole list.
  { files: ['src/features/*/stores/**', 'src/shared/stores/**'], rules: { 'no-restricted-imports': restrictedImports({ stores: true }) } },
  { files: ['src/shared/**'], rules: { 'no-restricted-imports': restrictedImports({ shared: true }) } },
  { files: ['src/shared/stores/**'], rules: { 'no-restricted-imports': restrictedImports({ shared: true, stores: true }) } },
  { files: ['src/features/*/components/**'], rules: { 'no-restricted-imports': restrictedImports({ components: true }) } },
  { files: [STYLES_MODULE], rules: { 'no-restricted-imports': restrictedImports({ styles: true }) } },
  { files: [IDENTITY_MODULE], rules: { 'no-restricted-imports': restrictedImports({ identity: true, shared: true }), 'no-restricted-syntax': restrictedSyntax({ identity: true }) } },
  { files: [PROBLEM_PARSER], rules: { 'rancho/one-problem-parser': 'off' } },
  { files: [CLIENT_STATE_REGISTRY], rules: { 'rancho/no-class-assertion': 'off' } },
  // A token never leaves the identity module, and nothing reads a credential nested inside one.
  { files: [IDENTITY_MODULE], rules: { 'rancho/identity-exports-no-token': 'error' } },
  // Only the generated client and the identity module's interceptor call fetch.
  { files: [IDENTITY_MODULE], rules: { 'no-restricted-globals': 'off', 'no-restricted-properties': 'off' } },
  // The tests' client uses plain fetch, against MSW, in place of the identity module's.
  { files: ['tests/setup.ts'], rules: { 'no-restricted-globals': 'off' } },
);
