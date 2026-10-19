import { useState } from 'react';
import { handleLogin } from '../services/accountService';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from "../contexts/authContext";

function Login() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");


    const [loginError, setLoginError] = useState("");

    const navigate = useNavigate();
    const { login } = useAuth();

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (username.trim() === "") {
            setLoginError("Username required");
            return;
        }
        else if (password.trim() === "") {
            setLoginError("Password required");
            return;
        }

        const response = await handleLogin(username, password);

        if (response.status === 200) {
            login();
            navigate(`/Dashboard/${username}`);
        }
        else {
            setLoginError(`Error ${response.status}: ${response.message}`);
        }
    }

  return (
      <div className="page">           
          <form onSubmit={(e) => handleSubmit(e)} >
              <div className="form-header">
                  <h1>Login</h1>
              </div>
              <div className="form-row">
                  {loginError}
              </div>
              <div className="form-row">
                  <label>Username:</label>
                  <input type="text" placeholder="Enter username" value={username} onChange={(e) => (setUsername(e.target.value))} ></input>
              </div>
              <div className="form-row">
                  <label>Password:</label>
                  <input type="password" placeholder="Enter password" value={password} onChange={(e) => (setPassword(e.target.value))} ></input>
              </div>
              <div className="form-btn-container">
                  <button type="submit">Login</button>
                <Link to="/Register/">Create Account</Link>
              </div>
          </form>
      </div>
  );
}

export default Login;