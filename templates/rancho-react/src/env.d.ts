/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL: string;
  readonly VITE_ENTERPRISE_SECURITY_URL: string;
  readonly VITE_IAM_ISSUER: string;
  readonly VITE_IAM_ORG_ID: string;
  readonly VITE_IAM_DESTINATION_APP: string;
  readonly VITE_IAM_SERVICE_NAME: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
