import './App.css';
import { Routes, Route } from 'react-router-dom';
import Login from "./Pages/Login";
import Welcome from "./Pages/Welcome";
import SessionExpired from './Pages/SessionExpired';
import Unauthenticated from "./Pages/Unauthenticated";
import Dashboard from './Pages/Dashboard';
import Register from './Pages/Register';
import NotFound from './Pages/NotFound';
import AccountConfirmation from './Pages/AccountConfirmation';
import RequestPasswordReset from './Pages/RequestPasswordReset';
import ConfirmPasswordReset from './Pages/ConfirmPasswordReset';

function App() {
    return (
        <div className="app-container">
            <Routes>
                <Route index element={<Login />} />
                <Route path="dashboard/:username" element={<Dashboard />} />
                <Route path="register" element={<Register />} />
                <Route path="welcome/:username" element={<Welcome />} />
                <Route path="sessionExpired" element={<SessionExpired />} />
                <Route path="unauthenticated" element={<Unauthenticated />} />
                <Route path="confirm-email" element={<AccountConfirmation />} />
                <Route path="request-password-reset" element={<RequestPasswordReset />} />
                <Route path="reset-password" element={<ConfirmPasswordReset />}/>
                <Route path="*" element={<NotFound />} />
            </Routes>
        </div>
    );
}

export default App;