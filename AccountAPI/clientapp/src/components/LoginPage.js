import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const LoginPage = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleSubmit = (e) => {
        e.preventDefault();

        // Prosta walidacja formularza
        if (!email || !password) {
            setError('Wszystkie pola są wymagane');
            return;
        }

        // Tu mogłaby być logika wysyłania danych do API logowania
        // np. POST /api/login
        // Przykładowa symulacja poprawnego logowania:
        if (email === 'test@example.com' && password === 'password123') {
            setError('');
            navigate('/users'); // Przekierowanie do strony z użytkownikami po poprawnym logowaniu
        } else {
            setError('Niepoprawny email lub hasło');
        }
    };

    return (
        <div>
            <h1>Logowanie</h1>
            <form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="email">Email:</label>
                    <input
                        type="email"
                        id="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                </div>
                <div>
                    <label htmlFor="password">Hasło:</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                {error && <div style={{ color: 'red' }}>{error}</div>}
                <div>
                    <button type="submit">Zaloguj się</button>
                </div>
            </form>
        </div>
    );
};

export default LoginPage;
