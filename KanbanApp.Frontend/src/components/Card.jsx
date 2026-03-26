export default function Card({ title, description, assignee }) {
    const initials = assignee
        ? assignee.slice(0, 2).toUpperCase()
        : null;

    return (
        <div style={{
            backgroundColor: '#fff',
            borderRadius: '6px',
            padding: '10px',
            marginBottom: '8px',
            boxShadow: '0 1px 3px rgba(0,0,0,0.15)'
        }}>
            <strong>{title}</strong>
            {description && <p style={{ margin: '5px 0', fontSize: '13px', color: '#666' }}>{description}</p>}
            {initials && (
                <div style={{
                    width: '28px',
                    height: '28px',
                    borderRadius: '50%',
                    backgroundColor: '#4a90d9',
                    color: '#fff',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '12px',
                    marginTop: '8px'
                }}>
                    {initials}
                </div>
            )}
        </div>
    );
}