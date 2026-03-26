import React from 'react';
import ReactDOM from 'react-dom/client';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import LoginPage from './views/LoginPage';
import RegisterPage from './views/RegisterPage';
import Dashboard from './views/Dashboard';
import BoardView from './views/BoardView';
import ProtectedRoute from './components/ProtectedRoute';

const router = createBrowserRouter([
    { path: '/login', element: <LoginPage /> },
    { path: '/register', element: <RegisterPage /> },
    { path: '/dashboard', element: <ProtectedRoute><Dashboard /></ProtectedRoute> },
    { path: '/board/:boardId', element: <ProtectedRoute><BoardView /></ProtectedRoute> },
    { path: '/', element: <LoginPage /> },
]);

ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <RouterProvider router={router} />
    </React.StrictMode>
);