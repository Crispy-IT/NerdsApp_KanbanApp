import { createContext, useContext, useState } from 'react';

const TopbarContext = createContext(null);

export function TopbarProvider({ children }) {
    const [title, setTitle] = useState('');
    const [actions, setActions] = useState(null);
    return (
        <TopbarContext.Provider value={{ title, setTitle, actions, setActions }}>
            {children}
        </TopbarContext.Provider>
    );
}

export function useTopbar() {
    return useContext(TopbarContext);
}