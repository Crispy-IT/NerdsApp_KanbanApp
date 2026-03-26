export default function Column({ title, children }) {
    return (
        <div style={{
            backgroundColor: '#f4f5f7',
            borderRadius: '8px',
            padding: '10px',
            minWidth: '250px',
            marginRight: '15px'
        }}>
            <h3 style={{ margin: '0 0 10px 0' }}>{title}</h3>
            <div>{children}</div>
        </div>
    );
}