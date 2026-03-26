import { useState } from 'react';
import Modal from 'react-modal';

Modal.setAppElement('#root');

export default function InviteModal({ isOpen, onClose, onInvite }) {
    const [email, setEmail] = useState('');
    const [message, setMessage] = useState(null);
    const [isError, setIsError] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!email.trim()) return;

        const result = await onInvite(email.trim());
        setIsError(!result.success);
        setMessage(result.message);
        if (result.success) setEmail('');
    };

    const handleClose = () => {
        setEmail('');
        setMessage(null);
        onClose();
    };

    return (
        <Modal
            isOpen={isOpen}
            onRequestClose={handleClose}
            style={{
                overlay: { backgroundColor: 'rgba(0,0,0,0.5)' },
                content: {
                    maxWidth: '400px',
                    margin: 'auto',
                    maxHeight: '250px',
                    borderRadius: '10px',
                    padding: '25px'
                }
            }}
        >
            <h2 style={{ marginTop: 0 }}>Invite Member</h2>
            <form onSubmit={handleSubmit}>
                <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="User email..."
                    autoFocus
                    style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc', boxSizing: 'border-box' }}
                />
                <div style={{ display: 'flex', gap: '8px', marginTop: '10px' }}>
                    <button type="submit" style={{ padding: '8px 16px', cursor: 'pointer' }}>Invite</button>
                    <button type="button" onClick={handleClose} style={{ padding: '8px 16px', cursor: 'pointer' }}>Cancel</button>
                </div>
            </form>
            {message && (
                <p style={{ color: isError ? 'red' : 'green', marginTop: '10px' }}>{message}</p>
            )}
        </Modal>
    );
}