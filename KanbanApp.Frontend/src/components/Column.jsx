import { Droppable, Draggable } from '@hello-pangea/dnd';
import Card from './Card';

export default function Column({ column }) {
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
        </div>
    );
}