import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.scss'
import AuthContainer from './views/Auth/AuthContainer'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthContainer />
  </StrictMode>,
)
