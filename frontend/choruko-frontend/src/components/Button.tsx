const Button = ({ label }: { label: string }) => {
    const style: React.CSSProperties = {
        backgroundColor: '#55ad9b',
        color: '#f1f8e8',
        textAlign: 'center',
        border: 'none',
        borderRadius: '10px',
        cursor: 'pointer',
    };

  return <button style={style} className="standart-button active">{label} <style>{`.active { background-color: #ad5555; color: #f1f8e8; }`}</style></button>;
};

export default Button;
    