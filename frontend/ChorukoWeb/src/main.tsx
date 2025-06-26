import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import './index.scss'
import Login from './components/tsx/Login.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Login />
  </StrictMode>,
)
