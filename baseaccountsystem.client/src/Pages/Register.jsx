import { useState } from 'react';
import { handleRegistration } from '../services/accountService';
import { useNavigate } from 'react-router-dom';
function Register() {
    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");

    const [registrationError, setRegistrationError] = useState("");

    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setRegistrationError("");

        const hasCapital = /[A-Z]/;
        const hasLowercase = /[a-z]/;
        const hasNumber = /[0-9]/;
        const hasSymbol = /[^a-zA-Z0-9\s]/;
        const isEmail = /^\S+@\S+\.[a-zA-Z]{2,6}$/i;

        //Email testing
        if (email.trim() === "") {
            setRegistrationError("Email address required");
            return;
        }
        else if (!isEmail.test(email)) {
            setRegistrationError("Valid email required");
            return;
        }

        //Password testing
        if (password.trim() === "") {
            setRegistrationError("Password required");
            return;
        }
        else if (confirmPassword.trim() === "") {
            setRegistrationError("Must confirm password");
            return;
        }
        else if (password !== confirmPassword) {
            setRegistrationError("Passwords must match");
            return;
        }
        else if (password.trim().length < 8) {
            setRegistrationError("Password must be at least 8 characters long.");
            return;
        }
        else if (!hasCapital.test(password)) {
            setRegistrationError("Password must have at least one uppercase letter.");
            return;
        }
        else if (!hasLowercase.test(password)) {
            setRegistrationError("Password must have at least one lowercase letter.");
            return;
        }
        else if (!hasNumber.test(password)) {
            setRegistrationError("Password must have at least one number.");
            return;
        }
        else if (!hasSymbol.test(password)) {
            setRegistrationError("Password must have at least one special character (!@#$%^&*).");
            return;
        }

        const response = await handleRegistration(username, email, password);

        if (response.status === 201) {
            navigate(`/welcome/${username}`);
        }
        else {
            setRegistrationError(`Error ${response.status}: ${response.message}`);
        }

    }

  return (
      <div className="page">
          <form onSubmit={(e) => handleSubmit(e)} >
              <div className="form-header">
                  <h1>New Account</h1>
              </div>
              <div className="form-row">
                  {registrationError}
              </div>
              <div className="form-row">
                  <label>Username:</label>
                  <input type="text" placeholder="Enter username" value={username} onChange={(e) => (setUsername(e.target.value))} ></input>
              </div>
              <div className="form-row">
                  <label>Email:</label>
                  <input type="email" placeholder="Enter email address" value={email} onChange={(e) => (setEmail(e.target.value))} ></input>
              </div>
              <div className="form-row">
                  <label>Password:</label>
                  <input type="password" placeholder="Enter password" value={password} onChange={(e) => (setPassword(e.target.value))} ></input>
              </div>
              <div className="form-row">
                  <label>Confirm Password:</label>
                  <input type="password" placeholder="Re-enter password" value={confirmPassword} onChange={(e) => (setConfirmPassword(e.target.value))} ></input>
              </div>
              <div className="form-btn-container">
                  <button type="submit">Register</button>
              </div>
          </form>
      </div>
  );
}

export default Register;