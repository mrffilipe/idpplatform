/**
 * Guia e template para ConfigJson do IdP tipo Firebase.
 * O backend usa: projectId + webApiKey (login Google no browser) e serviceAccount (validar idToken no servidor).
 */

/** Exemplo formatado — substitua pelos valores reais do seu projeto. */
export const FIREBASE_IDP_CONFIG_EXAMPLE = `{
  "projectId": "meu-projeto-firebase",
  "webApiKey": "AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
  "authDomain": "meu-projeto-firebase.firebaseapp.com",
  "serviceAccount": {
    "type": "service_account",
    "project_id": "meu-projeto-firebase",
    "private_key_id": "abc123...",
    "private_key": "-----BEGIN PRIVATE KEY-----\\n...\\n-----END PRIVATE KEY-----\\n",
    "client_email": "firebase-adminsdk-xxxxx@meu-projeto-firebase.iam.gserviceaccount.com",
    "client_id": "123456789",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token",
    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
    "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-xxxxx%40meu-projeto-firebase.iam.gserviceaccount.com",
    "universe_domain": "googleapis.com"
  }
}`

export const FIREBASE_IDP_FIELD_ROWS: Array<{
  field: string
  where: string
  notes: string
}> = [
  {
    field: 'projectId',
    where: 'Firebase Console → ⚙️ Configurações do projeto → Geral → ID do projeto',
    notes: 'Texto curto (ex.: meu-app-123). Não é o número do projeto.',
  },
  {
    field: 'webApiKey',
    where: 'Mesma tela → Chave da API da Web (Web API Key)',
    notes: 'Começa com AIzaSy…. Usada só na página de login (/account/login) para o popup do Google.',
  },
  {
    field: 'authDomain',
    where: 'Ao registrar um app Web: firebaseConfig → authDomain (ou {projectId}.firebaseapp.com)',
    notes:
      'Obrigatório para o popup Google no login. Se omitir no JSON, a API usa {projectId}.firebaseapp.com automaticamente.',
  },
  {
    field: 'serviceAccount',
    where:
      'Configurações do projeto → Contas de serviço → Firebase Admin SDK → Gerar nova chave privada (.json)',
    notes:
      'Cole aqui o conteúdo INTEIRO do arquivo .json baixado (objeto aninhado). O servidor usa isso para validar o idToken do Google.',
  },
]

export const FIREBASE_IDP_DO_NOT_USE = [
  'O objeto firebaseConfig / google-services.json do app Web (apiKey, authDomain, storageBucket…) — não cole o arquivo inteiro.',
  'Somente o trecho "const firebaseConfig = { ... }" do SDK no frontend do seu app — extraia só projectId e webApiKey se quiser conferir, mas monte o JSON no formato acima.',
  'Credenciais OAuth do Google Cloud Console usadas em outro fluxo — não são o serviceAccount do Firebase Admin.',
]
