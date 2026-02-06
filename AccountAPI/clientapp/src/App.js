import React from 'react';
import { Routes, Route, Link } from 'react-router-dom';
import UsersPage from './components/UsersPage'; // Zaktualizowany import
import LoginPage from './components/LoginPage'; // Zaktualizowany import
import './App.css';

function App() {
    return (
        <div className="App">
            <header className="App-header">
                <h1>Account APP</h1>
                <nav>
                    <ul>
                        <li>
                            <Link to="/login">Logowanie Uzytkownika</Link>
                        </li>
                        <li>
                            <Link to="/users">Logowanie Administratora</Link>
                        </li>
                    </ul>
                </nav>
            </header>
            <main>
                <Routes>
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/users" element={<UsersPage />} />
                </Routes>
            </main>
        </div>
    );
}

export default App;
