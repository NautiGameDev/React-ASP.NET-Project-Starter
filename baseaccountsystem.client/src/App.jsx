import './App.css';
import { Routes, Route } from 'react-router-dom';
import Login from "./Pages/Login";
import Welcome from "./Pages/Welcome";
import SessionExpired from './Pages/SessionExpired';
import Unauthenticated from "./Pages/Unauthenticated";
import Dashboard from './Pages/Dashboard';
import Register from './Pages/Register';
import NotFound from './Pages/NotFound';

function App() {
    return (
        <div className="app-container">
            <Routes>
                <Route index element={<Login />} />
                <Route path="Dashboard/:username" element={<Dashboard />} />
                <Route path="Register" element={<Register />} />
                <Route path="Welcome/:username" element={<Welcome />} />
                <Route path="SessionExpired" element={<SessionExpired />} />
                <Route path="Unauthenticated" element={<Unauthenticated />} />
                <Route path="*" element={<NotFound />} />
            </Routes>
        </div>
    );
}

export default App;