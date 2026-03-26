import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

export default function Dashboard() {
    const [boards, setBoards] = useState([]);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    useEffect(() => {
        let ignore = false;

        async function fetchBoards() {
            try {
                const response = await api.get('/api/boards');
                if (!ignore) setBoards(response.data);
            } catch (err) {
                console.error('Failed to fetch boards', err);
            } finally {
                if (!ignore) setLoading(false);
            }
        }

        fetchBoards();
        return () => { ignore = true; };
    }, []);

    if (loading) return <p>Loading boards...</p>;

    return (
        <div style={{ maxWidth: '600px', margin: '50px auto' }}>
            <h1>My Boards</h1>
            {boards.length === 0 ? (
                <p>You have no boards yet.</p>
            ) : (
                <ul style={{ listStyle: 'none', padding: 0 }}>
                    {boards.map(board => (
                        <li
                            key={board.id}
                            onClick={() => navigate(`/board/${board.id}`)}
                            style={{
                                padding: '15px',
                                margin: '10px 0',
                                border: '1px solid #ccc',
                                borderRadius: '8px',
                                cursor: 'pointer'
                            }}
                        >
                            <strong>{board.name}</strong>
                            {board.description && <p>{board.description}</p>}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}