import { useNavigate } from 'react-router-dom';
import { useEffect } from 'react';

function SessionExpired() {
    const navigate = useNavigate();

    const delayTransition = () => {
        setTimeout(() => {
            navigate("/");
        }, 3000);
    };

    useEffect(() => {
        delayTransition();
    }, []);

  return (
      <div className="page">
          <h1>Session expired. Returning to login page...</h1>
      </div>
  );
}

export default SessionExpired;