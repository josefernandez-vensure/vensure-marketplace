import { zodResolver } from '@hookform/resolvers/zod';
import { PhrButtonBase, PhrInputText, PhrSpinner } from '@prismhr/components-react';
import { useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { readProblem } from '../../../shared/lib/problem-details';
import { ProblemMessage } from '../../../shared/lib/ProblemMessage';
import { isStale, productQuery, useProductForEditing, useRenameProduct } from '../api/product';
import { renameProductForm, type RenameProductForm as Values } from '../schemas/rename-product';

/** An edit form of a versioned resource: the reference for every form that writes what it read. */
export function RenameProductForm({ id }: { id: string }) {
  const product = useProductForEditing(id);
  if (product.isPending) return <PhrSpinner />;
  if (product.error) return <ProblemMessage problem={readProblem(product.error)} />;
  // Mounted only once the product has loaded, the form takes it as its starting values, etag included. A later
  // refetch never touches what the user is editing.
  return <RenameProductFields id={id} initial={{ name: product.data.name, etag: product.data.etag }} />;
}

function RenameProductFields({ id, initial }: { id: string; initial: Values }) {
  const queryClient = useQueryClient();
  const rename = useRenameProduct(id);
  const [changedElsewhere, setChangedElsewhere] = useState(false);
  const form = useForm<Values>({ resolver: zodResolver(renameProductForm), defaultValues: initial });

  const submit = form.handleSubmit(async (values) => {
    setChangedElsewhere(false);
    try {
      // The etag sent is the one the edited values came with, never the query's current one.
      const written = await rename.mutateAsync(values);
      form.reset({ name: values.name, etag: written.etag });
    } catch (error) {
      if (isStale(error)) {
        // Someone else saved first: show their values, with their etag, and let the user decide. Never retry.
        const latest = await queryClient.fetchQuery({ ...productQuery(id), staleTime: 0 });
        form.reset({ name: latest.name, etag: latest.etag });
        setChangedElsewhere(true);
        return;
      }
      for (const [field, messages] of Object.entries(readProblem(error).fieldErrors)) {
        if (field === 'name') form.setError('name', { message: messages.join(' ') });
      }
    }
  });

  return (
    <form onSubmit={(event) => void submit(event)} aria-label="Rename product">
      <Controller
        control={form.control}
        name="name"
        render={({ field, fieldState }) => (
          <PhrInputText
            label="Name"
            value={field.value}
            onValueChange={field.onChange}
            touched={fieldState.isTouched || Boolean(fieldState.error)}
            errors={fieldState.error ? [{ kind: 'server', message: fieldState.error.message ?? '' }] : []}
          />
        )}
      />
      {changedElsewhere && <p role="alert">Someone changed this product. Its latest values are shown; check them and save again.</p>}
      {rename.error && !isStale(rename.error) && <ProblemMessage problem={readProblem(rename.error)} />}
      <PhrButtonBase buttonConfig={{ title: 'Save', type: 'submit' }} />
    </form>
  );
}
