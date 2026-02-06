import React, { useEffect, useState } from 'react';

const UsersPage = () => {
    const [users, setUsers] = useState([]);

    useEffect(() => {
        fetch('/api/user')  // <-- bardzo ważne: bez localhost, bez https, tylko ścieżka względna
            .then(response => {
                if (!response.ok) {
                    throw new Error('Błąd sieci!');
                }
                return response.json();
            })
            .then(data => setUsers(data))
            .catch(error => console.error('Błąd podczas pobierania danych:', error));
    }, []);

    return (
        <div>
            <h1>Lista użytkowników</h1>
            <ul>
                {users.map(user => (
                    <li key={user.id}>{user.name} ({user.email})</li>
                ))}
            </ul>
        </div>
    );
};

export default UsersPage;