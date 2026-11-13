// The checks lint can't make. Run with `npm run check`; CI runs it on every pull request.
import { execFileSync } from 'node:child_process';
import { mkdtempSync, readFileSync, readdirSync, rmSync, statSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('..', import.meta.url));
const read = (path) => readFileSync(join(root, path), 'utf8');
const failures = [];
const fail = (check, message) => failures.push(`${check}: ${message}`);

// Every feature is declared in src/features/REGISTRY.md, and every declared feature exists.
{
  const folders = readdirSync(join(root, 'src/features'), { withFileTypes: true }).filter((e) => e.isDirectory()).map((e) => e.name);
  const declared = [...read('src/features/REGISTRY.md').matchAll(/^- `([^`]+)`: .+$/gm)].map((m) => m[1]);
  for (const f of folders.filter((f) => !declared.includes(f))) fail('registry', `src/features/${f} isn't declared in src/features/REGISTRY.md`);
  for (const f of declared.filter((f) => !folders.includes(f))) fail('registry', `src/features/REGISTRY.md declares ${f}, which has no folder`);
}

// The global stylesheet imports the UI library's styles and the layout partial, nothing else; the partial never
// styles a UI-library component (its phr- elements and classes, or Syncfusion's e- classes).
{
  const imports = [...read('src/app/styles.css').matchAll(/@import\s+(?:url\()?['"]([^'"]+)['"]/g)].map((m) => m[1]);
  const allowed = ['@prismhr/components-react/styles.css', './layout.css'];
  for (const i of imports.filter((i) => !allowed.includes(i))) fail('styles', `src/app/styles.css imports ${i}; it imports only ${allowed.join(' and ')}`);
  const rules = read('src/app/styles.css').replace(/@import[^;]+;/g, '').replace(/\/\*[\s\S]*?\*\//g, '').trim();
  if (rules) fail('styles', 'src/app/styles.css holds rules of its own; layout goes in src/app/layout.css');
  const selectors = read('src/app/layout.css').replace(/\/\*[\s\S]*?\*\//g, '').match(/[^{}]+(?=\{)/g) ?? [];
  for (const s of selectors.filter((s) => /(^|[\s,>+~(])\.?(phr-|e-)[\w-]*/.test(s))) fail('styles', `src/app/layout.css styles a UI-library component: ${s.trim()}`);
}

// A UI-class package is a dependency only if the UI library depends on it (or it is Syncfusion's grid, which the UI
// library's grid components host): the application declares no presentation package of its own.
{
  const manifest = JSON.parse(read('package.json'));
  const lock = JSON.parse(read('package-lock.json'));
  const closure = new Set(['@prismhr/components-react', '@syncfusion/ej2-react-grids']);
  const queue = ['@prismhr/components-react'];
  while (queue.length) {
    const name = queue.pop();
    const entry = lock.packages[`node_modules/${name}`];
    for (const dep of Object.keys({ ...entry?.dependencies, ...entry?.peerDependencies })) {
      if (!closure.has(dep)) { closure.add(dep); queue.push(dep); }
    }
  }
  const uiClass = /^(@prismhr\/|@syncfusion\/|@tiptap\/|@dnd-kit\/|@mui\/|@emotion\/|@chakra-ui\/|@radix-ui\/|@headlessui\/|@fortawesome\/)|^(react-imask|tinycolor2|styled-components|antd|bootstrap|react-bootstrap|tailwindcss|clsx|classnames|framer-motion|motion|lucide-react|react-icons|.*-icons?)$|css|style|theme|animat/;
  for (const dep of Object.keys({ ...manifest.dependencies, ...manifest.devDependencies })) {
    if (uiClass.test(dep) && !closure.has(dep)) fail('ui-dependencies', `${dep} is a presentation package the UI library doesn't depend on`);
  }
}

// TypeScript stays strict: the configuration the build uses has strict on and no strict-family option turned off.
{
  const config = JSON.parse(execFileSync(process.execPath, [join(root, 'node_modules/typescript/bin/tsc'), '--showConfig', '-p', join(root, 'tsconfig.json')], { encoding: 'utf8' }));
  const options = config.compilerOptions ?? {};
  if (options.strict !== true) fail('typescript', 'tsconfig.json must set "strict": true');
  const family = ['noImplicitAny', 'noImplicitThis', 'alwaysStrict', 'strictNullChecks', 'strictFunctionTypes', 'strictBindCallApply', 'strictPropertyInitialization', 'strictBuiltinIteratorReturn', 'useUnknownInCatchVariables'];
  for (const option of family.filter((o) => options[o] === false)) fail('typescript', `tsconfig.json turns off ${option}`);
}

// src/generated/ is exactly what the generator writes from the committed OpenAPI document: never edited by hand.
{
  const scratch = mkdtempSync(join(tmpdir(), 'rancho-generated-'));
  try {
    const { createClient } = await import('@hey-api/openapi-ts');
    const config = await (await import('../openapi-ts.config.js')).default;
    await createClient({ ...config, input: join(root, 'openapi/v1.json'), output: { ...config.output, path: scratch }, logs: { level: 'silent' } });
    const { generateClasses } = await import('./generate-classes.mjs');
    writeFileSync(join(scratch, 'classes.gen.ts'), generateClasses(JSON.parse(read('openapi/v1.json'))));
    const files = (dir) => readdirSync(dir).flatMap((name) => statSync(join(dir, name)).isDirectory() ? files(join(dir, name)) : [join(dir, name)]);
    const text = (f) => readFileSync(f, 'utf8').replace(/\r\n/g, '\n');
    const expected = new Map(files(scratch).map((f) => [relative(scratch, f), text(f)]));
    const actual = new Map(files(join(root, 'src/generated')).map((f) => [relative(join(root, 'src/generated'), f), text(f)]));
    for (const [file, text] of expected) {
      if (!actual.has(file)) fail('generated', `src/generated/${file} is missing; run npm run generate`);
      else if (actual.get(file) !== text) fail('generated', `src/generated/${file} differs from what the generator writes; run npm run generate`);
    }
    for (const file of actual.keys()) if (!expected.has(file)) fail('generated', `src/generated/${file} isn't generated`);
  } finally {
    rmSync(scratch, { recursive: true, force: true });
  }
}

if (failures.length) {
  console.error(failures.join('\n'));
  process.exit(1);
}
console.log('All checks pass.');
