import { useSearchParams, useNavigate } from 'react-router-dom';
import { confirmPasswordReset } from '../services/accountService';
import { useState } from 'react';
function ConfirmPasswordReset() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const userId = searchParams.get('userId');
    const code = searchParams.get('code');

    const [sysMessage, setSysMessage] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");

    const handleSubmit = async (e) => {
        e.preventDefault();

        setSysMessage("");
        setErrorMessage("");

        const hasCapital = /[A-Z]/;
        const hasLowercase = /[a-z]/;
        const hasNumber = /[0-9]/;
        const hasSymbol = /[^a-zA-Z0-9\s]/;

        if (password.trim() === "") {
            setErrorMessage("Password required");
            return;
        }
        else if (confirmPassword.trim() === "") {
            setErrorMessage("Must confirm password");
            return;
        }
        else if (password !== confirmPassword) {
            setErrorMessage("Passwords must match");
            return;
        }
        else if (password.trim().length < 8) {
            setErrorMessage("Password must be at least 8 characters long.");
            return;
        }
        else if (!hasCapital.test(password)) {
            setErrorMessage("Password must have at least one uppercase letter.");
            return;
        }
        else if (!hasLowercase.test(password)) {
            setErrorMessage("Password must have at least one lowercase letter.");
            return;
        }
        else if (!hasNumber.test(password)) {
            setErrorMessage("Password must have at least one number.");
            return;
        }
        else if (!hasSymbol.test(password)) {
            setErrorMessage("Password must have at least one special character (!@#$%^&*).");
            return;
        }

        const response = await confirmPasswordReset(userId, code, password);

        if (response.status === 200) {
            setSysMessage("Password reset successful. Redirecting to login page.");
            setTimeout(() => {
                navigate("/")
            }, 3000);
        }
        else {
            setSysMessage(`Error ${response.status}: ${response.errorMessage}.`);
        }
    }

  return (
      <div className="page">
          {sysMessage.trim() === "" ? (
              <form onSubmit={(e) => handleSubmit(e)} >
                  <div className="form-header">
                    <h1>Enter new password</h1>
                  </div>
                  <div className="form-row">
                      {errorMessage}
                  </div>
                  <div className="form-row">
                      <label htmlFor="password">New Password:</label>
                      <input type="password" id="password" value={password} placeholder="Enter new password" onChange={(e) => setPassword(e.target.value)}></input>
                  </div>
                  <div className="form-row">
                      <label htmlFor="confirm-password">Confirm Password:</label>
                      <input type="password" id="confirm-password" value={confirmPassword} placeholder="Re-enter new password" onChange={(e) => setConfirmPassword(e.target.value)}></input>
                  </div>
                  <div className="form-btn-container">
                    <button type="submit">Submit</button>
                  </div>

              </form>
          ): (
                  <h2>{ sysMessage }</h2>
              )}
      </div>
  );
}

export default ConfirmPasswordReset;