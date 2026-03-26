import { useState } from 'react';
import { Droppable, Draggable } from '@hello-pangea/dnd';
import Card from './Card';

export default function Column({ column, onCreateCard }) {
    const [showForm, setShowForm] = useState(false);
    const [title, setTitle] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!title.trim()) return;
        await onCreateCard(column.id, title.trim());
        setTitle('');
        setShowForm(false);
    };

    return (
        <div style={{
            backgroundColor: '#f4f5f7',
            borderRadius: '8px',
            padding: '10px',
            minWidth: '250px',
            marginRight: '15px'
        }}>
            <h3 style={{ margin: '0 0 10px 0' }}>{column.name}</h3>
            <Droppable droppableId={String(column.id)}>
                {(provided, snapshot) => (
                    <div
                        ref={provided.innerRef}
                        {...provided.droppableProps}
                        style={{
                            minHeight: '50px',
                            backgroundColor: snapshot.isDraggingOver ? '#e2e8f0' : 'transparent',
                            borderRadius: '6px',
                            transition: 'background-color 0.2s'
                        }}
                    >
                        {column.cards.map((card, index) => (
                            <Draggable key={card.id} draggableId={String(card.id)} index={index}>
                                {(provided) => (
                                    <div
                                        ref={provided.innerRef}
                                        {...provided.draggableProps}
                                        {...provided.dragHandleProps}
                                    >
                                        <Card
                                            title={card.title}
                                            description={card.description}
                                            assignee={card.assignedToUserId}
                                        />
                                    </div>
                                )}
                            </Draggable>
                        ))}
                        {provided.placeholder}
                    </div>
                )}
            </Droppable>

            {showForm ? (
                <form onSubmit={handleSubmit} style={{ marginTop: '8px' }}>
                    <input
                        type="text"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        placeholder="Card title..."
                        autoFocus
                        style={{ width: '100%', padding: '6px', borderRadius: '4px', border: '1px solid #ccc', boxSizing: 'border-box' }}
                    />
                    <div style={{ display: 'flex', gap: '5px', marginTop: '5px' }}>
                        <button type="submit" style={{ padding: '5px 12px', cursor: 'pointer' }}>Add</button>
                        <button type="button" onClick={() => { setShowForm(false); setTitle(''); }} style={{ padding: '5px 12px', cursor: 'pointer' }}>Cancel</button>
                    </div>
                </form>
            ) : (
                <button
                    onClick={() => setShowForm(true)}
                    style={{
                        marginTop: '8px',
                        width: '100%',
                        padding: '6px',
                        background: 'none',
                        border: 'none',
                        color: '#888',
                        cursor: 'pointer',
                        textAlign: 'left'
                    }}
                >
                    + Create Card
                </button>
            )}
        </div>
    );
}