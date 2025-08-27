import './boot/detectNotch';
import { StrictMode, useState } from 'react'
import { createRoot } from 'react-dom/client'
import './index.scss'

import NavbarMobile from './components/NavbarMobile/NavbarMobile'
import Popup from './components/Popup'
import Input from './components/Input'
import Button from './components/Button'

function App() {
  const [isPopupOpen, setPopupOpen] = useState(true);

  return (
    <>
      <NavbarMobile />
      <Popup variant="red" title="Error" open={isPopupOpen} onClose={() => setPopupOpen(false)}>
        <Input
          variant="email"
          className="login__input--email"
          type="text"
          placeholder="Email"
        />
        <Button variant="red" onClick={() => setPopupOpen(false)}>Submit</Button>
      </Popup>
    </>
  );
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)