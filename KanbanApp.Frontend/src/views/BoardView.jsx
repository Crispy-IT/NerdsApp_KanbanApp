import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { DragDropContext } from '@hello-pangea/dnd';
import api from '../services/api';
import Column from '../components/Column';

export default function BoardView() {
    const { boardId } = useParams();
    const [board, setBoard] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

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

    const handleDragEnd = async (result) => {
        const { source, destination, draggableId } = result;

        if (!destination) return;
        if (source.droppableId === destination.droppableId && source.index === destination.index) return;

        const cardId = parseInt(draggableId);
        const sourceColumnId = parseInt(source.droppableId);
        const destColumnId = parseInt(destination.droppableId);

        // save previous state for rollback
        const previousBoard = JSON.parse(JSON.stringify(board));

        // optimistic update - move card immediately
        setBoard(prev => {
            const updated = JSON.parse(JSON.stringify(prev));
            const sourceCol = updated.columns.find(c => c.id === sourceColumnId);
            const destCol = updated.columns.find(c => c.id === destColumnId);

            const cardIndex = sourceCol.cards.findIndex(c => c.id === cardId);
            const [movedCard] = sourceCol.cards.splice(cardIndex, 1);
            destCol.cards.splice(destination.index, 0, movedCard);

            return updated;
        });

        // find card data for API call
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
            // revert on failure
            setBoard(previousBoard);
            setError('Failed to move card');
            setTimeout(() => setError(null), 3000);
        }
    };

    if (loading) return <p>Loading board...</p>;
    if (!board) return <p>Board not found.</p>;

    return (
        <div style={{ padding: '20px' }}>
            <h1>{board.name}</h1>
            {board.description && <p>{board.description}</p>}
            {error && <p style={{ color: 'red' }}>{error}</p>}
            <DragDropContext onDragEnd={handleDragEnd}>
                <div style={{ display: 'flex', overflowX: 'auto', paddingBottom: '10px' }}>
                    {board.columns.map(column => (
                        <Column key={column.id} column={column} />
                    ))}
                </div>
            </DragDropContext>
        </div>
    );
}