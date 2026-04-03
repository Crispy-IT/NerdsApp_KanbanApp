import { useState } from 'react';

export default function InviteModal({ isOpen, onClose, onInvite }) {
    const [email, setEmail] = useState('');
    const [message, setMessage] = useState(null);
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);

    if (!isOpen) return null;

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!email.trim()) return;
        setLoading(true);
        const result = await onInvite(email.trim());
        setIsError(!result.success);
        setMessage(result.message);
        if (result.success) setEmail('');
        setLoading(false);
    };

    const handleClose = () => {
        setEmail('');
        setMessage(null);
        onClose();
    };

    return (
        <div className="modal-overlay" onClick={handleClose}>
            <div className="modal-box" onClick={e => e.stopPropagation()}>
                <h2>Invite Member</h2>
                <form onSubmit={handleSubmit} className="auth-form">
                    <div className="form-group">
                        <label>Email</label>
                        <input
                            type="email"
                            value={email}
                            onChange={e => setEmail(e.target.value)}
                            placeholder="teammate@example.com"
                            autoFocus
                            required
                        />
                    </div>
                    {message && (
                        <p className={isError ? 'error-msg' : 'success-msg'}>{message}</p>
                    )}
                    <div className="modal-actions">
                        <button type="submit" className="btn-primary" disabled={loading}>
                            {loading ? 'Inviting...' : 'Send Invite'}
                        </button>
                        <button type="button" className="btn-secondary" onClick={handleClose}>Cancel</button>
                    </div>
                </form>
            </div>
        </div>
    );
}