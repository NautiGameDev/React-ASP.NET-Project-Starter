import { useSearchParams, useNavigate } from 'react-router-dom';
import { confirmEmail } from '../services/accountService';
import { useState } from 'react';

function AccountConfirmation() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const userId = searchParams.get('userId');
    const code = searchParams.get('code');

    const [sysMessage, setSysMessage] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);
    

    const handleConfirmation = async () => {
        if (!userId || !code) {
            setSysMessage("Invalid confirmation link. Missing user ID or code.");
            return;
        }

        setIsSubmitting(true);
        setSysMessage("");

        const response = await confirmEmail(userId, code);

        if (response.status === 200) {
            setSysMessage("Email confirmed. Redirecting to login...");

            setTimeout(() => {
                navigate("/");
            }, 3000);
        }
        else {
            setSysMessage(`Error ${response.status}: ${response.message}`);
            setIsSubmitting(false);
        }
    }

  return (
      <div className="page">
          <h2>Confirm Account</h2>
          <p>Click the button below to finish activating your account.</p>

          <button type="button" onClick={handleConfirmation} disabled={isSubmitting}>
              {isSubmitting ? "Confirming..." : "Confirm Email"}
          </button>

          {sysMessage && <p className="sys-message">{sysMessage}</p>}
      </div>
  );
}

export default AccountConfirmation;