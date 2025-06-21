import './Login.css'

function Login() {
// const style: React.CSSProperties = {
//   backgroundColor: '#000',
// }
  return (
    <>
      <div className='login-page__container'>
        <div className='logo__container'>
          {/* <picture>
            <source srcSet="/images/logo.webp" type="image/webp" media='(max-width: 480px)'/>
            <source srcSet="/images/logo2.webp" type="image/webp" media='(max-width: 768px)'/>
            <img className='logo' src="/images/logo.png" alt="Choruko Logo" />
          </picture> */}
        </div>
        <h1 className='login-h1'>Choruko</h1>
        <form action="submit">
          <input className='standart-input' type="text" name="" id="" placeholder='Email'/>
          <input className='standart-input' type="text" name="" id="" placeholder='Password'/>
        </form>
        <button className='standart-button login-in'>Login In</button>
        <button className='standart-additional-button reset-password'>Reset Password</button>
        <p className='without-account'>Don't have an account?</p>
        <button className='standart-button sign-up'>Sign Up</button>
      </div>
    </>  )
}

export default Login
