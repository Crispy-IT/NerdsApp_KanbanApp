import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import api from '../services/api';
import Column from '../components/Column';
import Card from '../components/Card';

export default function BoardView() {
    const { boardId } = useParams();
    const [board, setBoard] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let ignore = false;

        async function fetchBoard() {
            try {
                const response = await api.get(`/api/boards/${boardId}`);
                if (!ignore) setBoard(response.data);
            } catch (err) {
                console.error('Failed to fetch board', err);
            } finally {
                if (!ignore) setLoading(false);
            }
        }

        fetchBoard();
        return () => { ignore = true; };
    }, [boardId]);

    if (loading) return <p>Loading board...</p>;
    if (!board) return <p>Board not found.</p>;

    return (
        <div style={{ padding: '20px' }}>
            <h1>{board.name}</h1>
            {board.description && <p>{board.description}</p>}
            <div style={{ display: 'flex', overflowX: 'auto', paddingBottom: '10px' }}>
                {board.columns.map(column => (
                    <Column key={column.id} title={column.name}>
                        {column.cards.map(card => (
                            <Card
                                key={card.id}
                                title={card.title}
                                description={card.description}
                                assignee={card.assignedToUserId}
                            />
                        ))}
                    </Column>
                ))}
            </div>
        </div>
    );
}