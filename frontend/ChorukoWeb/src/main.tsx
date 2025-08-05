import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.scss'
// import AuthContainer from './views/Auth/AuthContainer'
// import JoinOrCreateHousehold from './views/JoinOrCreateHousehold/JoinOrCreateHouseholdOptions'
// import Invitations from './views/JoinOrCreateHousehold/Invitations/Invitations'
import Popup from './components/Popup'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    {/* <AuthContainer />, */}
    {/* <JoinOrCreateHousehold onJoinHousehold={() => {}} onCreateHousehold={() => {}} /> */}
    <Popup variant="primary"/>    
    {/* <Popup variant="red">This is a red popup</Popup> */}
    {/* <Invitations /> */}
  </StrictMode>,
)
