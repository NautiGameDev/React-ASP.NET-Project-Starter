import { useParams, useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
import { checkLoggedIn, handleLogout } from '../services/accountService';
import { useAuth } from '../contexts/authContext';

function Dashboard() {
    const { username } = useParams() ?? "Unknown";

    const navigate = useNavigate();
    const { isLoggedIn, login, logout } = useAuth();

    const handleAuthentication = async () => {
        if (isLoggedIn) {
            return;
        }

        const response = await checkLoggedIn();

        if (response === 200) {
            login();
        }
        else {
            navigate("/Unauthenticated");
        }
    }

    const logoutUser = async () => {
        const response = await handleLogout();

        if (response.status == 200) {
            logout();
            navigate("/");
        }
        else {
            alert(`Error logging out ${response.status}: ${response.error}`);
            logout();
            navigate("/");
        }
    }

    useEffect(() => {
        handleAuthentication();
    }, []);


  return (
      <div className="page">
          <h1>Logged in as {username}</h1>
          <button type="button" onClick={() => logoutUser()}>Logout</button>
      </div>
  );
}

export default Dashboard;