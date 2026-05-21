import { Alert, Box, Link, Stack, Typography } from '@mui/material'
import {
  FIREBASE_IDP_CONFIG_EXAMPLE,
  FIREBASE_IDP_DO_NOT_USE,
  FIREBASE_IDP_FIELD_ROWS,
} from '../../content/firebaseIdpConfig'

export function FirebaseConfigHelp() {
  return (
    <Stack spacing={2} sx={{ width: '100%' }}>
      <Alert severity="warning">
        <Typography variant="subtitle2" gutterBottom>
          Dois JSONs diferentes no Firebase — use só estes 3 campos
        </Typography>
        <Typography variant="body2" component="div">
          No Console você vê (1) configuração do app Web e (2) chave da conta de serviço. Para o IdP
          Platform, monte <strong>um único JSON</strong> com <code>projectId</code>, <code>webApiKey</code>,{' '}
          <code>authDomain</code> (do <code>firebaseConfig</code> — necessário para o popup Google) e{' '}
          <code>serviceAccount</code> (arquivo da conta de serviço aninhado). Antes disso, em{' '}
          <strong>Authentication → Sign-in method</strong>, habilite o provedor <strong>Google</strong>.
        </Typography>
      </Alert>

      <Box component="table" sx={{ width: '100%', fontSize: '0.85rem', borderCollapse: 'collapse' }}>
        <Box component="thead">
          <Box component="tr" sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Box component="th" sx={{ textAlign: 'left', py: 0.75, pr: 1 }}>
              Campo no ConfigJson
            </Box>
            <Box component="th" sx={{ textAlign: 'left', py: 0.75, pr: 1 }}>
              Onde pegar no Firebase Console
            </Box>
            <Box component="th" sx={{ textAlign: 'left', py: 0.75 }}>
              Observação
            </Box>
          </Box>
        </Box>
        <Box component="tbody">
          {FIREBASE_IDP_FIELD_ROWS.map((row) => (
            <Box component="tr" key={row.field} sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Box component="td" sx={{ py: 1, pr: 1, fontFamily: 'monospace', verticalAlign: 'top' }}>
                {row.field}
              </Box>
              <Box component="td" sx={{ py: 1, pr: 1, verticalAlign: 'top' }}>
                {row.where}
              </Box>
              <Box component="td" sx={{ py: 1, color: 'text.secondary', verticalAlign: 'top' }}>
                {row.notes}
              </Box>
            </Box>
          ))}
        </Box>
      </Box>

      <Alert severity="error" variant="outlined">
        <Typography variant="subtitle2" gutterBottom>
          Não use
        </Typography>
        <Box component="ul" sx={{ m: 0, pl: 2.5 }}>
          {FIREBASE_IDP_DO_NOT_USE.map((item) => (
            <Typography key={item} component="li" variant="body2">
              {item}
            </Typography>
          ))}
        </Box>
      </Alert>

      <Typography variant="subtitle2">Modelo pronto (edite e cole no campo abaixo)</Typography>
      <Box
        component="pre"
        sx={{
          m: 0,
          p: 2,
          borderRadius: 1,
          bgcolor: 'action.hover',
          overflow: 'auto',
          fontSize: '0.75rem',
          lineHeight: 1.45,
          fontFamily: 'ui-monospace, monospace',
          whiteSpace: 'pre',
        }}
      >
        {FIREBASE_IDP_CONFIG_EXAMPLE}
      </Box>

      <Typography variant="caption" color="text.secondary">
        Dica: abra o arquivo <code>*-firebase-adminsdk-*.json</code> baixado e copie todo o objeto para dentro de{' '}
        <code>&quot;serviceAccount&quot;: &#123; ... &#125;</code>. O <code>project_id</code> dentro do arquivo deve
        coincidir com <code>projectId</code> na raiz.
        {' '}
        <Link href="https://console.firebase.google.com/" target="_blank" rel="noopener">
          Abrir Firebase Console
        </Link>
      </Typography>
    </Stack>
  )
}
