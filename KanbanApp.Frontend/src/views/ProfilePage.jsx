import { useState, useEffect, useRef } from 'react';
import api from '../services/api';

export default function ProfilePage() {
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);
    const [bio, setBio] = useState('');
    const [saving, setSaving] = useState(false);
    const [uploading, setUploading] = useState(false);
    const [message, setMessage] = useState(null);
    const [isError, setIsError] = useState(false);
    const fileRef = useRef();

    useEffect(() => {
        let ignore = false;
        async function fetchProfile() {
            try {
                const response = await api.get('/api/users/me');
                if (!ignore) {
                    setProfile(response.data);
                    setBio(response.data.bio || '');
                }
            } catch {
                console.error('Failed to fetch profile');
            } finally {
                if (!ignore) setLoading(false);
            }
        }
        fetchProfile();
        return () => { ignore = true; };
    }, []);

    const handleSave = async (e) => {
        e.preventDefault();
        setSaving(true);
        try {
            const response = await api.put('/api/users/me', { bio });
            setProfile(response.data);
            setMessage('Profile updated successfully');
            setIsError(false);
        } catch {
            setMessage('Failed to update profile');
            setIsError(true);
        } finally {
            setSaving(false);
            setTimeout(() => setMessage(null), 3000);
        }
    };

    const handleAvatarUpload = async (e) => {
        const file = e.target.files[0];
        if (!file) return;
        setUploading(true);
        try {
            const formData = new FormData();
            formData.append('file', file);
            const response = await api.post('/api/users/me/avatar', formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            setProfile(response.data);
            setMessage('Avatar updated');
            setIsError(false);
        } catch {
            setMessage('Failed to upload avatar');
            setIsError(true);
        } finally {
            setUploading(false);
            setTimeout(() => setMessage(null), 3000);
        }
    };

    if (loading) return <div className="loading">loading profile...</div>;
    if (!profile) return <div className="page-content"><p>Failed to load profile.</p></div>;

    const initials = profile.userName?.slice(0, 2).toUpperCase() ?? '??';

    return (
        <div className="page-content fade-in">
            <div className="page-header">
                <h1>Profile</h1>
            </div>

            <div style={{ maxWidth: '520px' }}>
                <div style={{
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border)',
                    borderRadius: 'var(--radius-lg)',
                    padding: '28px',
                    marginBottom: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '20px'
                }}>
                    <div style={{ position: 'relative', flexShrink: 0 }}>
                        <div
                            onClick={() => fileRef.current.click()}
                            style={{
                                width: '64px',
                                height: '64px',
                                borderRadius: '50%',
                                background: 'var(--accent-indigo)',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                fontSize: '22px',
                                fontWeight: '700',
                                color: '#fff',
                                cursor: 'pointer',
                                overflow: 'hidden',
                                border: '2px solid var(--border)',
                                position: 'relative'
                            }}
                        >
                            {profile.profilePictureUrl
                                ? <img src={profile.profilePictureUrl} alt="avatar" style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                                : initials
                            }
                            <div style={{
                                position: 'absolute',
                                inset: 0,
                                background: 'rgba(0,0,0,0.4)',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                opacity: 0,
                                transition: 'opacity 0.2s',
                                fontSize: '18px'
                            }}
                                 onMouseEnter={e => e.currentTarget.style.opacity = 1}
                                 onMouseLeave={e => e.currentTarget.style.opacity = 0}
                            >
                                📷
                            </div>
                        </div>
                        <input ref={fileRef} type="file" accept="image/jpeg,image/png,image/webp" style={{ display: 'none' }} onChange={handleAvatarUpload} />
                        {uploading && <div style={{ position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '10px', color: 'var(--accent-cyan)' }}>...</div>}
                    </div>
                    <div>
                        <p style={{ fontWeight: '600', fontSize: '16px', marginBottom: '4px' }}>{profile.userName}</p>
                        <p style={{ color: 'var(--text-secondary)', fontSize: '13px' }}>{profile.email}</p>
                        <span className="tag" style={{ marginTop: '6px', display: 'inline-block' }}>member</span>
                    </div>
                </div>

                <div style={{
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border)',
                    borderRadius: 'var(--radius-lg)',
                    padding: '28px'
                }}>
                    <h2 style={{ fontSize: '16px', marginBottom: '20px' }}>Edit Profile</h2>
                    {message && (
                        <p className={isError ? 'error-msg' : 'success-msg'} style={{ marginBottom: '16px' }}>
                            {message}
                        </p>
                    )}
                    <form className="auth-form" onSubmit={handleSave}>
                        <div className="form-group">
                            <label>Bio</label>
                            <textarea
                                value={bio}
                                onChange={e => setBio(e.target.value)}
                                placeholder="Tell something about yourself..."
                                rows={4}
                                style={{ resize: 'vertical' }}
                            />
                        </div>
                        <button type="submit" className="btn-primary" disabled={saving}>
                            {saving ? 'Saving...' : 'Save Changes'}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}