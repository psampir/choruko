const Input = ({ placeholder }: { placeholder: string }) => {
  const style: React.CSSProperties = {
    backgroundColor: '#d8efd3',
    color: '#55ad9b',
    textAlign: 'center',
    border: 'solid 2px #55ad9b',
    borderRadius: '10px',
    cursor: 'text',

  };

  return <input style={style} className="standard-input" placeholder={placeholder} />;
};

export default Input;
