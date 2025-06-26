import '../scss/Login.scss'
import raccoon from '../../assets/svg/raccoon-waving.svg'
import { useState } from 'react'  

function Login() {

const [showPassword, setShowPassword] = useState(false);
const [resetPaassword, setResetPassword] = useState(false);

const toggleShowPassword = () => {setShowPassword(!showPassword)};
const resetPasswordView = () => {setResetPassword(!resetPaassword);}

  
  return (
    <div className="login active">
      <div className="login__top-container">
        <div className="login__logo-container">
          {/* <picture>
            <source srcSet="/images/logo.webp" type="image/webp" media='(max-width: 480px)'/>
            <source srcSet="/images/logo2.webp" type="image/webp" media='(max-width: 768px)'/>
            <img className='login__logo' src="/images/logo.png" alt="Choruko Logo" />
          </picture> */}
          <img src={raccoon} alt="Racoon Animated Image"  className="login__raccoon" />
        </div>
        <h1 className="login__title">Choruko</h1>
        <form className="login__form">
          <input className="standard__input"  type="text" placeholder="Email" />
        <div className="login__input-wrapper">
          <input
            className={`standard__input login__input--password ${showPassword ? ' active' : ''}` }
            type={showPassword ? "text" : "password"}
            placeholder="Password"
            style={{display: resetPaassword ? 'none' : 'block'}}
          />
          <span
            className="login__input-eye"
            onClick={toggleShowPassword}
            aria-label="Show/Hide password"
            tabIndex={0}
            role="button"
            style={{display: resetPaassword ? 'none' : 'block'}}
          />
        </div>
        </form>
        <button className="standard__button login__button--login" style={{marginTop: resetPaassword ? '0' : ''}}>{resetPaassword ? 'Reset Password' : 'Login In'}</button>
        <button className="standard-additional__button login__button--reset" onClick={resetPasswordView}> {resetPaassword ? 'Go back to Login' : 'Reset Password'}</button>
      </div>

      <div className="login__bottom-container">
        <p className="login__without-account">Don't have an account?</p>
        <button className="standard__button login__button--signup">Sign Up</button>
      </div>
      
    </div>
  )
}

export default Login
