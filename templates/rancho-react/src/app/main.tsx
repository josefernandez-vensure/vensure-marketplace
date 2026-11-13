import React from 'react';
import ReactDOMClient from 'react-dom/client';
import singleSpaReact from 'single-spa-react';
import { configureApiClient } from '../shared/lib/api-client';
import { authorizedFetch } from '../shared/lib/identity';
import { App, type AppProps } from './providers';
import './styles';

// The application's entry: the PrismOne shell (single-spa) loads this module and calls its lifecycles.
configureApiClient(import.meta.env.VITE_API_URL, authorizedFetch);

const lifecycles = singleSpaReact<AppProps>({
  React,
  ReactDOMClient,
  rootComponent: App,
  errorBoundary: () => <p role="alert">This application could not start.</p>,
});

export const { bootstrap, mount, unmount } = lifecycles;
