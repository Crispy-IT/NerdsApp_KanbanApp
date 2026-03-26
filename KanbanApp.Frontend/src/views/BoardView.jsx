import { useParams } from 'react-router-dom';

export default function BoardView() {
    const { boardId } = useParams();

    return (
        <div>
            <h1>Board #{boardId}</h1>
            <p>Kanban view coming soon...</p>
        </div>
    );
}