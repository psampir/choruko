import '../scss/Login.scss'
import racoon from '../../assets/svg/racoon-waving.svg'
import { useState } from 'react'  

function Login() {

const [showPassword, setShowPassword] = useState(false);
const toggleShowPassword = () => {setShowPassword(!showPassword)};

  
  return (
    <div className="login active">
      <div className="login__logo-container">
        {/* <picture>
          <source srcSet="/images/logo.webp" type="image/webp" media='(max-width: 480px)'/>
          <source srcSet="/images/logo2.webp" type="image/webp" media='(max-width: 768px)'/>
          <img className='login__logo' src="/images/logo.png" alt="Choruko Logo" />
        </picture> */}
        <img src={racoon} alt="Racoon Animated Image" />
      </div>
      <h1 className="login__title">Choruko</h1>
      <form className="login__form">
        <input className="standard__input"  type="text" placeholder="Email" />
       <div className="login__input-wrapper">
        <input
          className={`standard__input login__input--password ${showPassword ? ' active' : ''}`}
          type={showPassword ? "text" : "password"}
          placeholder="Password"
        />
        <span
          className="login__input-eye"
          onClick={toggleShowPassword}
          aria-label="Show/Hide password"
          tabIndex={0}
          role="button"
        />
      </div>
      </form>
      <button className="standard__button login__button--login">Login In</button>
      <button className="standard-additional__button login__button--reset">Reset Password</button>
      <p className="login__without-account">Don't have an account?</p>
      <button className="standard__button login__button--signup">Sign Up</button>
    </div>
  )
}

export default Login
