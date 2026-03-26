import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { DragDropContext } from '@hello-pangea/dnd';
import api from '../services/api';
import Column from '../components/Column';
import InviteModal from '../components/InviteModal';

export default function BoardView() {
    const { boardId } = useParams();
    const [board, setBoard] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [showInvite, setShowInvite] = useState(false);

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

    const handleCreateCard = async (columnId, title) => {
        try {
            const response = await api.post(`/api/boards/${boardId}/cards`, { title, columnId });
            setBoard(prev => {
                const updated = JSON.parse(JSON.stringify(prev));
                const col = updated.columns.find(c => c.id === columnId);
                col.cards.push(response.data);
                return updated;
            });
            setError(null);
        } catch {
            setError('Failed to create card');
            setTimeout(() => setError(null), 3000);
        }
    };

    const handleInvite = async (email) => {
        try {
            await api.post(`/api/boards/${boardId}/members`, { email });
            return { success: true, message: 'User invited successfully!' };
        } catch (err) {
            const msg = err.response?.data || 'Failed to invite user';
            return { success: false, message: msg };
        }
    };

    const handleDragEnd = async (result) => {
        const { source, destination, draggableId } = result;

        if (!destination) return;
        if (source.droppableId === destination.droppableId && source.index === destination.index) return;

        const cardId = parseInt(draggableId);
        const sourceColumnId = parseInt(source.droppableId);
        const destColumnId = parseInt(destination.droppableId);

        const previousBoard = JSON.parse(JSON.stringify(board));

        setBoard(prev => {
            const updated = JSON.parse(JSON.stringify(prev));
            const sourceCol = updated.columns.find(c => c.id === sourceColumnId);
            const destCol = updated.columns.find(c => c.id === destColumnId);

            const cardIndex = sourceCol.cards.findIndex(c => c.id === cardId);
            const [movedCard] = sourceCol.cards.splice(cardIndex, 1);
            destCol.cards.splice(destination.index, 0, movedCard);

            return updated;
        });

        const card = previousBoard.columns
            .find(c => c.id === sourceColumnId)
            .cards.find(c => c.id === cardId);

        try {
            await api.put(`/api/boards/${boardId}/cards/${cardId}`, {
                title: card.title,
                description: card.description,
                columnId: destColumnId
            });
            setError(null);
        } catch {
            setBoard(previousBoard);
            setError('Failed to move card');
            setTimeout(() => setError(null), 3000);
        }
    };

    if (loading) return <p>Loading board...</p>;
    if (!board) return <p>Board not found.</p>;

    return (
        <div style={{ padding: '20px' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '15px', marginBottom: '10px' }}>
                <h1 style={{ margin: 0 }}>{board.name}</h1>
                <button
                    onClick={() => setShowInvite(true)}
                    style={{ padding: '8px 16px', cursor: 'pointer', borderRadius: '6px' }}
                >
                    👥 Invite
                </button>
            </div>
            {board.description && <p>{board.description}</p>}
            {error && <p style={{ color: 'red' }}>{error}</p>}
            <DragDropContext onDragEnd={handleDragEnd}>
                <div style={{ display: 'flex', overflowX: 'auto', paddingBottom: '10px' }}>
                    {board.columns.map(column => (
                        <Column key={column.id} column={column} onCreateCard={handleCreateCard} />
                    ))}
                </div>
            </DragDropContext>
            <InviteModal
                isOpen={showInvite}
                onClose={() => setShowInvite(false)}
                onInvite={handleInvite}
            />
        </div>
    );
}