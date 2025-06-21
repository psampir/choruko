import './App.css'
import  Button  from './components/Button'
import Input from './components/Input'

function App() {
const style: React.CSSProperties = {
  backgroundColor: '#fff',

}
  return (
    <>
      <Button label="Click me" />
      <Input placeholder="Email" />
      <button className='active' style={style}>Click me</button>
    </>
  )
}

export default App
